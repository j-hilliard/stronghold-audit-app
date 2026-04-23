using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin, AuthorizationRole.Auditor)]
public class DeleteAudit : IRequest<Unit>
{
    public int AuditId { get; set; }
    public string DeletedBy { get; set; } = null!;
}

public class DeleteAuditHandler : IRequestHandler<DeleteAudit, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;
    private readonly IAuditLogService _auditLog;

    public DeleteAuditHandler(AppDbContext context, IProcessLogService log, IAuditLogService auditLog)
    {
        _context  = context;
        _log      = log;
        _auditLog = auditLog;
    }

    public async Task<Unit> Handle(DeleteAudit request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .FirstOrDefaultAsync(a => a.Id == request.AuditId && !a.IsDeleted, cancellationToken)
            ?? throw new ArgumentException($"Audit {request.AuditId} not found.");

        var isAdminUser = await _context.Users
            .Where(u => u.Email == request.DeletedBy && u.Active)
            .AnyAsync(u => u.UserRoles.Any(ur =>
                ur.Role.Name == AuthorizationRoles.AuditAdmin ||
                ur.Role.Name == AuthorizationRoles.Administrator), cancellationToken);

        if (!isAdminUser && audit.Status != "Draft")
            throw new InvalidOperationException($"Only Draft audits can be deleted. This audit is '{audit.Status}'.");

        var now = DateTime.UtcNow;

        // Soft-delete and void all CAs tied to this audit so they drop off KPIs
        var cas = await _context.CorrectiveActions
            .Where(ca => ca.AuditId == request.AuditId && !ca.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var ca in cas)
        {
            ca.Status    = "Voided";
            ca.IsDeleted = true;
            ca.DeletedAt = now;
            ca.DeletedBy = request.DeletedBy;
            ca.UpdatedAt = now;
            ca.UpdatedBy = request.DeletedBy;
        }

        audit.IsDeleted = true;
        audit.DeletedAt = now;
        audit.DeletedBy = request.DeletedBy;
        audit.UpdatedAt = now;
        audit.UpdatedBy = request.DeletedBy;

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("DeleteAudit", "Audit", "Info",
            $"Audit {audit.Id} deleted by {request.DeletedBy}. {cas.Count} corrective action(s) voided.",
            relatedObject: audit.Id.ToString());

        await _auditLog.LogAsync(
            request.DeletedBy,
            "DeleteAudit",
            "Audit",
            $"Audit #{audit.Id} ({audit.Division?.Code ?? "?"}) deleted. {cas.Count} corrective action(s) voided.",
            entityId: audit.Id.ToString(),
            severity: "Warning");

        return Unit.Value;
    }
}
