using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;
using System.Security.Cryptography;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.AuditReviewer)]
public class SendDistributionEmail : IRequest<Unit>
{
    public int AuditId { get; set; }
    public string SentBy { get; set; } = null!;
    public List<int> AttachmentIds { get; set; } = new();
    public string? SubjectOverride { get; set; }
    public bool IncludeCorrectiveActions { get; set; }
    public bool IncludeOpenCasOnly { get; set; }
    public string? Message { get; set; }
    public bool IncludePdf { get; set; }
    /// <summary>Routing recipients to skip for this send (ephemeral — not persisted).</summary>
    public List<string> ExcludedEmails { get; set; } = new();
}

public class SendDistributionEmailHandler : IRequestHandler<SendDistributionEmail, Unit>
{
    private readonly AppDbContext _context;
    private readonly IEmailService _email;
    private readonly IPdfGeneratorService _pdf;
    private readonly IConfiguration _config;
    private readonly ILogger<SendDistributionEmailHandler> _log;

    public SendDistributionEmailHandler(
        AppDbContext context,
        IEmailService email,
        IPdfGeneratorService pdf,
        IConfiguration config,
        ILogger<SendDistributionEmailHandler> log)
    {
        _context = context;
        _email = email;
        _pdf = pdf;
        _config = config;
        _log = log;
    }

    public async Task<Unit> Handle(SendDistributionEmail request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .Include(a => a.Division)
            .Include(a => a.Header)
            .Include(a => a.Findings).ThenInclude(f => f.CorrectiveActions)
            .Include(a => a.Responses)
            .Include(a => a.Attachments)
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, cancellationToken)
            ?? throw new KeyNotFoundException($"Audit {request.AuditId} not found.");

        // Audit must be Approved (or already Distributed for resends) before distribution can be sent
        if (audit.Status != "Approved" && audit.Status != "Distributed")
            throw new InvalidOperationException(
                $"Audit must be Approved before sending distribution. Current status: '{audit.Status}'.");

        // ── Collect recipients ────────────────────────────────────────────────
        var divisionRecipients = await _context.EmailRoutingRules
            .Where(r => r.DivisionId == audit.DivisionId && r.IsActive)
            .Select(r => r.EmailAddress)
            .ToListAsync(cancellationToken);

        var perAuditRecipients = await _context.AuditDistributionRecipients
            .Where(r => r.AuditId == request.AuditId)
            .Select(r => r.EmailAddress)
            .ToListAsync(cancellationToken);

        var excluded = new HashSet<string>(request.ExcludedEmails, StringComparer.OrdinalIgnoreCase);
        var allRecipients = divisionRecipients.Union(perAuditRecipients)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(r => !excluded.Contains(r))
            .ToList();
        if (allRecipients.Count == 0)
            throw new InvalidOperationException(
                "Cannot distribute: no recipients are configured. Add distribution recipients before sending.");

        await SendEmailAsync(audit, allRecipients, request.AttachmentIds, request.SubjectOverride,
            request.IncludeCorrectiveActions, request.IncludeOpenCasOnly, request.Message,
            request.IncludePdf, cancellationToken);

        // ── Mark distributed only after a successful send ─────────────────────
        var now = DateTime.UtcNow;
        audit.ReviewedAt = now;
        audit.ReviewedBy = request.SentBy;
        if (audit.Status == "Approved")
            audit.Status = "Distributed";
        audit.UpdatedAt = now;
        audit.UpdatedBy = request.SentBy;
        await _context.SaveChangesAsync(cancellationToken);

        // ── In-app notification to the auditor ────────────────────────────────
        if (!string.IsNullOrWhiteSpace(audit.CreatedBy))
        {
            try
            {
                _context.AppNotifications.Add(new AppNotification
                {
                    RecipientEmail = audit.CreatedBy,
                    Type           = "AuditDistributed",
                    Title          = "Audit Distributed",
                    Body           = $"Audit {audit.TrackingNumber ?? $"#{audit.Id}"} has been distributed to stakeholders.",
                    EntityType     = "Audit",
                    EntityId       = audit.Id,
                    LinkUrl        = $"/audit-management/audits/{audit.Id}/review",
                    CreatedAt      = now,
                });
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _log.LogWarning("SendDistributionEmail: failed to create in-app notification for audit {AuditId}: {Error}", request.AuditId, ex.Message);
            }
        }

