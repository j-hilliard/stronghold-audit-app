namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Immutable log of every change made to a template version while in Draft status.
/// Records who changed what and when — no soft delete on this table (logs are permanent).
/// </summary>
public class TemplateChangeLog
{
    public int Id { get; set; }
    public int TemplateVersionId { get; set; }
    public string ChangedBy { get; set; } = null!;
    public DateTime ChangedAt { get; set; }

    /// <summary>"AddQuestion" | "RemoveQuestion" | "Reorder" | "EditText" | "Publish"</summary>
    public string ChangeType { get; set; } = null!;

    public string? ChangeNote { get; set; }

    // Navigation
    public AuditTemplateVersion TemplateVersion { get; set; } = null!;
}
