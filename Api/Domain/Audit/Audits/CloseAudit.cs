using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditReviewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin)]
public class CloseAudit : IRequest<Unit>
{
    public int AuditId { get; set; }
    public string ClosedBy { get; set; } = null!;
    public string? Notes { get; set; }
}

public class CloseAuditHandler : IRequestHandler<CloseAudit, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;
    private readonly IMediator _mediator;

    public CloseAuditHandler(AppDbContext context, IProcessLogService log, IMediator mediator)
    {
        _context = context;
        _log = log;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(CloseAudit request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, cancellationToken)
            ?? throw new ArgumentException($"Audit {request.AuditId} not found.");

        var closableStatuses = new[] { "Submitted", "Reopened", "UnderReview", "Approved", "Distributed" };
        if (!closableStatuses.Contains(audit.Status))
            throw new InvalidOperationException($"Audit {request.AuditId} cannot be closed from status '{audit.Status}'.");

        // Block closure if any non-terminal CAs remain
        var openCaCount = await _context.CorrectiveActions
            .CountAsync(ca => !ca.IsDeleted
                && (ca.AuditId == audit.Id || (ca.Finding != null && ca.Finding.AuditId == audit.Id))
                && ca.Status != "Closed"
                && ca.Status != "Voided", cancellationToken);

        if (openCaCount > 0)
            throw new InvalidOperationException(
                $"Cannot close audit: {openCaCount} corrective action(s) are still open.");

        var now = DateTime.UtcNow;
        audit.Status = "Closed";
        audit.UpdatedAt = now;
        audit.UpdatedBy = request.ClosedBy;

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("CloseAudit", "Audit", "Info",
            $"Audit {audit.Id} closed by {request.ClosedBy}. Notes: {request.Notes ?? "none"}",
            relatedObject: audit.Id.ToString());

        // Auto-send distribution email on close — non-fatal if it fails
        try
        {
            await _mediator.Send(new SendDistributionEmail
            {
                AuditId    = request.AuditId,
                SentBy     = request.ClosedBy,
                AttachmentIds = new List<int>(),
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            await _log.LogAsync("CloseAudit", "Audit", "Warning",
                $"Auto-distribution email failed for audit {audit.Id}: {ex.Message}",
                relatedObject: audit.Id.ToString());
        }

        return Unit.Value;
    }
}
