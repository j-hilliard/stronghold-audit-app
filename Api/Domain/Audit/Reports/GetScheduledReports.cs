using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;
using Stronghold.AppDashboard.Data.Models.Audit;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Reports;

public class ScheduledReportDto
{
    public int Id { get; set; }
    public int? DivisionId { get; set; }
    public string? DivisionCode { get; set; }
    public string TemplateId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Frequency { get; set; } = null!;
    public int? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public string TimeUtc { get; set; } = "07:00";
    public string? DateRangePreset { get; set; }
    public List<string> Recipients { get; set; } = new();
    public string? PrimaryColor { get; set; }
    public decimal? ScoreThreshold { get; set; }
    public DateTime? LastRunAt { get; set; }
    public DateTime NextRunAt { get; set; }
    public bool IsActive { get; set; }
}

public class SaveScheduledReportRequest
{
    public int? Id { get; set; }
    public int? DivisionId { get; set; }
    public string TemplateId { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Frequency { get; set; } = null!;
    public int? DayOfWeek { get; set; }
    public int? DayOfMonth { get; set; }
    public string TimeUtc { get; set; } = "07:00";
    public string? DateRangePreset { get; set; }
    public List<string> Recipients { get; set; } = new();
    public string? PrimaryColor { get; set; }
    public decimal? ScoreThreshold { get; set; }
    public bool IsActive { get; set; } = true;
}

// ── Get ────────────────────────────────────────────────────────────────────────

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin, AuthorizationRole.Executive)]
public class GetScheduledReports : IRequest<List<ScheduledReportDto>>
{
    public int? DivisionId { get; set; }
}

public class GetScheduledReportsHandler : IRequestHandler<GetScheduledReports, List<ScheduledReportDto>>
{
    private readonly AppDbContext _db;
    private readonly IAuditUserContext _userContext;

    public GetScheduledReportsHandler(AppDbContext db, IAuditUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<List<ScheduledReportDto>> Handle(GetScheduledReports request, CancellationToken ct)
    {
        var query = _db.ScheduledReports
            .Include(r => r.Division)
            .Where(r => r.IsActive)
            .AsQueryable();

        if (!_userContext.IsGlobal && _userContext.AllowedDivisionIds is { Count: > 0 } allowed)
            query = query.Where(r => r.DivisionId == null || allowed.Contains(r.DivisionId.Value));

        if (request.DivisionId.HasValue)
            query = query.Where(r => r.DivisionId == request.DivisionId.Value || r.DivisionId == null);

        var results = await query.OrderBy(r => r.Title).ToListAsync(ct);
        return results.Select(ToDto).ToList();
    }

    internal static ScheduledReportDto ToDto(ScheduledReport r) => new()
    {
        Id = r.Id,
        DivisionId = r.DivisionId,
        DivisionCode = r.Division?.Code,
        TemplateId = r.TemplateId,
        Title = r.Title,
        Frequency = r.Frequency,
        DayOfWeek = r.DayOfWeek,
        DayOfMonth = r.DayOfMonth,
        TimeUtc = r.TimeUtc,
        DateRangePreset = r.DateRangePreset,
        Recipients = System.Text.Json.JsonSerializer.Deserialize<List<string>>(r.RecipientsJson) ?? new(),
        PrimaryColor = r.PrimaryColor,
        ScoreThreshold = r.ScoreThreshold,
        LastRunAt = r.LastRunAt,
        NextRunAt = r.NextRunAt,
        IsActive = r.IsActive,
    };
}
