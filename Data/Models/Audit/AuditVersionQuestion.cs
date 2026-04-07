namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Junction table linking questions to a specific template version with per-version rules.
/// Copied when a version is cloned. Only editable when the version is in Draft status.
///
/// This is the "what the form looks like for this version" record.
/// The question text itself is on AuditQuestion; rules are here because they can
/// change between versions (e.g., a question that was scoreable may become non-scoreable).
/// </summary>
public class AuditVersionQuestion : AuditableEntity
{
    public int TemplateVersionId { get; set; }
    public int SectionId { get; set; }
    public int QuestionId { get; set; }
    public int DisplayOrder { get; set; }

    /// <summary>Can the auditor mark this N/A instead of a scored status?</summary>
    public bool AllowNA { get; set; } = true;

    /// <summary>Must the auditor enter a comment when marking Non-Conforming?</summary>
    public bool RequireCommentOnNC { get; set; } = true;

    /// <summary>Does this question count toward the conformance score?</summary>
    public bool IsScoreable { get; set; } = true;

    /// <summary>Per-version weight override. Null means use AuditQuestion.Weight.</summary>
    public decimal? Weight { get; set; }

    // Navigation
    public AuditTemplateVersion TemplateVersion { get; set; } = null!;
    public AuditSection Section { get; set; } = null!;
    public AuditQuestion Question { get; set; } = null!;
}
