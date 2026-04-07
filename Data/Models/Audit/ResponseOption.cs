namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// A selectable option within a ResponseType.
/// Stores scoring logic centrally so the audit engine applies rules consistently.
///
/// For the StatusChoice response type the seeded options are:
///   Conforming      — ScoreValue=1, IsNegativeFinding=false
///   NonConforming   — ScoreValue=0, IsNegativeFinding=true, TriggersComment=true, TriggersCorrectiveAction=true
///   Warning         — ScoreValue=0, IsNegativeFinding=true, TriggersComment=true
///   NA              — ScoreValue=null (excluded from score denominator)
/// </summary>
public class ResponseOption : AuditableEntity
{
    public int ResponseTypeId { get; set; }
    public string OptionLabel { get; set; } = null!;

    /// <summary>Internal value stored in AuditResponse.Status ("Conforming", "NonConforming", "Warning", "NA")</summary>
    public string OptionValue { get; set; } = null!;

    /// <summary>Null means excluded from score denominator (e.g., N/A).</summary>
    public decimal? ScoreValue { get; set; }

    public int DisplayOrder { get; set; }

    /// <summary>True for options that represent a problem (drives findings and red highlighting).</summary>
    public bool IsNegativeFinding { get; set; }

    /// <summary>Selecting this option shows the comment textarea.</summary>
    public bool TriggersComment { get; set; }

    /// <summary>Selecting this option flags corrective action required.</summary>
    public bool TriggersCorrectiveAction { get; set; }

    // Navigation
    public ResponseType ResponseType { get; set; } = null!;
}
