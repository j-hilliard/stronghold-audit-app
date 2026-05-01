using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.AuditReviewer)]
public class GetDistributionPreview : IRequest<DistributionPreviewDto>
{
    public int AuditId { get; set; }
    public List<int> AttachmentIds { get; set; } = new();
}

public class PreviewRecipientDto
{
    public string EmailAddress { get; set; } = null!;
    public string? Name { get; set; }
    /// <summary>"Routing" = division-level routing rule; "Manual" = per-audit manually added.</summary>
    public string Source { get; set; } = null!;
    /// <summary>Set only when Source == "Manual" — used for permanent removal.</summary>
    public int? ManualRecipientId { get; set; }
}

public class DistributionPreviewDto
{
    public string Subject { get; set; } = string.Empty;
    /// <summary>Flat email list — kept for legacy count display.</summary>
    public List<string> Recipients { get; set; } = new();
    /// <summary>Full recipient details including source and ID for inline editing.</summary>
    public List<PreviewRecipientDto> RecipientDetails { get; set; } = new();
    public string BodyHtml { get; set; } = string.Empty;
    public string? FindingsSummary { get; set; }
}

public class GetDistributionPreviewHandler : IRequestHandler<GetDistributionPreview, DistributionPreviewDto>
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IAuditEmailBodyBuilder _builder;

    public GetDistributionPreviewHandler(
        AppDbContext context,
        IConfiguration config,
        IAuditEmailBodyBuilder builder)
    {
        _context = context;
        _config  = config;
        _builder = builder;
    }

    public async Task<DistributionPreviewDto> Handle(GetDistributionPreview request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .Include(a => a.Division)
            .Include(a => a.Header)
            .Include(a => a.Findings).ThenInclude(f => f.CorrectiveActions)
            .Include(a => a.Responses)
            .Include(a => a.Attachments)
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, cancellationToken)
            ?? throw new KeyNotFoundException($"Audit {request.AuditId} not found.");

        // ── Recipients ────────────────────────────────────────────────────────
        var routingRecipients = await _context.EmailRoutingRules
            .Where(r => r.DivisionId == audit.DivisionId && r.IsActive)
            .Select(r => new { r.EmailAddress, Name = (string?)null })
            .ToListAsync(cancellationToken);

        var manualRecipients = await _context.AuditDistributionRecipients
            .Where(r => r.AuditId == request.AuditId)
            .Select(r => new { r.Id, r.EmailAddress, r.Name })
            .ToListAsync(cancellationToken);

        var seenEmails     = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var recipientDetails = new List<PreviewRecipientDto>();

        foreach (var r in routingRecipients)
        {
            if (seenEmails.Add(r.EmailAddress ?? string.Empty) && !string.IsNullOrWhiteSpace(r.EmailAddress))
                recipientDetails.Add(new PreviewRecipientDto { EmailAddress = r.EmailAddress, Source = "Routing" });
        }
        foreach (var r in manualRecipients)
        {
            if (seenEmails.Add(r.EmailAddress ?? string.Empty) && !string.IsNullOrWhiteSpace(r.EmailAddress))
                recipientDetails.Add(new PreviewRecipientDto { EmailAddress = r.EmailAddress, Name = r.Name, Source = "Manual", ManualRecipientId = r.Id });
        }

        var allRecipients = recipientDetails.Select(r => r.EmailAddress).ToList();

        // ── Build email via shared builder ────────────────────────────────────
        var appBaseUrl = _config.GetValue<string>("App:BaseUrl") ?? "http://localhost:7220";

        var (subject, bodyHtml) = _builder.Build(new AuditEmailBuildOptions
        {
            Audit                   = audit,
            SelectedAttachmentIds   = request.AttachmentIds,
            AppBaseUrl              = appBaseUrl,
            IncludeCorrectiveActions = true,
            // SentByName null → preview footer text
        });

        return new DistributionPreviewDto
        {
            Subject          = subject,
            Recipients       = allRecipients,
            RecipientDetails = recipientDetails,
            BodyHtml         = bodyHtml,
            FindingsSummary  = audit.ReviewSummary,
        };
    }
}
