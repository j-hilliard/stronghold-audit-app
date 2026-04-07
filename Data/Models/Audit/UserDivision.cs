using Stronghold.AppDashboard.Data.Models;

namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Links a User to the Division(s) they are allowed to access.
/// Used to enforce role+scope security: non-admin users only see audits
/// for their assigned divisions.
///
/// Admins (Administrator, AuditManager) bypass this filter and see all divisions.
/// </summary>
public class UserDivision
{
    public int UserId { get; set; }
    public int DivisionId { get; set; }
    public DateTime AssignedAt { get; set; }
    public string AssignedBy { get; set; } = null!;

    // Navigation
    public User User { get; set; } = null!;
    public Division Division { get; set; } = null!;
}
