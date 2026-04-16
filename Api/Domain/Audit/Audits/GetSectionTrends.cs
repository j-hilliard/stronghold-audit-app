using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.ReadOnlyViewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class GetSectionTrends : IRequest<SectionTrendsReportDto>
{
    public int? DivisionId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetSectionTrendsHandler : IRequestHandler<GetSectionTrends, SectionTrendsReportDto>
{
    private readonly AppDbContext _context;

    public GetSectionTrendsHandler(AppDbContext context) => _context = context;

    public async Task<SectionTrendsReportDto> Handle(GetSectionTrends request, CancellationToken cancellationToken)
    {
        var auditQuery = _context.Audits
            .Where(a => a.Status == "Submitted" || a.Status == "Closed")
            .Select(a => new
            {
                a.Id,
                a.DivisionId,
                EffectiveDate = a.SubmittedAt ?? a.CreatedAt
            })
            .AsQueryable();

        if (request.DateFrom.HasValue)
            auditQuery = auditQuery.Where(a => a.EffectiveDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            auditQuery = auditQuery.Where(a => a.EffectiveDate <= request.DateTo.Value);

        var audits = await auditQuery.ToListAsync(cancellationToken);
        if (audits.Count == 0)
        {
            return new SectionTrendsReportDto();
        }

        var auditIds = audits.Select(a => a.Id).ToHashSet();
        var auditQuarterById = audits.ToDictionary(
            a => a.Id,
            a => ToQuarterKey(a.EffectiveDate));

        var companyAuditCounts = audits
            .GroupBy(a => ToQuarterKey(a.EffectiveDate))
            .ToDictionary(g => g.Key, g => g.Count());

        var divisionAuditIds = request.DivisionId.HasValue
            ? audits.Where(a => a.DivisionId == request.DivisionId.Value).Select(a => a.Id).ToHashSet()
            : audits.Select(a => a.Id).ToHashSet();

        var divisionAuditCounts = audits
            .Where(a => divisionAuditIds.Contains(a.Id))
            .GroupBy(a => ToQuarterKey(a.EffectiveDate))
            .ToDictionary(g => g.Key, g => g.Count());

        var nonConformingResponses = await _context.AuditResponses
            .Where(r =>
                auditIds.Contains(r.AuditId) &&
                r.Status == "NonConforming" &&
                r.SectionNameSnapshot != null &&
                r.SectionNameSnapshot != "")
            .Select(r => new
            {
                r.AuditId,
                SectionName = r.SectionNameSnapshot!
            })
            .ToListAsync(cancellationToken);

        var companyNcCounts = nonConformingResponses
            .GroupBy(r =>
            {
                var quarter = auditQuarterById[r.AuditId];
                return (Section: r.SectionName, Quarter: quarter);
            })
            .ToDictionary(g => g.Key, g => g.Count());

        var divisionNcCounts = nonConformingResponses
            .Where(r => divisionAuditIds.Contains(r.AuditId))
            .GroupBy(r =>
            {
                var quarter = auditQuarterById[r.AuditId];
                return (Section: r.SectionName, Quarter: quarter);
            })
            .ToDictionary(g => g.Key, g => g.Count());

        var orderedQuarters = companyAuditCounts.Keys
            .Union(divisionAuditCounts.Keys)
            .OrderBy(q => q.Year)
            .ThenBy(q => q.Quarter)
            .ToList();

        var sectionNames = companyNcCounts.Keys.Select(k => k.Section)
            .Union(divisionNcCounts.Keys.Select(k => k.Section))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name)
            .ToList();

        var sectionSeries = new List<SectionTrendDto>();
        foreach (var section in sectionNames)
        {
            var divisionTrend = new List<SectionTrendPointDto>(orderedQuarters.Count);
            var companyTrend = new List<SectionTrendPointDto>(orderedQuarters.Count);

            foreach (var quarter in orderedQuarters)
            {
                var divisionAuditCount = divisionAuditCounts.TryGetValue(quarter, out var dac) ? dac : 0;
                var companyAuditCount = companyAuditCounts.TryGetValue(quarter, out var cac) ? cac : 0;

                var divisionNcCount = divisionNcCounts.TryGetValue((section, quarter), out var dnc) ? dnc : 0;
                var companyNcCount = companyNcCounts.TryGetValue((section, quarter), out var cnc) ? cnc : 0;

                var quarterLabel = quarter.ToLabel();

                divisionTrend.Add(new SectionTrendPointDto
                {
                    Quarter = quarterLabel,
                    AuditCount = divisionAuditCount,
                    NcCount = divisionNcCount,
                    FindingsPerAudit = divisionAuditCount > 0
                        ? Math.Round((double)divisionNcCount / divisionAuditCount, 4)
                        : 0d,
                });

                companyTrend.Add(new SectionTrendPointDto
                {
                    Quarter = quarterLabel,
                    AuditCount = companyAuditCount,
                    NcCount = companyNcCount,
                    FindingsPerAudit = companyAuditCount > 0
                        ? Math.Round((double)companyNcCount / companyAuditCount, 4)
                        : 0d,
                });
            }

            sectionSeries.Add(new SectionTrendDto
            {
                SectionName = section,
                DivisionTrend = divisionTrend,
                CompanyTrend = companyTrend,
            });
        }

        return new SectionTrendsReportDto
        {
            Quarters = orderedQuarters.Select(q => q.ToLabel()).ToList(),
            Sections = sectionSeries,
        };
    }

    private static QuarterKey ToQuarterKey(DateTime date)
    {
        var quarter = ((date.Month - 1) / 3) + 1;
        return new QuarterKey(date.Year, quarter);
    }

    private readonly record struct QuarterKey(int Year, int Quarter)
    {
        public string ToLabel() => $"{Year} Q{Quarter}";
    }
}
