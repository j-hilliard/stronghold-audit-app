namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// A grouping of questions within a template version (e.g., "Permitting", "PPE", "Equipment").
/// Sections are copied when a version is cloned.
/// </summary>
public class AuditSection : AuditableEntity
{
    public int TemplateVersionId { get; set; }
    public string Name { get; set; } = null!;
    public int DisplayOrder { get; set; }

    // Navigation
    public AuditTemplateVersion TemplateVersion { get; set; } = null!;
    public ICollection<AuditVersionQuestion> VersionQuestions { get; set; } = new List<AuditVersionQuestion>();
}
