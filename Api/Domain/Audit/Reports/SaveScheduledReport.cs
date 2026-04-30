using MediatR;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;
using Stronghold.AppDashboard.Data.Models.Audit;
using System.Text.Json;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Reports;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager,
    AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin)]
public class SaveScheduledReport : IRequest<ScheduledReportDto>
{
    public SaveScheduledReportRequest Payload { get; set; } = null!;
    public string SavedBy { get; set; } = null!;
}

public class SaveScheduledReportHandler : IRequestHandler<SaveScheduledReport, ScheduledReportDto>
{
    private readonly AppDbContext _db;

    public SaveScheduledReportHandler(AppDbContext db) => _db = db;

    public async Task<ScheduledReportDto> Handle(SaveScheduledReport request, CancellationToken ct)
    {
        var p = request.Payload;
        ScheduledReport? entity;

        if (p.Id.HasValue)
        {
            entity = await _db.ScheduledReports.FindAsync(new object[] { p.Id.Value }, ct)
                     ?? throw new KeyNotFoundException($"Scheduled report {p.Id} not found.");
        }
        else
        {
            entity = new ScheduledReport { CreatedBy = request.SavedBy };
            _db.ScheduledReports.Add(entity);
        }

        entity.DivisionId      = p.DivisionId;
        entity.TemplateId      = p.TemplateId;
        entity.Title           = p.Title;
        entity.Frequency       = p.Frequency;
        entity.DayOfWeek       = p.DayOfWeek;
        entity.DayOfMonth      = p.DayOfMonth;
        entity.TimeUtc         = p.TimeUtc;
        entity.DateRangePreset = p.DateRangePreset;
        entity.RecipientsJson  = JsonSerializer.Serialize(p.Recipients);
        entity.PrimaryColor    = p.PrimaryColor;
        entity.ScoreThreshold  = p.ScoreThreshold;
        entity.IsActive        = p.IsActive;
        entity.NextRunAt       = ComputeNextRun(p.Frequency, p.TimeUtc, p.DayOfWeek, p.DayOfMonth);

        await _db.SaveChangesAsync(ct);

        await _db.Entry(entity).Reference(r => r.Division).LoadAsync(ct);
        return GetScheduledReportsHandler.ToDto(entity);
    }

    internal static DateTime ComputeNextRun(string frequency, string timeUtc, int? dow, int? dom)
    {
        var parts = timeUtc.Split(':');
        int h = int.TryParse(parts.ElementAtOrDefault(0), out var hh) ? hh : 7;
        int m = int.TryParse(parts.ElementAtOrDefault(1), out var mm) ? mm : 0;

        var now = DateTime.UtcNow;
        var candidate = new DateTime(now.Year, now.Month, now.Day, h, m, 0, DateTimeKind.Utc);
        if (candidate <= now) candidate = candidate.AddDays(1);

        return frequency switch
        {
            "Daily" => candidate,
            "Weekly" when dow.HasValue =>
                AdvanceToWeekday(candidate, (DayOfWeek)dow.Value),
            "Monthly" when dom.HasValue =>
                AdvanceToMonthDay(candidate, dom.Value),
            "Quarterly" =>
                AdvanceToNextQuarter(candidate),
            _ => candidate,
        };
    }

    private static DateTime AdvanceToWeekday(DateTime from, DayOfWeek target)
    {
        int diff = ((int)target - (int)from.DayOfWeek + 7) % 7;
        return from.AddDays(diff == 0 ? 7 : diff);
    }

    private static DateTime AdvanceToMonthDay(DateTime from, int day)
    {
        day = Math.Clamp(day, 1, 28);
        var next = new DateTime(from.Year, from.Month, day, from.Hour, from.Minute, 0, DateTimeKind.Utc);
        if (next <= from) next = next.AddMonths(1);
        return next;
    }

    private static DateTime AdvanceToNextQuarter(DateTime from)
    {
        var months = new[] { 1, 4, 7, 10 };
        var targetMonth = months.FirstOrDefault(m => m > from.Month, 1);
        var targetYear = targetMonth <= from.Month ? from.Year + 1 : from.Year;
        return new DateTime(targetYear, targetMonth, 1, from.Hour, from.Minute, 0, DateTimeKind.Utc);
    }
}
