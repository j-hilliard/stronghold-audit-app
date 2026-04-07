namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Master template record — one per division.
/// The actual checklist content lives in AuditTemplateVersion records.
/// </summary>
public class AuditTemplate : AuditableEntity
{
    public string Name { get; set; } = null!;

    /// <summary>"JobSite" | "Facility" — determines which header fields are shown</summary>
    public string? AuditType { get; set; }

    public string? Description { get; set; }

    /// <summary>Explicit active flag — allows disabling a template without deleting it</summary>
    public bool IsActive { get; set; } = true;

    public int DivisionId { get; set; }

    // Navigation
    public Division Division { get; set; } = null!;
    public ICollection<AuditTemplateVersion> Versions { get; set; } = new List<AuditTemplateVersion>();
}
