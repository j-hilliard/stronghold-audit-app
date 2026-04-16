namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// A member of the review/distribution group that receives submission notifications
/// for all audits (regardless of division).
/// Managed via the Admin → Review Group settings panel.
/// </summary>
public class ReviewGroupMember : AuditableEntity
{
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; } = true;
}
