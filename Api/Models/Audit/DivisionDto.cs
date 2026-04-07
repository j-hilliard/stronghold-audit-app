namespace Stronghold.AppDashboard.Api.Models.Audit;

public class DivisionDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;

    /// <summary>"JobSite" | "Facility" — determines which header fields are shown</summary>
    public string AuditType { get; set; } = null!;
}
