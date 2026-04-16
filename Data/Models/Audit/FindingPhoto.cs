namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// A photo attached to a specific question response within an audit.
/// Multiple photos per question are allowed.
/// RequirePhotoOnNC enforcement is handled at the application layer.
/// </summary>
public class FindingPhoto : AuditableEntity
{
    public int AuditId { get; set; }

    /// <summary>FK to AuditQuestion.Id — the question this photo documents</summary>
    public int QuestionId { get; set; }

    /// <summary>Original user-facing filename</summary>
    public string FileName { get; set; } = null!;

    /// <summary>Filesystem path where the photo bytes are stored</summary>
    public string FilePath { get; set; } = null!;

    public long FileSizeBytes { get; set; }

    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = null!;

    public string? Caption { get; set; }

    // Navigation
    public Models.Audit.Audit Audit { get; set; } = null!;
    public AuditQuestion Question { get; set; } = null!;
}
