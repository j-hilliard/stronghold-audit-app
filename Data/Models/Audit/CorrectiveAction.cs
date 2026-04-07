namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// A remediation action assigned to address an AuditFinding.
/// Status lifecycle: Open → InProgress → Closed (or Overdue if past DueDate)
/// </summary>
public class CorrectiveAction : AuditableEntity
{
    public int FindingId { get; set; }

    /// <summary>Direct link to the parent audit (denormalized for efficient querying)</summary>
    public int? AuditId { get; set; }

    /// <summary>Direct link to the response that triggered this action</summary>
    public int? AuditResponseId { get; set; }

    public string Description { get; set; } = null!;
    public DateOnly? DueDate { get; set; }
    public DateOnly? CompletedDate { get; set; }

    public string? AssignedTo { get; set; }

    /// <summary>FK to Users.UserId — set when a known app user is assigned</summary>
    public int? AssignedToUserId { get; set; }

    /// <summary>"Open" | "InProgress" | "Overdue" | "Closed"</summary>
    public string Status { get; set; } = "Open";

    /// <summary>When true, the action cannot be closed without an evidence note</summary>
    public bool EvidenceRequired { get; set; }

    public DateTime? EvidenceReceivedDate { get; set; }
    public DateTime? ClosedDate { get; set; }

    // Navigation
    public AuditFinding Finding { get; set; } = null!;
}
