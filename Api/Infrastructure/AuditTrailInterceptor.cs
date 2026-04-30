using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Stronghold.AppDashboard.Data.Models.Audit;

namespace Stronghold.AppDashboard.Api.Infrastructure;

/// <summary>
/// EF Core interceptor that automatically captures every insert/update/delete on
/// tracked entity types and writes a row to AuditTrailLog.
/// Registered as a singleton — uses a ConcurrentDictionary keyed by DbContext
/// instance to safely hold per-request pending entries.
/// </summary>
public class AuditTrailInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _http;

    private static readonly HashSet<string> TrackedTypes = new(StringComparer.Ordinal)
    {
        "Audit", "CorrectiveAction", "UserRole", "User",
        "AuditTemplateVersion", "Division", "AuditFinding"
    };

    private readonly ConcurrentDictionary<DbContext, List<AuditTrailLog>> _pending = new();

    public AuditTrailInterceptor(IHttpContextAccessor http)
    {
        _http = http;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            CaptureEntries(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
            await WriteEntriesAsync(eventData.Context, cancellationToken);

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        if (eventData.Context is not null)
            _pending.TryRemove(eventData.Context, out _);

        base.SaveChangesFailed(eventData);
    }

    // ── Private ──────────────────────────────────────────────────────────────

    private void CaptureEntries(DbContext context)
    {
        var user = _http.HttpContext?.User?.FindFirst("preferred_username")?.Value
                ?? _http.HttpContext?.User?.Identity?.Name
                ?? "system";
        var ip   = _http.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        var now  = DateTime.UtcNow;

        var entries = new List<AuditTrailLog>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            var typeName = entry.Entity.GetType().Name;
            if (!TrackedTypes.Contains(typeName))
                continue;

            var pkProp  = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            var entityId = pkProp?.CurrentValue?.ToString() ?? "?";

            string action = entry.State switch
            {
                EntityState.Added    => "Insert",
                EntityState.Modified => "Update",
                EntityState.Deleted  => "Delete",
                _                    => "Unknown"
            };

            string? oldValues     = null;
            string? newValues     = null;
            string? changedCols   = null;

            if (entry.State == EntityState.Modified)
            {
                var changed = entry.Properties
                    .Where(p => p.IsModified)
                    .ToList();

                changedCols = JsonSerializer.Serialize(changed.Select(p => p.Metadata.Name));
                oldValues   = JsonSerializer.Serialize(
                    changed.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
                newValues   = JsonSerializer.Serialize(
                    changed.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
            }
            else if (entry.State == EntityState.Deleted)
            {
                oldValues = JsonSerializer.Serialize(
                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue));
            }
            else if (entry.State == EntityState.Added)
            {
                newValues = JsonSerializer.Serialize(
                    entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue));
            }

            entries.Add(new AuditTrailLog
            {
                Timestamp      = now,
                UserEmail      = user,
                Action         = action,
                EntityType     = typeName,
                EntityId       = entityId,
                OldValues      = oldValues,
                NewValues      = newValues,
                ChangedColumns = changedCols,
                IpAddress      = ip,
            });
        }

        if (entries.Count > 0)
            _pending[context] = entries;
    }

    private async Task WriteEntriesAsync(DbContext context, CancellationToken ct)
    {
        if (!_pending.TryRemove(context, out var entries) || entries.Count == 0)
            return;

        // Insert directly via parameterized SQL to avoid re-triggering the interceptor.
        foreach (var e in entries)
        {
            await context.Database.ExecuteSqlAsync(
                $"""
                INSERT INTO [audit].[AuditTrailLog]
                    ([Timestamp],[UserEmail],[Action],[EntityType],[EntityId],
                     [OldValues],[NewValues],[ChangedColumns],[IpAddress])
                VALUES
                    ({e.Timestamp},{e.UserEmail},{e.Action},{e.EntityType},{e.EntityId},
                     {e.OldValues},{e.NewValues},{e.ChangedColumns},{e.IpAddress})
                """,
                ct);
        }
    }
}
