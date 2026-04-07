namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Master question record. NEVER deleted — only archived (see ADR-006).
/// The question text lives here; per-version rules live in AuditVersionQuestion.
///
/// When a question is removed from a template, IsArchived is set to true.
/// All historical AuditResponse records still reference this question
/// via QuestionId AND QuestionTextSnapshot, so history is always complete.
/// </summary>
public class AuditQuestion : AuditableEntity
{
    public string QuestionText { get; set; } = null!;

    /// <summary>Short label for compact views and reporting (e.g., "Permit Correct")</summary>
    public string? ShortLabel { get; set; }

    /// <summary>Guidance text shown to the auditor as a tooltip or sub-label</summary>
    public string? HelpText { get; set; }

    /// <summary>FK to ResponseType — drives which options are shown (defaults to StatusChoice)</summary>
    public int? ResponseTypeId { get; set; }

    /// <summary>Must the auditor provide an answer before submitting? (N/A counts as an answer)</summary>
    public bool IsRequired { get; set; } = false;

    /// <summary>Default rule: must comment when Warning. Per-version override lives on AuditVersionQuestion.</summary>
    public bool RequireCommentOnWarning { get; set; } = false;

    /// <summary>Whether the "Corrected On-Site" toggle is shown for this question</summary>
    public bool ShowCorrectedOnSite { get; set; } = true;

    /// <summary>Whether selecting a negative finding auto-creates a CorrectiveAction</summary>
    public bool RequireCorrectiveAction { get; set; } = false;

    /// <summary>Whether this question contributes to the conformance score by default</summary>
    public bool IsScored { get; set; } = true;

    /// <summary>Default scoring weight. Per-version override available on AuditVersionQuestion.</summary>
    public decimal Weight { get; set; } = 1.0m;

    /// <summary>True when admin removes this from active templates. Never set IsDeleted = true on questions.</summary>
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public string? ArchivedBy { get; set; }

    // Navigation
    public ResponseType? ResponseType { get; set; }
    public ICollection<AuditVersionQuestion> VersionQuestions { get; set; } = new List<AuditVersionQuestion>();
    public ICollection<AuditResponse> Responses { get; set; } = new List<AuditResponse>();
}
