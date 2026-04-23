namespace Stronghold.AppDashboard.Data.Models.Audit;

/// <summary>
/// Low-level EF change capture — every insert/update/delete on tracked entities.
/// Written automatically by AuditTrailInterceptor, never by application code.
/// OldValues/NewValues are JSON snapshots of the changed columns only.
/// </summary>
public class AuditTrailLog
{
    public long Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserEmail { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public string EntityId { get; set; } = null!;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? ChangedColumns { get; set; }
    public string? IpAddress { get; set; }
}
