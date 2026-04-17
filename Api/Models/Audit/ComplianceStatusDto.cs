namespace Stronghold.AppDashboard.Api.Models.Audit;

/// <summary>
/// Compliance schedule status for a single division.
/// Used by the dashboard compliance row (2C-1+2).
/// </summary>
public class ComplianceStatusDto
{
    public int DivisionId { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string DivisionName { get; set; } = null!;

    /// <summary>Date of the most recent submitted/closed audit, or null if none exists.</summary>
    public DateOnly? LastAuditDate { get; set; }

    /// <summary>Days since the last submitted/closed audit. Null if no audit exists.</summary>
    public int? DaysSinceLastAudit { get; set; }

    /// <summary>Configured audit frequency in days. Null = no schedule set for this division.</summary>
    public int? FrequencyDays { get; set; }

    /// <summary>
    /// Days until the next audit is due (negative = overdue).
    /// Null when LastAuditDate is null or FrequencyDays is null.
    /// </summary>
    public int? DaysUntilDue { get; set; }

    /// <summary>"OnTrack" | "DueSoon" | "Overdue" | "NoSchedule" | "NeverAudited"</summary>
    public string Status { get; set; } = "NoSchedule";
}
