namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// A non-conformance finding generated when an audit is submitted.
/// One finding is created per NonConforming response.
/// QuestionTextSnapshot mirrors AuditResponse for the same self-containment reason.
/// </summary>
public class AuditFinding : AuditableEntity
{
    public int AuditId { get; set; }
    public int QuestionId { get; set; }
    public string QuestionTextSnapshot { get; set; } = null!;
    public string? Description { get; set; }
    public bool CorrectedOnSite { get; set; }

    // Navigation
    public Audit Audit { get; set; } = null!;
    public AuditQuestion Question { get; set; } = null!;
    public ICollection<CorrectiveAction> CorrectiveActions { get; set; } = new List<CorrectiveAction>();
}
