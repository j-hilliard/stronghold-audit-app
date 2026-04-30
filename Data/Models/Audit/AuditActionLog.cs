namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Human-readable action log — every meaningful business event (submit, delete, role change, etc.)
/// Written explicitly by service/handler code via IAuditLogService.
/// </summary>
public class AuditActionLog
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string PerformedBy { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public string? EntityId { get; set; }
    public string Description { get; set; } = null!;
    public string Severity { get; set; } = "Info";
    public string? IpAddress { get; set; }
    public string? RequestPath { get; set; }
}
