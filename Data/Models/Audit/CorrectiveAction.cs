namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// A remediation action assigned to address an AuditFinding.
/// Status lifecycle: Open → InProgress → Closed (or Overdue if past DueDate)
/// </summary>
public class CorrectiveAction : AuditableEntity
{
    public int FindingId { get; set; }
    public string Description { get; set; } = null!;
    public DateOnly? DueDate { get; set; }
    public DateOnly? CompletedDate { get; set; }
    public string? AssignedTo { get; set; }

    /// <summary>"Open" | "InProgress" | "Overdue" | "Closed"</summary>
    public string Status { get; set; } = "Open";

    // Navigation
    public AuditFinding Finding { get; set; } = null!;
}
