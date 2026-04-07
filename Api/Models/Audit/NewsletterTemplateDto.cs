namespace Stronghold.AppDashboard.Api.Models.Audit;

public class NewsletterTemplateDto
{
    public int? Id { get; set; }
    public int DivisionId { get; set; }
    public string Name { get; set; } = null!;
    public string PrimaryColor { get; set; } = "#1e3a5f";
    public string AccentColor { get; set; } = "#f59e0b";
    public string? CoverImageUrl { get; set; }
    /// <summary>Null means all sections are visible.</summary>
    public List<string>? VisibleSections { get; set; }
    public bool IsDefault { get; set; }
}

public class SaveNewsletterTemplateRequest
{
    public int DivisionId { get; set; }
    public string Name { get; set; } = null!;
    public string PrimaryColor { get; set; } = "#1e3a5f";
    public string AccentColor { get; set; } = "#f59e0b";
    public string? CoverImageUrl { get; set; }
    public List<string>? VisibleSections { get; set; }
    public bool IsDefault { get; set; }
}
