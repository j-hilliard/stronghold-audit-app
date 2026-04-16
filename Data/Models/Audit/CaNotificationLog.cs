namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Append-only log of CA reminder emails sent.
/// Used to deduplicate: the reminder service checks this table before sending
/// so each notification type is sent at most once per CA per day.
/// NotificationType: "DueSoon" | "DueToday" | "Overdue"
/// </summary>
public class CaNotificationLog
{
    public int Id { get; set; }
    public int CorrectiveActionId { get; set; }

    /// <summary>"DueSoon" | "DueToday" | "Overdue"</summary>
    public string NotificationType { get; set; } = null!;

    public DateTime SentAt { get; set; }
    public string Recipient { get; set; } = null!;

    // Navigation
    public CorrectiveAction CorrectiveAction { get; set; } = null!;
}