        return Unit.Value;
    }

    private async Task SendEmailAsync(
        Data.Models.Audit.Audit audit,
        List<string> recipients,
        List<int> attachmentIds,
        string? subjectOverride,
        bool includeCorrectiveActions,
        bool includeOpenCasOnly,
        string? message,
        bool includePdf,
        CancellationToken ct)
    {
            var header = audit.Header;
            var divCode = audit.Division?.Code ?? "—";
            var auditDate = header?.AuditDate?.ToString("MM/dd/yyyy") ?? (audit.SubmittedAt?.ToString("MM/dd/yyyy") ?? "—");
            var location = header?.Location ?? "—";
            var auditor = header?.Auditor ?? "—";
            var pm = header?.PM ?? "—";
            var client = header?.Client ?? "—";
            var jobNumber = header?.JobNumber ?? "—";

            // Score
            var responses = audit.Responses.Where(r => r.Status != null).ToList();
            var conforming = responses.Count(r => r.Status == "Conforming");
            var denom = responses.Count(r => r.Status is "Conforming" or "NonConforming" or "Warning");
            var scoreText = denom > 0 ? $"{Math.Round((double)conforming / denom * 100, 1)}%" : "N/A";

            var appBaseUrl = _config.GetValue<string>("App:BaseUrl") ?? "http://localhost:7220";
            var reviewLink = $"{appBaseUrl}/audits/{audit.Id}/review";

            var subject = !string.IsNullOrWhiteSpace(subjectOverride)
                ? subjectOverride
                : $"[{divCode}] Compliance Audit Distribution — {auditDate}";

            // ── Review summary block (message param takes priority over saved summary) ─
            var effectiveSummary = !string.IsNullOrWhiteSpace(message) ? message : audit.ReviewSummary;
            var summaryBlock = !string.IsNullOrWhiteSpace(effectiveSummary)
                ? $"""
                  <div style="margin:20px 0;padding:14px 16px;background:#f8fafc;border-left:4px solid #1e3a5f;border-radius:0 4px 4px 0;">
                    <p style="margin:0 0 6px 0;font-size:12px;font-weight:bold;color:#1e3a5f;text-transform:uppercase;letter-spacing:0.5px;">Findings Summary</p>
                    <p style="margin:0;color:#1e293b;font-size:14px;line-height:1.7;white-space:pre-wrap;">{System.Net.WebUtility.HtmlEncode(effectiveSummary)}</p>
                  </div>
                  """
                : string.Empty;

            // ── Non-conforming findings block ─────────────────────────────────
            var findings = audit.Findings.Where(f => !f.IsDeleted).OrderBy(f => f.Id).ToList();
            string findingsBlock;
            if (findings.Count == 0)
            {
                findingsBlock = "<p style=\"color:#16a34a;font-size:14px;\">No non-conforming findings.</p>";
            }
            else
            {
                var rows = string.Join("", findings.Select((f, i) =>
                {
                    string caRows;
                    if (!includeCorrectiveActions)
                    {
                        caRows = string.Empty;
                    }
                    else
                    {
                        var cas = f.CorrectiveActions
                            .Where(ca => !ca.IsDeleted && (!includeOpenCasOnly || (ca.Status != "Closed" && ca.Status != "Voided")))
                            .OrderBy(ca => ca.CreatedAt)
                            .ToList();
                        caRows = cas.Count == 0
                            ? "<tr><td colspan=\"3\" style=\"padding:4px 8px;color:#94a3b8;font-size:12px;\">No corrective actions assigned</td></tr>"
                            : string.Join("", cas.Select(ca =>
                                $"<tr>" +
                                $"<td style=\"padding:4px 8px 4px 24px;font-size:12px;color:#475569;\">{System.Net.WebUtility.HtmlEncode(ca.Description ?? "—")}</td>" +
                                $"<td style=\"padding:4px 8px;font-size:12px;color:#475569;\">{System.Net.WebUtility.HtmlEncode(ca.AssignedTo ?? "Unassigned")}</td>" +
                                $"<td style=\"padding:4px 8px;font-size:12px;color:#475569;\">{ca.DueDate?.ToString("MM/dd/yyyy") ?? "—"}</td>" +
                                $"</tr>"));
                    }

                    return $"""
                        <tr style="background:{((i % 2 == 0) ? "#fff" : "#f8fafc")}">
                          <td colspan="3" style="padding:8px 8px 4px;font-size:13px;font-weight:600;color:#dc2626;">
                            {i + 1}. {System.Net.WebUtility.HtmlEncode(f.QuestionTextSnapshot ?? "—")}
                            {(f.CorrectedOnSite ? "<span style=\"margin-left:8px;font-size:11px;color:#16a34a;font-weight:normal;\">[Corrected On-Site]</span>" : "")}
                          </td>
                        </tr>
                        {caRows}
                        """;
                }));

                var caHeaders = includeCorrectiveActions
                    ? """
                      <th style="padding:8px;color:#fff;text-align:left;font-size:12px;">Assigned To</th>
                      <th style="padding:8px;color:#fff;text-align:left;font-size:12px;">Due Date</th>
                      """
                    : string.Empty;

                findingsBlock = $"""
                    <table style="width:100%;border-collapse:collapse;font-size:13px;">
                      <thead>
                        <tr style="background:#1e3a5f;">
                          <th style="padding:8px;color:#fff;text-align:left;font-size:12px;">Finding</th>
                          {caHeaders}
                        </tr>
                      </thead>
                      <tbody>{rows}</tbody>
                    </table>
                    """;
            }

            // ── Selected attachments list (names only for dry-run safety) ─────
            var selectedAttachments = audit.Attachments
                .Where(a => attachmentIds.Contains(a.Id) && !a.IsDeleted)
                .ToList();

            var attachmentBlock = selectedAttachments.Count > 0
                ? $"""
                  <div style="margin-top:16px;">
                    <p style="margin:0 0 6px;font-size:12px;font-weight:bold;color:#334155;text-transform:uppercase;letter-spacing:0.5px;">Attachments Included ({selectedAttachments.Count})</p>
                    <ul style="margin:0;padding-left:20px;">{string.Join("", selectedAttachments.Select(a => $"<li style=\"font-size:13px;color:#475569;\">{System.Net.WebUtility.HtmlEncode(a.FileName)}</li>"))}</ul>
                  </div>
                  """
                : string.Empty;

            var body = $"""
                <div style="font-family:Arial,sans-serif;max-width:640px;margin:0 auto;">
                  <div style="background:#1e3a5f;padding:20px;border-radius:4px 4px 0 0;">
                    <h2 style="color:#fff;margin:0;">{divCode} — Compliance Audit Distribution</h2>
                    <p style="color:#93c5fd;margin:6px 0 0;font-size:13px;">{auditDate}</p>
                  </div>
                  <div style="border:1px solid #ddd;border-top:none;padding:20px;border-radius:0 0 4px 4px;">
                    <table style="width:100%;border-collapse:collapse;margin-bottom:16px;">
                      <tr><td style="padding:5px 0;color:#64748b;width:140px;font-size:13px;">Division</td><td style="padding:5px 0;font-weight:bold;font-size:13px;">{divCode}</td></tr>
                      <tr><td style="padding:5px 0;color:#64748b;font-size:13px;">Location</td><td style="padding:5px 0;font-size:13px;">{System.Net.WebUtility.HtmlEncode(location)}</td></tr>
                      <tr><td style="padding:5px 0;color:#64748b;font-size:13px;">Auditor</td><td style="padding:5px 0;font-size:13px;">{System.Net.WebUtility.HtmlEncode(auditor)}</td></tr>
                      <tr><td style="padding:5px 0;color:#64748b;font-size:13px;">Project Manager</td><td style="padding:5px 0;font-size:13px;">{System.Net.WebUtility.HtmlEncode(pm)}</td></tr>
                      <tr><td style="padding:5px 0;color:#64748b;font-size:13px;">Client</td><td style="padding:5px 0;font-size:13px;">{System.Net.WebUtility.HtmlEncode(client)}</td></tr>
                      <tr><td style="padding:5px 0;color:#64748b;font-size:13px;">Job Number</td><td style="padding:5px 0;font-size:13px;">{System.Net.WebUtility.HtmlEncode(jobNumber)}</td></tr>
                      <tr><td style="padding:5px 0;color:#64748b;font-size:13px;">Audit Date</td><td style="padding:5px 0;font-size:13px;">{auditDate}</td></tr>
                      <tr><td style="padding:5px 0;color:#64748b;font-size:13px;">Conformance Score</td><td style="padding:5px 0;font-weight:bold;font-size:13px;color:{(findings.Count == 0 ? "#16a34a" : "#dc2626")}">{scoreText}</td></tr>
                    </table>

                    {summaryBlock}

                    <h3 style="font-size:14px;color:#1e293b;margin:20px 0 10px;border-bottom:1px solid #e2e8f0;padding-bottom:6px;">
                      Non-Conforming Findings ({findings.Count})
                    </h3>
                    {findingsBlock}
                    {attachmentBlock}

                    <div style="margin-top:24px;">
                      <a href="{reviewLink}" style="background:#1e3a5f;color:#fff;padding:10px 20px;border-radius:4px;text-decoration:none;display:inline-block;font-size:13px;">
                        View Full Audit Report →
                      </a>
                    </div>
                    <p style="color:#94a3b8;font-size:11px;margin-top:20px;">
                      Sent by {System.Net.WebUtility.HtmlEncode(audit.ReviewedBy ?? "AuditAdmin")} via Stronghold Compliance Audit system.
                    </p>
                  </div>
                </div>
                """;

            // ── SMTP attachments (files from local storage) ───────────────────
            var smtpAttachments = selectedAttachments
                .Where(a => !string.IsNullOrEmpty(a.BlobPath) && File.Exists(a.BlobPath))
                .Select(a => (a.FileName, a.BlobPath!))
                .ToList();

            // ── PDF attachment ────────────────────────────────────────────────
            string? pdfTempPath = null;
            try
            {
                if (includePdf)
                {
                    var pdfBytes = await _pdf.GeneratePdfAsync(body, ct: ct);
                    pdfTempPath = Path.Combine(Path.GetTempPath(),
                        $"audit-{audit.Id}-{Convert.ToHexString(RandomNumberGenerator.GetBytes(8)).ToLowerInvariant()}.pdf");
                    await File.WriteAllBytesAsync(pdfTempPath, pdfBytes, ct);
                    var pdfName = $"Audit-{(audit.TrackingNumber ?? audit.Id.ToString()).Replace("/", "-")}.pdf";
                    smtpAttachments.Add((pdfName, pdfTempPath));
                }

                await _email.SendAsync(subject, body, recipients, smtpAttachments, ct);
            }
            finally
            {
                if (pdfTempPath != null && File.Exists(pdfTempPath))
                    File.Delete(pdfTempPath);
            }
    }
}
