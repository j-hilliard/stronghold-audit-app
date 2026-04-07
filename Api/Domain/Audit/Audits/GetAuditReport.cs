using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;
using System.Globalization;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(AuthorizationRole.AuthenticatedUser)]
public class GetAuditReport : IRequest<AuditReportDto>
{
    public int? DivisionId { get; set; }
    public string? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    /// <summary>
    /// When set, limits results to audits that have at least one NonConforming
    /// response in this section. Drives the section KPI card click-to-filter feature.
    /// </summary>
    public string? SectionFilter { get; set; }
}

public class GetAuditReportHandler : IRequestHandler<GetAuditReport, AuditReportDto>
{
    private readonly AppDbContext _context;

    public GetAuditReportHandler(AppDbContext context) => _context = context;

    public async Task<AuditReportDto> Handle(GetAuditReport request, CancellationToken cancellationToken)
    {
        var query = _context.Audits
            .Include(a => a.Division)
            .Include(a => a.Header)
            .Include(a => a.Responses)
            .Include(a => a.Findings)
            .AsQueryable();

        // Only include submitted/closed audits for reporting (drafts skew KPIs)
        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(a => a.Status == request.Status);
        else
            query = query.Where(a => a.Status == "Submitted" || a.Status == "Closed");

        if (request.DivisionId.HasValue)
            query = query.Where(a => a.DivisionId == request.DivisionId.Value);

        if (request.DateFrom.HasValue)
            query = query.Where(a => a.SubmittedAt >= request.DateFrom.Value || a.CreatedAt >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(a => a.SubmittedAt <= request.DateTo.Value || a.CreatedAt <= request.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(request.SectionFilter))
            query = query.Where(a => a.Responses.Any(r =>
                r.SectionNameSnapshot == request.SectionFilter && r.Status == "NonConforming"));

        var audits = await query.OrderByDescending(a => a.SubmittedAt ?? a.CreatedAt).ToListAsync(cancellationToken);

        // Calculate per-audit scores
        var rows = audits.Select(a =>
        {
            var scoreable = a.Responses.Where(r => r.Status != null).ToList();
            var conforming = scoreable.Count(r => r.Status == "Conforming");
            var nc = scoreable.Count(r => r.Status == "NonConforming");
            var warn = scoreable.Count(r => r.Status == "Warning");
            var denom = conforming + nc + warn;
            double? score = denom > 0 ? (double)conforming / denom * 100 : null;

            return new AuditReportRowDto
            {
                Id = a.Id,
                DivisionCode = a.Division.Code,
                Status = a.Status,
                AuditDate = a.Header?.AuditDate?.ToString("yyyy-MM-dd"),
                Auditor = a.Header?.Auditor,
                JobNumber = a.Header?.JobNumber,
                Location = a.Header?.Location,
                ScorePercent = score.HasValue ? Math.Round(score.Value, 1) : null,
                NonConformingCount = nc,
                WarningCount = warn,
            };
        }).ToList();

        // KPIs
        var scoredRows = rows.Where(r => r.ScorePercent.HasValue).ToList();
        double? avgScore = scoredRows.Any() ? Math.Round(scoredRows.Average(r => r.ScorePercent!.Value), 1) : null;

        // Trend — group by ISO week for the filtered set (or last 12 weeks if no date filter)
        var trendAudits = audits
            .Where(a => (a.SubmittedAt ?? a.CreatedAt) >= DateTime.UtcNow.AddDays(-84))
            .ToList();

        var trend = trendAudits
            .GroupBy(a =>
            {
                var d = a.SubmittedAt ?? a.CreatedAt;
                var week = ISOWeek.GetWeekOfYear(d);
                return $"{d.Year}-W{week:D2}";
            })
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var gRows = g.Select(a =>
                {
                    var s = a.Responses.Where(r => r.Status != null).ToList();
                    var c2 = s.Count(r => r.Status == "Conforming");
                    var d2 = s.Count(r => r.Status is "Conforming" or "NonConforming" or "Warning");
                    return d2 > 0 ? (double?)((double)c2 / d2 * 100) : null;
                }).Where(x => x.HasValue).Select(x => x!.Value).ToList();

                return new AuditTrendPointDto
                {
                    Week = g.Key,
                    AvgScore = gRows.Any() ? Math.Round(gRows.Average(), 1) : null,
                    AuditCount = g.Count(),
                };
            })
            .ToList();

        // Section-level NC breakdown across all filtered audits
        var allResponses = audits.SelectMany(a => a.Responses).ToList();
        var sectionBreakdown = allResponses
            .Where(r => r.Status == "NonConforming" && !string.IsNullOrWhiteSpace(r.SectionNameSnapshot))
            .GroupBy(r => r.SectionNameSnapshot!)
            .Select(g => new SectionNcBreakdownDto { SectionName = g.Key, NcCount = g.Count() })
            .OrderByDescending(x => x.NcCount)
            .ToList();

        // Open corrective actions for the filtered audit set
        var auditIds = audits.Select(a => a.Id).ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var openCas = await _context.CorrectiveActions
            .Where(ca => ca.AuditId != null && auditIds.Contains(ca.AuditId.Value) && ca.Status != "Closed")
            .OrderBy(ca => ca.DueDate)
            .ToListAsync(cancellationToken);

        var nowUtc = DateTime.UtcNow;
        var openCaSummary = openCas.Select(ca => new OpenCorrectiveActionSummaryDto
        {
            Id = ca.Id,
            AuditId = ca.AuditId ?? 0,
            Description = ca.Description,
            AssignedTo = ca.AssignedTo,
            DueDate = ca.DueDate?.ToString("yyyy-MM-dd"),
            Status = ca.Status,
            IsOverdue = ca.DueDate.HasValue && ca.DueDate.Value < today,
            DaysOpen = (int)(nowUtc - ca.CreatedAt).TotalDays,
        }).ToList();

        var correctedOnSite = allResponses.Count(r => r.Status == "NonConforming" && r.CorrectedOnSite);

        return new AuditReportDto
        {
            TotalAudits = rows.Count,
            AvgScorePercent = avgScore,
            TotalNonConforming = rows.Sum(r => r.NonConformingCount),
            TotalWarnings = rows.Sum(r => r.WarningCount),
            CorrectedOnSiteCount = correctedOnSite,
            Trend = trend,
            SectionBreakdown = sectionBreakdown,
            OpenCorrectiveActions = openCaSummary,
            Rows = rows,
        };
    }
}
