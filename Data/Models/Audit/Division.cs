namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Represents a Stronghold division/company unit.
/// Seeded with 9 divisions from the prototype: TKIE, STS, STG, SHI, SHI_RT, SHI_RA, ETS, CSL, FACILITY.
/// </summary>
public class Division : AuditableEntity
{
    /// <summary>Short code used in the prototype (e.g., "TKIE", "FACILITY")</summary>
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    /// <summary>"JobSite" or "Facility" — determines which header fields are shown on the audit form</summary>
    public string AuditType { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Minimum compliance target score (0–100). Null = no target set.
    /// Dashboard shows red/green indicator vs. this target.
    /// </summary>
    public decimal? ScoreTarget { get; set; }

    /// <summary>
    /// How often this division is expected to complete an audit, in days. Null = no schedule set.
    /// Used by the compliance status dashboard row (2C-1+2) to compute On Track / Due Soon / Overdue.
    /// Common values: 7 (weekly), 14 (bi-weekly), 30 (monthly), 90 (quarterly).
    /// </summary>
    public int? AuditFrequencyDays { get; set; }

    /// <summary>
    /// When true, closing a corrective action requires at least one closure photo to be attached first.
    /// Enforced at the application layer in CloseCorrectiveAction.
    /// </summary>
    public bool RequireClosurePhoto { get; set; } = false;

    // ── SLA configuration ──────────────────────────────────────────────────────

    /// <summary>
    /// Default CA due-date window in days for Normal-priority CAs.
    /// When set, CreateCorrectiveAction auto-computes DueDate = today + SlaNormalDays if none is supplied.
    /// </summary>
    public int? SlaNormalDays { get; set; }

    /// <summary>
    /// Due-date window in days for Urgent-priority CAs (faster SLA).
    /// </summary>
    public int? SlaUrgentDays { get; set; }

    /// <summary>
    /// Number of days past DueDate before a CA is considered "escalated".
    /// 0 = escalate immediately on becoming overdue. Null = no escalation threshold.
    /// </summary>
    public int? SlaEscalateAfterDays { get; set; }

    /// <summary>
    /// Email address (or semicolon-separated list) to notify when a CA passes its escalation threshold.
    /// </summary>
    public string? EscalationEmail { get; set; }

    /// <summary>
    /// OPU number from NAV/ERP system (e.g. "E465" for ETS, "E464" for CSL).
    /// Used for cross-referencing with finance/operations systems.
    /// </summary>
    public string? OPUNumber { get; set; }

    // Navigation
    public ICollection<AuditTemplate> Templates { get; set; } = new List<AuditTemplate>();
    public ICollection<EmailRoutingRule> EmailRoutingRules { get; set; } = new List<EmailRoutingRule>();
}
