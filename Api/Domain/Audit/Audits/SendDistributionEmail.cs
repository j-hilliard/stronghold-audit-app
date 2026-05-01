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
    private readonly IAuditEmailBodyBuilder _builder;
    private readonly ILogger<SendDistributionEmailHandler> _log;

    public SendDistributionEmailHandler(
        AppDbContext context,
        IEmailService email,
        IPdfGeneratorService pdf,
        IConfiguration config,
        IAuditEmailBodyBuilder builder,
        ILogger<SendDistributionEmailHandler> log)
    {
        _context = context;
        _email   = email;
        _pdf     = pdf;
        _config  = config;
        _builder = builder;
        _log     = log;
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
        var appBaseUrl = _config.GetValue<string>("App:BaseUrl") ?? "http://localhost:7220";

        var (subject, body) = _builder.Build(new AuditEmailBuildOptions
        {
            Audit                    = audit,
            FindingsSummaryOverride  = message,
            SubjectOverride          = subjectOverride,
            IncludeCorrectiveActions = includeCorrectiveActions,
            IncludeOpenCasOnly       = includeOpenCasOnly,
            SelectedAttachmentIds    = attachmentIds,
            AppBaseUrl               = appBaseUrl,
            SentByName               = audit.ReviewedBy ?? "AuditAdmin",
        });

        // ── SMTP file attachments ─────────────────────────────────────────────
        var selectedAttachments = audit.Attachments
            .Where(a => attachmentIds.Contains(a.Id) && !a.IsDeleted)
            .ToList();

        var smtpAttachments = selectedAttachments
            .Where(a => !string.IsNullOrEmpty(a.BlobPath) && File.Exists(a.BlobPath))
            .Select(a => (a.FileName, a.BlobPath!))
            .ToList();

        // ── PDF attachment ────────────────────────────────────────────────────
        string? pdfTempPath = null;
        try
        {
            if (includePdf)
            {
                var pdfBytes = await _pdf.GeneratePdfAsync(body, ct: ct);
                pdfTempPath  = Path.Combine(Path.GetTempPath(),
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
