using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.AuditReviewer, AuthorizationRole.TemplateAdmin)]
public class StartReview : IRequest<Unit>
{
    public int AuditId { get; set; }
    public string ReviewStartedBy { get; set; } = null!;
}

public class StartReviewHandler : IRequestHandler<StartReview, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;
    private readonly IConfiguration _config;

    public StartReviewHandler(AppDbContext context, IProcessLogService log, IConfiguration config)
    {
        _context = context;
        _log     = log;
        _config  = config;
    }

    public async Task<Unit> Handle(StartReview request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, cancellationToken)
            ?? throw new KeyNotFoundException($"Audit {request.AuditId} not found.");

        if (audit.Status != "Submitted")
            throw new InvalidOperationException(
                $"Audit {request.AuditId} cannot start review from status '{audit.Status}'. Expected 'Submitted'.");

        var now = DateTime.UtcNow;
        audit.Status    = "UnderReview";
        audit.UpdatedAt = now;
        audit.UpdatedBy = request.ReviewStartedBy;
        await _context.SaveChangesAsync(cancellationToken);

        // Notify the auditor who submitted this audit that review has started
        if (!string.IsNullOrWhiteSpace(audit.CreatedBy))
        {
            try
            {
                _context.AppNotifications.Add(new AppNotification
                {
                    RecipientEmail = audit.CreatedBy,
                    Type           = "ReviewStarted",
                    Title          = "Audit Under Review",
                    Body           = $"Your audit {audit.TrackingNumber ?? $"#{audit.Id}"} is now under review by {request.ReviewStartedBy}.",
                    EntityType     = "Audit",
                    EntityId       = audit.Id,
                    LinkUrl        = $"/audit-management/audits/{audit.Id}/review",
                    CreatedAt      = now,
                });
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await _log.LogAsync("StartReview_NotificationFailed", "Audit", "Warning",
                    $"Failed to create review-started notification for audit {audit.Id}: {ex.Message}",
                    relatedObject: audit.Id.ToString());
            }
        }

        await _log.LogAsync("StartReview", "Audit", "Info",
            $"Audit {audit.Id} moved to UnderReview by {request.ReviewStartedBy}.",
            relatedObject: audit.Id.ToString());

        return Unit.Value;
    }
}
