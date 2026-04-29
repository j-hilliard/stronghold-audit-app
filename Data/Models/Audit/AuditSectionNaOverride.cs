namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Records that the auditor explicitly marked an entire section as Not Applicable for this audit.
/// Reason is required — creates a legal audit trail justifying why the section was not audited.
/// Questions in a N/A'd section are excluded from scoring and the unanswered count.
/// </summary>
public class AuditSectionNaOverride
{
    public int Id { get; set; }
    public int AuditId { get; set; }
    public int SectionId { get; set; }

    /// <summary>Required justification entered by the auditor. Stored for legal defensibility.</summary>
    public string Reason { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;

    // Navigation
    public Audit Audit { get; set; } = null!;
    public AuditSection Section { get; set; } = null!;
}
