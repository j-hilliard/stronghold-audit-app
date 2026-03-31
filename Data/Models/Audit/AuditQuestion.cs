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

    /// <summary>True when admin removes this from active templates. Never set IsDeleted = true on questions.</summary>
    public bool IsArchived { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public string? ArchivedBy { get; set; }

    // Navigation
    public ICollection<AuditVersionQuestion> VersionQuestions { get; set; } = new List<AuditVersionQuestion>();
    public ICollection<AuditResponse> Responses { get; set; } = new List<AuditResponse>();
}
