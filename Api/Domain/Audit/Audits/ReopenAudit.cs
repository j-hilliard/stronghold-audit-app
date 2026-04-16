using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator)]
public class ReopenAudit : IRequest<Unit>
{
    public int AuditId { get; set; }
    public string ReopenedBy { get; set; } = null!;
    public string? Reason { get; set; }
}

public class ReopenAuditHandler : IRequestHandler<ReopenAudit, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public ReopenAuditHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Unit> Handle(ReopenAudit request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, cancellationToken)
            ?? throw new ArgumentException($"Audit {request.AuditId} not found.");

        if (audit.Status != "Submitted" && audit.Status != "Closed")
            throw new InvalidOperationException($"Audit {request.AuditId} cannot be reopened from status '{audit.Status}'.");

        var now = DateTime.UtcNow;
        audit.Status = "Reopened";
        audit.UpdatedAt = now;
        audit.UpdatedBy = request.ReopenedBy;

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("ReopenAudit", "Audit", "Info",
            $"Audit {audit.Id} reopened by {request.ReopenedBy}. Reason: {request.Reason ?? "none"}",
            relatedObject: audit.Id.ToString());

        return Unit.Value;
    }
}
