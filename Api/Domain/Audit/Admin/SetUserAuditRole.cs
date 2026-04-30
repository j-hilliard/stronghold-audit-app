using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(AuthorizationRole.Administrator, AuthorizationRole.TemplateAdmin, AuthorizationRole.AuditAdmin)]
public class SetUserAuditRole : IRequest
{
    public int UserId { get; set; }
    /// <summary>Audit role name to assign, or null to remove all audit roles from this user.</summary>
    public string? RoleName { get; set; }
}

public class SetUserAuditRoleHandler : IRequestHandler<SetUserAuditRole>
{
    private static readonly HashSet<string> AuditRoleNames = new(StringComparer.OrdinalIgnoreCase)
    {
        AuthorizationRoles.TemplateAdmin,
        AuthorizationRoles.AuditManager,
        AuthorizationRoles.AuditReviewer,
        AuthorizationRoles.CorrectiveActionOwner,
        AuthorizationRoles.ReadOnlyViewer,
        AuthorizationRoles.ExecutiveViewer,
    };

    private readonly AppDbContext _context;

    public SetUserAuditRoleHandler(AppDbContext context) => _context = context;

    public async Task Handle(SetUserAuditRole request, CancellationToken cancellationToken)
    {
        if (request.RoleName != null && !AuditRoleNames.Contains(request.RoleName))
            throw new ArgumentException($"'{request.RoleName}' is not a valid audit role.");

        // Remove any existing audit role assignments for this user
        var existing = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == request.UserId && AuditRoleNames.Contains(ur.Role.Name))
            .ToListAsync(cancellationToken);

        _context.UserRoles.RemoveRange(existing);

        // Add new role if provided
        if (request.RoleName != null)
        {
            var role = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == request.RoleName, cancellationToken)
                ?? throw new InvalidOperationException($"Role '{request.RoleName}' not found in database.");

            _context.UserRoles.Add(new Data.Models.UserRole
            {
                UserId = request.UserId,
                RoleId = role.RoleId,
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
