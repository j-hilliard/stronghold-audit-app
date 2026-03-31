namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Header fields for an audit. Stored separately from Audit for clean normalization.
/// JobSite and Facility audits share this table; fields not applicable to the
/// audit type are null (e.g., JobNumber is null for Facility audits).
/// </summary>
public class AuditHeader : AuditableEntity
{
    public int AuditId { get; set; }

    // ── Job Site fields ──────────────────────────────────────────
    public string? JobNumber { get; set; }
    public string? Client { get; set; }
    public string? PM { get; set; }
    public string? Unit { get; set; }
    public string? Time { get; set; }
    /// <summary>"DAY" | "NIGHT"</summary>
    public string? Shift { get; set; }
    public string? WorkDescription { get; set; }

    // ── Facility fields ──────────────────────────────────────────
    public string? Company1 { get; set; }
    public string? Company2 { get; set; }
    public string? Company3 { get; set; }
    public string? ResponsibleParty { get; set; }

    // ── Shared fields ────────────────────────────────────────────
    public string? Location { get; set; }
    public DateOnly? AuditDate { get; set; }
    public string? Auditor { get; set; }

    // Navigation
    public Audit Audit { get; set; } = null!;
}
