using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class GetAuditsByEmployee : IRequest<List<AuditsByEmployeeDto>>
{
    public int? DivisionId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class AuditsByEmployeeDto
{
    public string Auditor { get; set; } = null!;
    public int AuditCount { get; set; }
    public double? AvgScorePercent { get; set; }
    public int TotalNonConforming { get; set; }
    public int TotalWarnings { get; set; }
    public string? LastAuditDate { get; set; }
    public string? LastDivisionCode { get; set; }
}

public class GetAuditsByEmployeeHandler : IRequestHandler<GetAuditsByEmployee, List<AuditsByEmployeeDto>>
{
    private readonly AppDbContext _db;
    private readonly IAuditUserContext _userContext;

    public GetAuditsByEmployeeHandler(AppDbContext db, IAuditUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<List<AuditsByEmployeeDto>> Handle(GetAuditsByEmployee request, CancellationToken ct)
    {
        var query = _db.Audits
            .Include(a => a.Division)
            .Include(a => a.Header)
            .Include(a => a.Responses)
            .Where(a => a.Status == "Submitted" || a.Status == "Closed")
            .AsQueryable();

        if (!_userContext.IsGlobal && _userContext.AllowedDivisionIds is { Count: > 0 } allowed)
            query = query.Where(a => allowed.Contains(a.DivisionId));
        if (request.DivisionId.HasValue)
            query = query.Where(a => a.DivisionId == request.DivisionId.Value);
        if (request.DateFrom.HasValue)
            query = query.Where(a => (a.SubmittedAt ?? a.CreatedAt) >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(a => (a.SubmittedAt ?? a.CreatedAt) <= request.DateTo.Value);

        var audits = await query
            .Where(a => !string.IsNullOrEmpty(a.Header!.Auditor))
            .OrderByDescending(a => a.SubmittedAt ?? a.CreatedAt)
            .ToListAsync(ct);

        var result = audits
            .GroupBy(a => a.Header!.Auditor!)
            .Select(g =>
            {
                var scores = g
                    .Select(a => GetAuditReportHandler.ComputeTwoLevelScore(a.Responses))
                    .Where(s => s.HasValue)
                    .Select(s => s!.Value)
                    .ToList();

                var latest = g.OrderByDescending(a => a.SubmittedAt ?? a.CreatedAt).First();

                return new AuditsByEmployeeDto
                {
                    Auditor = g.Key,
                    AuditCount = g.Count(),
                    AvgScorePercent = scores.Any() ? Math.Round(scores.Average(), 1) : null,
                    TotalNonConforming = g.Sum(a => a.Responses.Count(r => r.Status == "NonConforming")),
                    TotalWarnings = g.Sum(a => a.Responses.Count(r => r.Status == "Warning")),
                    LastAuditDate = latest.Header?.AuditDate?.ToString("yyyy-MM-dd")
                        ?? latest.SubmittedAt?.ToString("yyyy-MM-dd"),
                    LastDivisionCode = latest.Division.Code,
                };
            })
            .OrderByDescending(x => x.AuditCount)
            .ToList();

        return result;
    }
}
