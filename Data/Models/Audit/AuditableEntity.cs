namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Base class for all audit schema entities.
/// Every table in the audit schema has:
///   - int identity primary key
///   - Created/Updated audit columns (who + when)
///   - Soft-delete columns (HR data is never hard-deleted)
/// </summary>
public abstract class AuditableEntity
{
    public int Id { get; set; }

    // Audit trail — mandatory on every record
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

    // Soft delete — never hard-delete HR-sensitive data (see ADR-006)
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
