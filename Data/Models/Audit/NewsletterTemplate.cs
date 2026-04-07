namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Per-division newsletter template settings — primary color, accent, cover image,
/// and which sections are visible when the Composer generates a Newsletter report.
/// One active template per division; multiple versions are allowed but only one IsDefault.
/// </summary>
public class NewsletterTemplate : AuditableEntity
{
    public int DivisionId { get; set; }
    public string Name { get; set; } = null!;
    public string PrimaryColor { get; set; } = "#1e3a5f";
    public string AccentColor { get; set; } = "#f59e0b";
    public string? CoverImageUrl { get; set; }

    /// <summary>JSON array of visible section names. Null = all sections visible.</summary>
    public string? VisibleSectionsJson { get; set; }

    public bool IsDefault { get; set; }

    // Navigation
    public Division Division { get; set; } = null!;
}
