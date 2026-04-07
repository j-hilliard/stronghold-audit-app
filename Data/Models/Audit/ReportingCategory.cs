namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Stable analytics anchor for trend reporting.
/// Sections are display-layer constructs that business users can rename freely.
/// ReportingCategory is the stable identifier used in reports and dashboards.
///
/// Each AuditSection maps to a ReportingCategory.
/// Each AuditResponse snapshots the ReportingCategory name at save time (ReportingCategorySnapshot)
/// so that trend queries remain accurate even after sections are renamed or reorganised.
///
/// Examples: Permitting, PPE, Equipment, LOTO, JHA, Culture, Environmental.
/// </summary>
public class ReportingCategory : AuditableEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<AuditSection> Sections { get; set; } = new List<AuditSection>();
}
