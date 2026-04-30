using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;
using System.Globalization;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.ReadOnlyViewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.Auditor, AuthorizationRole.AuditAdmin, AuthorizationRole.Executive)]
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
    private readonly IAuditUserContext _userContext;

    public GetAuditReportHandler(AppDbContext context, IAuditUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<AuditReportDto> Handle(GetAuditReport request, CancellationToken cancellationToken)
    {
        var query = _context.Audits
            .Include(a => a.Division)
            .Include(a => a.Header)
            .Include(a => a.Responses)
            .Include(a => a.Findings)
            .Include(a => a.SectionNaOverrides).ThenInclude(n => n.Section)
            .AsQueryable();

        // Division scope: scoped users only see their assigned divisions
        if (!_userContext.IsGlobal && _userContext.AllowedDivisionIds is { Count: > 0 } allowed)
            query = query.Where(a => allowed.Contains(a.DivisionId));

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

        // Calculate per-audit weighted scores (two-level: question weight within section → section weight to total)
        var rows = audits.Select(a =>
        {
            // Exclude responses from sections the auditor explicitly marked N/A
            var naSectionNames = a.SectionNaOverrides
                .Select(n => n.Section.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var scorableResponses = naSectionNames.Count > 0
                ? a.Responses.Where(r => !naSectionNames.Contains(r.SectionNameSnapshot ?? "")).ToList()
                : a.Responses;

            double? score = ComputeTwoLevelScore(scorableResponses);

            var nc = scorableResponses.Count(r => r.Status == "NonConforming");
            var warn = scorableResponses.Count(r => r.Status == "Warning");

            return new AuditReportRowDto
            {
                Id = a.Id,
                DivisionCode = a.Division.Code,
                Status = a.Status,
                AuditDate = a.Header?.AuditDate?.ToString("yyyy-MM-dd"),
                Auditor = a.Header?.Auditor,
                JobNumber = a.Header?.JobNumber,
                Location = a.Header?.Location,
                ScorePercent = score,
                NonConformingCount = nc,
                WarningCount = warn,
            };
        }).ToList();

        // KPIs
        var scoredRows = rows.Where(r => r.ScorePercent.HasValue).ToList();
        double? avgScore = scoredRows.Any() ? Math.Round(scoredRows.Average(r => r.ScorePercent!.Value), 1) : null;

        // Trend — use the full filtered set when user supplied a date filter;
        // otherwise default to last 12 weeks so the chart isn't overwhelming with all-time data.
        var trendAudits = (request.DateFrom.HasValue || request.DateTo.HasValue)
            ? audits
            : audits.Where(a => (a.SubmittedAt ?? a.CreatedAt) >= DateTime.UtcNow.AddDays(-84)).ToList();

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
                var gRows = g.Select(a => ComputeTwoLevelScore(a.Responses))
                    .Where(x => x.HasValue).Select(x => x!.Value).ToList();

                return new AuditTrendPointDto
                {
                    Week = g.Key,
                    AvgScore = gRows.Any() ? Math.Round(gRows.Average(), 1) : null,
                    AuditCount = g.Count(),
                };
            })
            .ToList();

        // Section-level NC breakdown — exclude responses from N/A sections
        var allResponses = audits.SelectMany(a =>
        {
            var naSectionNames = a.SectionNaOverrides
                .Select(n => n.Section.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            return naSectionNames.Count > 0
                ? a.Responses.Where(r => !naSectionNames.Contains(r.SectionNameSnapshot ?? ""))
                : a.Responses;
        }).ToList();
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

    /// <summary>
    /// Two-level weighted score:
    ///   1. Within each section: sectionScore = Σ(conforming × qWeight) / Σ(scored × qWeight)
    ///   2. Overall: Σ(sectionScore × sWeight) / Σ(sWeight for sections with answered questions) × 100
    /// Uses snapshot columns so historical scores are immune to template edits.
    /// </summary>
    internal static double? ComputeTwoLevelScore(IEnumerable<Data.Models.Audit.AuditResponse> responses)
    {
        var scored = responses
            .Where(r => r.Status is "Conforming" or "NonConforming" or "Warning")
            .ToList();

        if (scored.Count == 0) return null;

        var sectionScores = scored
            .GroupBy(r => r.SectionNameSnapshot ?? "")
            .Select(g =>
            {
                var sWeight = (double)g.First().SectionWeightSnapshot;
                var denom = g.Sum(r => (double)r.QuestionWeightSnapshot);
                if (denom == 0) return (sWeight, sScore: (double?)null);
                var numer = g.Where(r => r.Status == "Conforming").Sum(r => (double)r.QuestionWeightSnapshot);
                return (sWeight, sScore: (double?)(numer / denom));
            })
            .Where(s => s.sScore.HasValue)
            .ToList();

        if (sectionScores.Count == 0) return null;

        var totalSectionWeight = sectionScores.Sum(s => s.sWeight);
        if (totalSectionWeight == 0) return null;

        var weightedSum = sectionScores.Sum(s => s.sScore!.Value * s.sWeight);
        return Math.Round(weightedSum / totalSectionWeight * 100, 1);
    }
}
