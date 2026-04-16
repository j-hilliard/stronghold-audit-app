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

    /// <summary>
    /// Section-level scoring weight multiplier applied to every question in this section.
    /// Default 1.0 = no adjustment. Snapshotted onto AuditResponse.SectionWeightSnapshot at save time.
    /// </summary>
    public decimal Weight { get; set; } = 1.0m;

    /// <summary>
    /// When true, this section is not shown by default and must be explicitly enabled at audit creation.
    /// For example, "RADIOGRAPHY" or "ROPE_ACCESS" sections only apply to certain jobs.
    /// </summary>
    public bool IsOptional { get; set; } = false;

    /// <summary>
    /// Groups optional sections so they can be toggled together (e.g., all "RADIOGRAPHY" sections on/off at once).
    /// Null = always visible. Non-null = only shown when the auditor explicitly enables the group.
    /// </summary>
    public string? OptionalGroupKey { get; set; }

    // Navigation
    public AuditTemplateVersion TemplateVersion { get; set; } = null!;
    public ReportingCategory? ReportingCategory { get; set; }
    public ICollection<AuditVersionQuestion> VersionQuestions { get; set; } = new List<AuditVersionQuestion>();
}
