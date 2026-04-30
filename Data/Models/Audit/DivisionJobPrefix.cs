namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Defines a job-number prefix letter for a division.
/// Most divisions have one (e.g. ETS→H, STS→S). ETS also has B for Fab Shop work.
/// CSL has no letter prefix (Prefix = "").
/// </summary>
public class DivisionJobPrefix
{
    public int Id { get; set; }
    public int DivisionId { get; set; }

    /// <summary>The letter prefix (e.g. "H", "B", "S"). Empty string for divisions with no letter (CSL).</summary>
    public string Prefix { get; set; } = "";

    /// <summary>Human label shown in the audit creation form (e.g. "Main", "Fab Shop Work").</summary>
    public string Label { get; set; } = null!;

    /// <summary>True for the prefix that auto-selects when only one exists or when the division has no choice needed.</summary>
    public bool IsDefault { get; set; }

    public int SortOrder { get; set; }

    public Division Division { get; set; } = null!;
}
