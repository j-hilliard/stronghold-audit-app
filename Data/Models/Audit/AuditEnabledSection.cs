namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Immutable snapshot of which optional section groups were enabled when the audit was created.
/// Once an audit is created, this set never changes — it defines the audit's scope.
///
/// Sections with a matching OptionalGroupKey are included in the audit form and scoring.
/// Sections whose OptionalGroupKey is NOT listed here are excluded entirely.
/// </summary>
public class AuditEnabledSection
{
    public int Id { get; set; }
    public int AuditId { get; set; }

    /// <summary>Matches AuditSection.OptionalGroupKey — e.g. "RADIOGRAPHY", "ROPE_ACCESS"</summary>
    public string OptionalGroupKey { get; set; } = null!;

    // Navigation
    public Audit Audit { get; set; } = null!;
}
