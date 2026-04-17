namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// A photo attached to a corrective action as closure evidence.
/// RequireClosurePhoto enforcement on Division determines whether at least one photo
/// is required before the CA can be marked Closed.
/// </summary>
public class CorrectiveActionPhoto : AuditableEntity
{
    public int CorrectiveActionId { get; set; }

    /// <summary>Original user-facing filename</summary>
    public string FileName { get; set; } = null!;

    /// <summary>Filesystem path where the photo bytes are stored</summary>
    public string FilePath { get; set; } = null!;

    public long FileSizeBytes { get; set; }

    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = null!;

    public string? Caption { get; set; }

    // Navigation
    public CorrectiveAction CorrectiveAction { get; set; } = null!;
}
