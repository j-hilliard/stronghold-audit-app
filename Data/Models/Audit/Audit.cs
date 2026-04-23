namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// A single compliance audit instance.
/// Status lifecycle: Draft → Submitted → (Reopened → Draft) → Closed
///
/// TemplateVersionId is locked at creation and never changes.
/// Submitted audits are immutable without an explicit reopen (see ADR-008).
/// </summary>
public class Audit : AuditableEntity
{
    public int DivisionId { get; set; }

    /// <summary>Locked at creation — never changes. Ensures historical accuracy (see ADR-004).</summary>
    public int TemplateVersionId { get; set; }

    /// <summary>"JobSite" | "Facility" — copied from Division.AuditType at creation</summary>
    public string AuditType { get; set; } = null!;

    /// <summary>"Draft" | "Submitted" | "Reopened" | "Closed"</summary>
    public string Status { get; set; } = "Draft";

    public DateTime? SubmittedAt { get; set; }

    // Navigation
    public Division Division { get; set; } = null!;
    public AuditTemplateVersion TemplateVersion { get; set; } = null!;
    public AuditHeader? Header { get; set; }
    public ICollection<AuditResponse> Responses { get; set; } = new List<AuditResponse>();
    public ICollection<AuditFinding> Findings { get; set; } = new List<AuditFinding>();
    public ICollection<AuditAttachment> Attachments { get; set; } = new List<AuditAttachment>();

    /// <summary>Optional section groups enabled at audit creation (immutable after create).</summary>
    public ICollection<AuditEnabledSection> EnabledSections { get; set; } = new List<AuditEnabledSection>();

    /// <summary>AI-generated plain-language summary generated at submission time. Null when AI is disabled or unavailable.</summary>
    public string? AiSummary { get; set; }

    /// <summary>AuditAdmin-authored findings narrative included in the distribution email.</summary>
    public string? ReviewSummary { get; set; }

    /// <summary>Timestamp when the AuditAdmin sent the distribution email.</summary>
    public DateTime? ReviewedAt { get; set; }

    /// <summary>Display name of the AuditAdmin who sent the distribution email.</summary>
    public string? ReviewedBy { get; set; }

    /// <summary>
    /// Auto-generated tracking number assigned at creation (e.g. "H26-012-IPT").
    /// Format: {Prefix}{YY}-{NNN}-{SiteCode} where NNN is per-division per-year sequence.
    /// Null only for audits created before this feature was introduced.
    /// </summary>
    public string? TrackingNumber { get; set; }

    /// <summary>FK to the DivisionJobPrefix used when generating the tracking number.</summary>
    public int? JobPrefixId { get; set; }

    public DivisionJobPrefix? JobPrefix { get; set; }
}
