using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(AuthorizationRole.Administrator, AuthorizationRole.AuditAdmin, AuthorizationRole.ITAdmin)]
public class GetAuditLogs : IRequest<AuditLogsResult>
{
    public DateTime? DateFrom   { get; set; }
    public DateTime? DateTo     { get; set; }
    public string?   UserEmail  { get; set; }
    public string?   EntityType { get; set; }
    public string?   Action     { get; set; }
    public string?   Search     { get; set; }
    public int       Page       { get; set; } = 1;
    public int       PageSize   { get; set; } = 50;
}

public class AuditLogsResult
{
    public List<AuditActionLogDto> ActionLogs  { get; set; } = [];
    public List<AuditTrailLogDto>  TrailLogs   { get; set; } = [];
    public int TotalActionLogs { get; set; }
    public int TotalTrailLogs  { get; set; }
}

public class AuditActionLogDto
{
    public int      Id          { get; set; }
    public DateTime Timestamp   { get; set; }
    public string   PerformedBy { get; set; } = null!;
    public string   Action      { get; set; } = null!;
    public string   EntityType  { get; set; } = null!;
    public string?  EntityId    { get; set; }
    public string   Description { get; set; } = null!;
    public string   Severity    { get; set; } = null!;
    public string?  IpAddress   { get; set; }
}

public class AuditTrailLogDto
{
    public long     Id             { get; set; }
    public DateTime Timestamp      { get; set; }
    public string   UserEmail      { get; set; } = null!;
    public string   Action         { get; set; } = null!;
    public string   EntityType     { get; set; } = null!;
    public string   EntityId       { get; set; } = null!;
    public string?  OldValues      { get; set; }
    public string?  NewValues      { get; set; }
    public string?  ChangedColumns { get; set; }
    public string?  IpAddress      { get; set; }
}

public class GetAuditLogsHandler : IRequestHandler<GetAuditLogs, AuditLogsResult>
{
    private readonly AppDbContext _context;

    public GetAuditLogsHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AuditLogsResult> Handle(GetAuditLogs request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        // ── Action logs ───────────────────────────────────────────────────────
        var actionQ = _context.AuditActionLogs.AsNoTracking();

        if (request.DateFrom.HasValue)
            actionQ = actionQ.Where(l => l.Timestamp >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            actionQ = actionQ.Where(l => l.Timestamp <= request.DateTo.Value);
        if (!string.IsNullOrEmpty(request.UserEmail))
            actionQ = actionQ.Where(l => l.PerformedBy.Contains(request.UserEmail));
        if (!string.IsNullOrEmpty(request.EntityType))
            actionQ = actionQ.Where(l => l.EntityType == request.EntityType);
        if (!string.IsNullOrEmpty(request.Action))
            actionQ = actionQ.Where(l => l.Action == request.Action);
        if (!string.IsNullOrEmpty(request.Search))
            actionQ = actionQ.Where(l => l.Description.Contains(request.Search) || l.EntityId!.Contains(request.Search));

        var totalAction = await actionQ.CountAsync(cancellationToken);
        var actionLogs  = await actionQ
            .OrderByDescending(l => l.Timestamp)
            .Skip(skip).Take(request.PageSize)
            .Select(l => new AuditActionLogDto
            {
                Id          = l.Id,
                Timestamp   = l.Timestamp,
                PerformedBy = l.PerformedBy,
                Action      = l.Action,
                EntityType  = l.EntityType,
                EntityId    = l.EntityId,
                Description = l.Description,
                Severity    = l.Severity,
                IpAddress   = l.IpAddress,
            })
            .ToListAsync(cancellationToken);

        // ── Trail logs ────────────────────────────────────────────────────────
        var trailQ = _context.AuditTrailLogs.AsNoTracking();

        if (request.DateFrom.HasValue)
            trailQ = trailQ.Where(l => l.Timestamp >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            trailQ = trailQ.Where(l => l.Timestamp <= request.DateTo.Value);
        if (!string.IsNullOrEmpty(request.UserEmail))
            trailQ = trailQ.Where(l => l.UserEmail.Contains(request.UserEmail));
        if (!string.IsNullOrEmpty(request.EntityType))
            trailQ = trailQ.Where(l => l.EntityType == request.EntityType);
        if (!string.IsNullOrEmpty(request.Action))
            trailQ = trailQ.Where(l => l.Action == request.Action);
        if (!string.IsNullOrEmpty(request.Search))
            trailQ = trailQ.Where(l => l.EntityId.Contains(request.Search) || l.UserEmail.Contains(request.Search));

        var totalTrail = await trailQ.CountAsync(cancellationToken);
        var trailLogs  = await trailQ
            .OrderByDescending(l => l.Timestamp)
            .Skip(skip).Take(request.PageSize)
            .Select(l => new AuditTrailLogDto
            {
                Id             = l.Id,
                Timestamp      = l.Timestamp,
                UserEmail      = l.UserEmail,
                Action         = l.Action,
                EntityType     = l.EntityType,
                EntityId       = l.EntityId,
                OldValues      = l.OldValues,
                NewValues      = l.NewValues,
                ChangedColumns = l.ChangedColumns,
                IpAddress      = l.IpAddress,
            })
            .ToListAsync(cancellationToken);

        return new AuditLogsResult
        {
            ActionLogs      = actionLogs,
            TrailLogs       = trailLogs,
            TotalActionLogs = totalAction,
            TotalTrailLogs  = totalTrail,
        };
    }
}
