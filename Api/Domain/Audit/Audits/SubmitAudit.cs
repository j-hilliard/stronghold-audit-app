using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;
using AuditFindingEntity = Stronghold.AppDashboard.Data.Models.Audit.AuditFinding;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class SubmitAudit : IRequest<Unit>
{
    public int AuditId { get; set; }
    public string SubmittedBy { get; set; } = null!;
}

public class SubmitAuditHandler : IRequestHandler<SubmitAudit, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;
    private readonly IEmailService _email;
    private readonly IAuditSummaryService _aiSummary;
    private readonly IConfiguration _config;

    public SubmitAuditHandler(
        AppDbContext context,
        IProcessLogService log,
        IEmailService email,
        IAuditSummaryService aiSummary,
        IConfiguration config)
    {
        _context  = context;
        _log      = log;
        _email    = email;
        _aiSummary = aiSummary;
        _config   = config;
    }

    public async Task<Unit> Handle(SubmitAudit request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .Include(a => a.Responses)
            .Include(a => a.Findings)
            .Include(a => a.Division)
            .Include(a => a.Header)
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, cancellationToken)
            ?? throw new ArgumentException($"Audit {request.AuditId} not found.");

        if (audit.Status != "Draft" && audit.Status != "Reopened")
            throw new InvalidOperationException($"Audit {request.AuditId} is already {audit.Status}.");

        var now = DateTime.UtcNow;
        var defaultDueDate = DateOnly.FromDateTime(now.AddDays(14));

        // ── Generate AuditFinding records for each NonConforming response ─────
        // Remove any existing findings first (safe on resubmit after reopen)
        _context.AuditFindings.RemoveRange(audit.Findings);

        var nonConforming = audit.Responses
            .Where(r => r.Status == "NonConforming")
            .ToList();

        foreach (var response in nonConforming)
        {
            _context.AuditFindings.Add(new AuditFindingEntity
            {
                AuditId = audit.Id,
                QuestionId = response.QuestionId,
                QuestionTextSnapshot = response.QuestionTextSnapshot,
                Description = response.Comment,
                CorrectedOnSite = response.CorrectedOnSite,
                CreatedAt = now,
                CreatedBy = request.SubmittedBy
            });
        }

        audit.Status = "Submitted";
        audit.SubmittedAt = now;
        audit.UpdatedAt = now;
        audit.UpdatedBy = request.SubmittedBy;

        // ── AI summary — generate before SaveChanges so we can persist it ─────
        var aiSummaryText = await GenerateAiSummaryAsync(audit, nonConforming, cancellationToken);
        audit.AiSummary = aiSummaryText;

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("SubmitAudit", "Audit", "Info",
            $"Audit {audit.Id} submitted by {request.SubmittedBy}. {nonConforming.Count} finding(s) generated.",
            relatedObject: audit.Id.ToString());

        // ── Send submission notification email ────────────────────────────────
        await SendSubmissionEmailAsync(audit, nonConforming.Count, now, defaultDueDate, aiSummaryText, cancellationToken);

        return Unit.Value;
    }

    /// <summary>
    /// Builds the AI summary input from audit responses and calls the summary service.
    /// Also fetches the prior audit score for trend comparison.
    /// Never throws — returns null on any failure.
    /// </summary>
    private async Task<string?> GenerateAiSummaryAsync(
        Data.Models.Audit.Audit audit,
        List<Data.Models.Audit.AuditResponse> nonConforming,
        CancellationToken ct)
    {
        try
        {
            // Current score (simple conformance %)
            var responses = audit.Responses.Where(r => r.Status != null).ToList();
            var conforming = responses.Count(r => r.Status == "Conforming");
            var denom = responses.Count(r => r.Status is "Conforming" or "NonConforming" or "Warning");
            double? score = denom > 0 ? Math.Round((double)conforming / denom * 100, 1) : null;

            // Prior audit score for this division (most recent submitted/closed before this one)
            double? priorScore = null;
            var prior = await _context.Audits
                .Where(a => a.DivisionId == audit.DivisionId
                         && a.Id != audit.Id
                         && (a.Status == "Submitted" || a.Status == "Closed")
                         && a.SubmittedAt < audit.SubmittedAt)
                .Include(a => a.Responses)
                .OrderByDescending(a => a.SubmittedAt)
                .FirstOrDefaultAsync(ct);

            if (prior != null)
            {
                var pr = prior.Responses.Where(r => r.Status != null).ToList();
                var pc = pr.Count(r => r.Status == "Conforming");
                var pd = pr.Count(r => r.Status is "Conforming" or "NonConforming" or "Warning");
                if (pd > 0) priorScore = Math.Round((double)pc / pd * 100, 1);
            }

            var ncItems = nonConforming
                .Select(r => (r.SectionNameSnapshot ?? "General", r.QuestionTextSnapshot ?? string.Empty))
                .ToList();

            var warnItems = audit.Responses
                .Where(r => r.Status == "Warning")
                .Select(r => (r.SectionNameSnapshot ?? "General", r.QuestionTextSnapshot ?? string.Empty))
                .ToList();

            return await _aiSummary.GenerateSummaryAsync(new AuditSummaryInput
            {
                DivisionCode        = audit.Division?.Code ?? "Unknown",
                Score               = score,
                PriorScore          = priorScore,
                NcItems             = ncItems,
                WarningItems        = warnItems,
                CorrectedOnSiteCount = nonConforming.Count(r => r.CorrectedOnSite),
                TotalNcCount        = nonConforming.Count,
            }, ct);
        }
        catch (Exception ex)
        {
            await _log.LogAsync("SubmitAudit", "AiSummary", "Warning",
                $"AI summary failed for audit {audit.Id}: {ex.Message}",
                relatedObject: audit.Id.ToString());
            return null;
        }
    }

    private async Task SendSubmissionEmailAsync(
        Data.Models.Audit.Audit audit,
        int ncCount,
        DateTime submittedAt,
        DateOnly defaultDueDate,
        string? aiSummaryText,
        CancellationToken cancellationToken)
    {
        try
        {
            // Collect recipients: division routing list + active review group members
            var divisionRecipients = await _context.EmailRoutingRules
                .Where(r => r.DivisionId == audit.DivisionId && r.IsActive)
                .Select(r => r.EmailAddress)
                .ToListAsync(cancellationToken);

            var reviewGroupRecipients = await _context.ReviewGroupMembers
                .Where(m => m.IsActive)
                .Select(m => m.Email)
                .ToListAsync(cancellationToken);

            var allRecipients = divisionRecipients.Union(reviewGroupRecipients).Distinct().ToList();
            if (allRecipients.Count == 0) return;

            // Score calculation
            var responses  = audit.Responses.Where(r => r.Status != null).ToList();
            var conforming = responses.Count(r => r.Status == "Conforming");
            var denom      = responses.Count(r => r.Status is "Conforming" or "NonConforming" or "Warning");
            var scoreText  = denom > 0 ? $"{Math.Round((double)conforming / denom * 100, 1)}%" : "N/A";

            var header     = audit.Header;
            var jobNumber  = header?.JobNumber ?? "—";
            var location   = header?.Location  ?? "—";
            var auditor    = header?.Auditor    ?? "—";
            var auditDate  = header?.AuditDate?.ToString("MM/dd/yyyy") ?? submittedAt.ToString("MM/dd/yyyy");
            var divCode    = audit.Division?.Code ?? "—";

            var appBaseUrl  = _config.GetValue<string>("App:BaseUrl") ?? "http://localhost:7220";
            var reviewLink  = $"{appBaseUrl}/audits/{audit.Id}/review";

            // Subject: Division | Score | NC count | Date  (matches plan spec)
            var subject = $"{divCode} Compliance Audit — {scoreText} — {ncCount} Non-Conforming Finding{(ncCount != 1 ? "s" : "")} — {auditDate}";

            // AI summary block (only when generated)
            var aiSummaryBlock = !string.IsNullOrWhiteSpace(aiSummaryText)
                ? $"""
                  <div style="background:#f0f9ff;border-left:4px solid #0ea5e9;padding:14px 16px;margin:16px 0;border-radius:0 4px 4px 0;">
                    <p style="margin:0 0 6px 0;font-size:12px;font-weight:bold;color:#0369a1;text-transform:uppercase;letter-spacing:0.5px;">AI Audit Summary</p>
                    <p style="margin:0;color:#1e293b;font-size:14px;line-height:1.6;">{System.Net.WebUtility.HtmlEncode(aiSummaryText)}</p>
                  </div>
                  """
                : string.Empty;

            var body = $"""
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;">
                  <div style="background:#1e3a5f;padding:20px;border-radius:4px 4px 0 0;">
                    <h2 style="color:#fff;margin:0;">Audit Submitted — {divCode}</h2>
                  </div>
                  <div style="border:1px solid #ddd;border-top:none;padding:20px;border-radius:0 0 4px 4px;">
                    <table style="width:100%;border-collapse:collapse;">
                      <tr><td style="padding:6px 0;color:#666;width:140px;">Division</td><td style="padding:6px 0;font-weight:bold;">{divCode}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Job Number</td><td style="padding:6px 0;">{jobNumber}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Location</td><td style="padding:6px 0;">{location}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Auditor</td><td style="padding:6px 0;">{auditor}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Audit Date</td><td style="padding:6px 0;">{auditDate}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Conformance Score</td><td style="padding:6px 0;font-weight:bold;color:{(ncCount == 0 ? "#16a34a" : "#dc2626")};">{scoreText}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Non-Conformances</td><td style="padding:6px 0;">{ncCount}</td></tr>
                      {(ncCount > 0 ? $"<tr><td style=\"padding:6px 0;color:#666;\">CA Due Date</td><td style=\"padding:6px 0;\">{defaultDueDate:MM/dd/yyyy} (14-day default)</td></tr>" : "")}
                    </table>
                    {aiSummaryBlock}
                    <div style="margin-top:24px;">
                      <a href="{reviewLink}" style="background:#1e3a5f;color:#fff;padding:10px 20px;border-radius:4px;text-decoration:none;display:inline-block;">
                        View Full Audit Report →
                      </a>
                    </div>
                    <p style="color:#888;font-size:12px;margin-top:24px;">
                      This notification was sent automatically by the Stronghold Compliance Audit system.
                    </p>
                  </div>
                </div>
                """;

            await _email.SendAsync(subject, body, allRecipients, cancellationToken);
        }
        catch (Exception ex)
        {
            // Never fail the submission because of an email error
            await _log.LogAsync("SubmitAudit", "Email", "Warning",
                $"Submission email failed for audit {audit.Id}: {ex.Message}",
                relatedObject: audit.Id.ToString());
        }
    }
}
