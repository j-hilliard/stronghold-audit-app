namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// A snapshot of a checklist at a point in time.
/// Status lifecycle: Draft → Active → Superseded
///
/// Rules (see ADR-004):
/// - Only one Active version per template at a time
/// - Active versions are immutable — no edits allowed
/// - Draft versions are editable (clone → edit → publish)
/// - Audits lock to their version at creation and never change
/// </summary>
public class AuditTemplateVersion : AuditableEntity
{
    public int TemplateId { get; set; }
    public int VersionNumber { get; set; }

    /// <summary>"Draft" | "Active" | "Superseded"</summary>
    public string Status { get; set; } = "Draft";

    public DateTime? PublishedAt { get; set; }
    public string? PublishedBy { get; set; }

    /// <summary>The version this was cloned from, if any</summary>
    public int? ClonedFromVersionId { get; set; }

    // Navigation
    public AuditTemplate Template { get; set; } = null!;
    public ICollection<AuditSection> Sections { get; set; } = new List<AuditSection>();
    public ICollection<AuditVersionQuestion> VersionQuestions { get; set; } = new List<AuditVersionQuestion>();
    public ICollection<TemplateChangeLog> ChangeLogs { get; set; } = new List<TemplateChangeLog>();
}
