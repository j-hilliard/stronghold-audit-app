namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Persists a recurring report delivery configuration.
/// </summary>
public class ScheduledReport
{
    public int Id { get; set; }

    /// <summary>Null = all divisions.</summary>
    public int? DivisionId { get; set; }

    /// <summary>"annual-review" | "quarterly-summary" | "post-audit-summary" | "ncr-report" | "executive-dashboard" | "ca-aging"</summary>
    public string TemplateId { get; set; } = null!;

    public string Title { get; set; } = null!;

    /// <summary>"Daily" | "Weekly" | "Monthly" | "Quarterly"</summary>
    public string Frequency { get; set; } = null!;

    /// <summary>0–6 Sunday–Saturday; only relevant when Frequency = Weekly.</summary>
    public int? DayOfWeek { get; set; }

    /// <summary>1–31; only relevant when Frequency = Monthly.</summary>
    public int? DayOfMonth { get; set; }

    /// <summary>24-hour UTC time, e.g. "07:00".</summary>
    public string TimeUtc { get; set; } = "07:00";

    /// <summary>"last30days" | "thisquarter" | "lastquarter" | "thisyear" | "lastyear"</summary>
    public string? DateRangePreset { get; set; }

    /// <summary>JSON array of email addresses.</summary>
    public string RecipientsJson { get; set; } = "[]";

    /// <summary>Hex color, e.g. "#1e3a5f".</summary>
    public string? PrimaryColor { get; set; }

    /// <summary>If set, only send the report when avg score drops below this threshold.</summary>
    public decimal? ScoreThreshold { get; set; }

    public DateTime? LastRunAt { get; set; }
    public DateTime NextRunAt { get; set; }

    public string CreatedBy { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Division? Division { get; set; }
}
