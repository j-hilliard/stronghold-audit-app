namespace Stronghold.AppDashboard.Data.Models.Audit;

public class AuditDistributionRecipient
{
    public int Id { get; set; }
    public int AuditId { get; set; }
    public string EmailAddress { get; set; } = null!;
    public string? Name { get; set; }
    public string AddedBy { get; set; } = null!;
    public DateTime AddedAt { get; set; }

    // Navigation
    public Audit Audit { get; set; } = null!;
}
