namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Master template record — one per division.
/// The actual checklist content lives in AuditTemplateVersion records.
/// </summary>
public class AuditTemplate : AuditableEntity
{
    public string Name { get; set; } = null!;

    public int DivisionId { get; set; }

    // Navigation
    public Division Division { get; set; } = null!;
    public ICollection<AuditTemplateVersion> Versions { get; set; } = new List<AuditTemplateVersion>();
}
