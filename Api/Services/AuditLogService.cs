using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;

namespace Stronghold.AppDashboard.Api.Services;

public interface IAuditLogService
{
    Task LogAsync(
        string performedBy,
        string action,
        string entityType,
        string description,
        string? entityId    = null,
        string severity     = "Info");
}

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _http;

    public AuditLogService(AppDbContext context, IHttpContextAccessor http)
    {
        _context = context;
        _http    = http;
    }

    public async Task LogAsync(
        string performedBy,
        string action,
        string entityType,
        string description,
        string? entityId = null,
        string severity  = "Info")
    {
        try
        {
            var entry = new AuditActionLog
            {
                Timestamp   = DateTime.UtcNow,
                PerformedBy = performedBy,
                Action      = action,
                EntityType  = entityType,
                Description = description,
                EntityId    = entityId,
                Severity    = severity,
                IpAddress   = _http.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                RequestPath = _http.HttpContext?.Request?.Path.Value,
            };

            _context.AuditActionLogs.Add(entry);
            await _context.SaveChangesAsync();
        }
        catch
        {
            _context.ChangeTracker.Clear();
        }
    }
}
