namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// A single question response within an audit.
///
/// QuestionTextSnapshot is the CRITICAL field — it stores the exact question text
/// at the moment the audit was saved. This guarantees historical records are always
/// complete and readable even if:
///   - The question text is later edited (typo fix, rewording)
///   - The question is archived
///   - The question is removed from newer template versions
/// See ADR-004 for the full versioning rationale.
/// </summary>
public class AuditResponse : AuditableEntity
{
    public int AuditId { get; set; }
    public int QuestionId { get; set; }

    /// <summary>Exact question text at time of audit. Self-contained — no join needed to read history.</summary>
    public string QuestionTextSnapshot { get; set; } = null!;

    /// <summary>
    /// Section name as it appeared at time of audit.
    /// Business users can rename sections; this preserves history exactly.
    /// </summary>
    public string? SectionNameSnapshot { get; set; }

    /// <summary>
    /// ReportingCategory name at time of audit.
    /// Ensures trend reports stay accurate even when questions move between sections.
    /// </summary>
    public string? ReportingCategorySnapshot { get; set; }

    /// <summary>Display order of the question within its section at time of audit</summary>
    public int? SortOrderSnapshot { get; set; }

    /// <summary>
    /// Effective question weight at time of save (AuditVersionQuestion.Weight ?? AuditQuestion.Weight).
    /// Snapshotted so historical scores stay deterministic even if weights change later.
    /// </summary>
    public decimal QuestionWeightSnapshot { get; set; } = 1.0m;

    /// <summary>
    /// Section weight (AuditSection.Weight) at time of save.
    /// Snapshotted alongside QuestionWeightSnapshot for the same reason.
    /// </summary>
    public decimal SectionWeightSnapshot { get; set; } = 1.0m;

    /// <summary>
    /// Whether the question was marked IsLifeCritical at the time of save.
    /// Snapshotted so the fail flag stays stable even if the template is changed later.
    /// </summary>
    public bool IsLifeCriticalSnapshot { get; set; } = false;

    /// <summary>"Conforming" | "NonConforming" | "Warning" | "NA"</summary>
    public string? Status { get; set; }

    public string? Comment { get; set; }
    public bool CorrectedOnSite { get; set; }

    /// <summary>True when the question rules require a corrective action for this response</summary>
    public bool CorrectiveActionRequired { get; set; }

    public DateOnly? CorrectiveActionDueDate { get; set; }

    // Navigation
    public Audit Audit { get; set; } = null!;
    public AuditQuestion Question { get; set; } = null!;
}
