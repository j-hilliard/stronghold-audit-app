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

    // Navigation
    public ICollection<AuditTemplate> Templates { get; set; } = new List<AuditTemplate>();
    public ICollection<EmailRoutingRule> EmailRoutingRules { get; set; } = new List<EmailRoutingRule>();
}
