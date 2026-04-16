using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditReviewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
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

    public CloseAuditHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Unit> Handle(CloseAudit request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, cancellationToken)
            ?? throw new ArgumentException($"Audit {request.AuditId} not found.");

        if (audit.Status != "Submitted" && audit.Status != "Reopened")
            throw new InvalidOperationException($"Audit {request.AuditId} cannot be closed from status '{audit.Status}'.");

        var now = DateTime.UtcNow;
        audit.Status = "Closed";
        audit.UpdatedAt = now;
        audit.UpdatedBy = request.ClosedBy;

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("CloseAudit", "Audit", "Info",
            $"Audit {audit.Id} closed by {request.ClosedBy}. Notes: {request.Notes ?? "none"}",
            relatedObject: audit.Id.ToString());

        return Unit.Value;
    }
}
