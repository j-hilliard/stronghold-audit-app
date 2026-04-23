using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(
    AuthorizationRole.Administrator, AuthorizationRole.TemplateAdmin, AuthorizationRole.AuditAdmin,
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.Auditor, AuthorizationRole.NormalUser)]
public class GetUsersWithAuditRoles : IRequest<List<UserAuditRoleDto>> { }

public class GetUsersWithAuditRolesHandler : IRequestHandler<GetUsersWithAuditRoles, List<UserAuditRoleDto>>
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

    public GetUsersWithAuditRolesHandler(AppDbContext context) => _context = context;

    public async Task<List<UserAuditRoleDto>> Handle(
        GetUsersWithAuditRoles request,
        CancellationToken cancellationToken)
    {
        // Load all active users
        var users = await _context.Users
            .Where(u => u.Active)
            .OrderBy(u => u.LastName).ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);

        // Load all audit role assignments in one query
        var auditRoleAssignments = await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => AuditRoleNames.Contains(ur.Role.Name))
            .ToListAsync(cancellationToken);

        var roleByUserId = auditRoleAssignments
            .GroupBy(ur => ur.UserId)
            .ToDictionary(g => g.Key, g => g.First().Role.Name);

        return users.Select(u => new UserAuditRoleDto
        {
            UserId    = u.UserId,
            FirstName = u.FirstName,
            LastName  = u.LastName,
            Email     = u.Email,
            AuditRole = roleByUserId.TryGetValue(u.UserId, out var role) ? role : null,
        }).ToList();
    }
}
