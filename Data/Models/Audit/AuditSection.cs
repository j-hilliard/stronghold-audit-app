namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// A grouping of questions within a template version (e.g., "Permitting", "PPE", "Equipment").
/// Sections are copied when a version is cloned.
/// </summary>
public class AuditSection : AuditableEntity
{
    public int TemplateVersionId { get; set; }
    public string Name { get; set; } = null!;

    /// <summary>
    /// Stable machine-readable code. Name can be changed by business users;
    /// SectionCode stays fixed so it can be used in migration logic if needed.
    /// </summary>
    public string? SectionCode { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>FK to ReportingCategory — used in analytics to aggregate across renamed sections</summary>
    public int? ReportingCategoryId { get; set; }

    /// <summary>Whether this section must contain at least one answered question before the audit can be submitted</summary>
    public bool IsRequired { get; set; } = false;

    // Navigation
    public AuditTemplateVersion TemplateVersion { get; set; } = null!;
    public ReportingCategory? ReportingCategory { get; set; }
    public ICollection<AuditVersionQuestion> VersionQuestions { get; set; } = new List<AuditVersionQuestion>();
}
