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
    AuthorizationRole.TemplateAdmin)]
public class ApproveAudit : IRequest<Unit>
{
    public int AuditId { get; set; }
    public string ApprovedBy { get; set; } = null!;
}

public class ApproveAuditHandler : IRequestHandler<ApproveAudit, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;
    private readonly IConfiguration _config;

    public ApproveAuditHandler(AppDbContext context, IProcessLogService log, IConfiguration config)
    {
        _context = context;
        _log = log;
        _config = config;
    }

    public async Task<Unit> Handle(ApproveAudit request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, cancellationToken)
            ?? throw new KeyNotFoundException($"Audit {request.AuditId} not found.");

        if (audit.Status != "UnderReview" && audit.Status != "Submitted")
            throw new InvalidOperationException(
                $"Audit {request.AuditId} cannot be approved from status '{audit.Status}'. Expected 'UnderReview' or 'Submitted'.");

        var now = DateTime.UtcNow;
        audit.Status    = "Approved";
        audit.UpdatedAt = now;
        audit.UpdatedBy = request.ApprovedBy;
        await _context.SaveChangesAsync(cancellationToken);

        // Notify the original auditor (CreatedBy)
        var baseUrl = _config.GetValue<string>("App:BaseUrl") ?? "http://localhost:7220";
        _context.AppNotifications.Add(new AppNotification
        {
            RecipientEmail = audit.CreatedBy,
            Type           = "AuditApproved",
            Title          = "Audit Approved",
            Body           = $"Your audit {audit.TrackingNumber ?? $"#{audit.Id}"} has been approved and is ready for distribution.",
            EntityType     = "Audit",
            EntityId       = audit.Id,
            LinkUrl        = $"/audit-management/audits/{audit.Id}/review",
            CreatedAt      = now,
        });
        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("ApproveAudit", "Audit", "Info",
            $"Audit {audit.Id} approved by {request.ApprovedBy}.",
            relatedObject: audit.Id.ToString());

        return Unit.Value;
    }
}
