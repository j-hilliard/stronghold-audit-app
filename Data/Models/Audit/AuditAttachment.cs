namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// A file attachment (photo, document) associated with an audit.
/// BlobPath stores the Azure Blob Storage path (Phase 2+).
/// For local dev, FileName is sufficient for reference.
/// </summary>
public class AuditAttachment : AuditableEntity
{
    public int AuditId { get; set; }
    public string FileName { get; set; } = null!;

    /// <summary>Azure Blob Storage path — populated in Phase 2+ when blob storage is configured</summary>
    public string? BlobPath { get; set; }

    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = null!;

    // Navigation
    public Audit Audit { get; set; } = null!;
}
