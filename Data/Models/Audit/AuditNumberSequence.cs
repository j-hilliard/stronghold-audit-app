namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Tracks the last-used sequence number per division per calendar year.
/// Used to generate the NNN portion of tracking numbers (e.g. H26-012-IPT).
/// Row is locked with UPDLOCK during CreateAudit to ensure atomic increment.
/// </summary>
public class AuditNumberSequence
{
    public int Id { get; set; }
    public int DivisionId { get; set; }
    public int Year { get; set; }

    /// <summary>Atomically incremented at audit creation. 1-based — the first audit of the year gets 001.</summary>
    public int LastSequence { get; set; }

    public Division Division { get; set; } = null!;
}
