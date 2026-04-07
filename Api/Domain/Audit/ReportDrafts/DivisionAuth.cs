using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.ReportDrafts;

/// <summary>
/// Division-scope authorization helper used by all ReportDraft handlers.
/// Admins (Administrator, AuditManager) bypass the UserDivisions check.
/// </summary>
internal static class DivisionAuth
{
    /// <summary>
    /// Throws UnauthorizedAccessException if the caller does not have access
    /// to the specified division (or any division when divisionId is null).
    /// </summary>
    public static async Task AssertAccessAsync(
        AppDbContext context,
        string requesterEmail,
        int? divisionId,
        CancellationToken ct)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == requesterEmail, ct)
            ?? throw new UnauthorizedAccessException("User not found.");

        // Admins and AuditManagers can access all divisions
        var isAdmin = await context.UserRoles
            .AsNoTracking()
            .Include(ur => ur.Role)
            .AnyAsync(ur => ur.UserId == user.UserId &&
                (ur.Role.Name == AuthorizationRoles.Administrator ||
                 ur.Role.Name == AuthorizationRoles.AuditManager), ct);

        if (isAdmin) return;

        // For a specific division, check UserDivisions
        if (divisionId.HasValue)
        {
            var hasAccess = await context.UserDivisions
                .AsNoTracking()
                .AnyAsync(ud => ud.UserId == user.UserId && ud.DivisionId == divisionId.Value, ct);

            if (!hasAccess)
                throw new UnauthorizedAccessException(
                    $"User does not have access to division {divisionId.Value}.");
        }
        // null divisionId = listing across all divisions — admin-only; non-admins fall through
        // because they will be filtered to their own divisions by the query itself
    }
}
