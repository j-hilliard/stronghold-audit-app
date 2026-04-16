using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class DeleteAudit : IRequest<Unit>
{
    public int AuditId { get; set; }
    public string DeletedBy { get; set; } = null!;
}

public class DeleteAuditHandler : IRequestHandler<DeleteAudit, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public DeleteAuditHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Unit> Handle(DeleteAudit request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .FirstOrDefaultAsync(a => a.Id == request.AuditId && !a.IsDeleted, cancellationToken)
            ?? throw new ArgumentException($"Audit {request.AuditId} not found.");

        if (audit.Status != "Draft")
            throw new InvalidOperationException($"Only Draft audits can be deleted. This audit is '{audit.Status}'.");

        var now = DateTime.UtcNow;
        audit.IsDeleted = true;
        audit.DeletedAt = now;
        audit.DeletedBy = request.DeletedBy;
        audit.UpdatedAt = now;
        audit.UpdatedBy = request.DeletedBy;

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("DeleteAudit", "Audit", "Info",
            $"Draft audit {audit.Id} deleted by {request.DeletedBy}.",
            relatedObject: audit.Id.ToString());

        return Unit.Value;
    }
}
