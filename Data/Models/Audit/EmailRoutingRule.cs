namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Configures which email addresses receive audit review notifications for a division.
/// Replaces the hardcoded REVIEW_EMAILS list from the prototype.
/// Managed by SystemAdmin via the Email Routing settings page.
/// </summary>
public class EmailRoutingRule : AuditableEntity
{
    public int DivisionId { get; set; }
    public string EmailAddress { get; set; } = null!;
    public bool IsActive { get; set; } = true;

    // Navigation
    public Division Division { get; set; } = null!;
}
