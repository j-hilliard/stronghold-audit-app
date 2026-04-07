using System.ComponentModel.DataAnnotations;

namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Persisted state for a user-composed compliance report.
/// Blocks are stored as opaque JSON — the backend never inspects block structure.
/// DateFrom/DateTo are the authoritative date range for regeneration.
/// Period is a display label only (e.g. "Q1 2026" or "Mar 1 – Apr 7, 2026").
/// </summary>
public class ReportDraft : AuditableEntity
{
    public int DivisionId { get; set; }
    public string Title { get; set; } = null!;

    /// <summary>Display label only — never used for data queries.</summary>
    public string Period { get; set; } = null!;

    /// <summary>UTC start of the report window (midnight). Null = all history.</summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>UTC end of the report window (23:59:59). Null = now.</summary>
    public DateTime? DateTo { get; set; }

    /// <summary>Serialized ReportBlock[] JSON. Treated as opaque string by the backend.</summary>
    public string BlocksJson { get; set; } = "[]";

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    // Navigation
    public Division Division { get; set; } = null!;
}
