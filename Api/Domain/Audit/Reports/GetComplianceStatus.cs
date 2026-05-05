using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Reports;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.ReadOnlyViewer, AuthorizationRole.ExecutiveViewer,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin, AuthorizationRole.Executive)]
public class GetComplianceStatus : IRequest<List<ComplianceStatusDto>> { }

public class GetComplianceStatusHandler : IRequestHandler<GetComplianceStatus, List<ComplianceStatusDto>>
{
    private readonly AppDbContext _db;
    private readonly IAuditUserContext _userContext;

    public GetComplianceStatusHandler(AppDbContext db, IAuditUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<List<ComplianceStatusDto>> Handle(GetComplianceStatus request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Load active divisions scoped to this user's allowed set
        var divisionsQuery = _db.Divisions
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .AsQueryable();

        if (!_userContext.IsGlobal && _userContext.AllowedDivisionIds is { Count: > 0 } allowed)
            divisionsQuery = divisionsQuery.Where(d => allowed.Contains(d.Id));

        var divisions = await divisionsQuery.ToListAsync(cancellationToken);

        // For each division, find the most recent submitted/closed audit date
        var divisionIds = divisions.Select(d => d.Id).ToList();

        var lastAuditDates = await _db.Audits
            .Where(a => divisionIds.Contains(a.DivisionId) &&
                        (a.Status == "Submitted" || a.Status == "Closed") &&
                        a.SubmittedAt != null)
            .GroupBy(a => a.DivisionId)
            .Select(g => new
            {
                DivisionId = g.Key,
                LastSubmittedAt = g.Max(a => a.SubmittedAt)
            })
            .ToDictionaryAsync(x => x.DivisionId, x => x.LastSubmittedAt, cancellationToken);

        var result = new List<ComplianceStatusDto>(divisions.Count);

        foreach (var div in divisions)
        {
            lastAuditDates.TryGetValue(div.Id, out var lastSubmittedAt);
            var lastAuditDate = lastSubmittedAt.HasValue
                ? DateOnly.FromDateTime(lastSubmittedAt.Value)
                : (DateOnly?)null;

            int? daysSince = lastAuditDate.HasValue
                ? today.DayNumber - lastAuditDate.Value.DayNumber
                : null;

            int? daysUntilDue = null;
            string status;

            if (div.AuditFrequencyDays == null)
            {
                status = "NoSchedule";
            }
            else if (lastAuditDate == null)
            {
                status = "NeverAudited";
            }
            else
            {
                daysUntilDue = div.AuditFrequencyDays.Value - daysSince!.Value;

                status = daysUntilDue switch
                {
                    < 0 => "Overdue",
                    <= 7 => "DueSoon",
                    _ => "OnTrack"
                };
            }

            result.Add(new ComplianceStatusDto
            {
                DivisionId = div.Id,
                DivisionCode = div.Code,
                DivisionName = div.Name,
                LastAuditDate = lastAuditDate,
                DaysSinceLastAudit = daysSince,
                FrequencyDays = div.AuditFrequencyDays,
                DaysUntilDue = daysUntilDue,
                Status = status,
            });
        }

        return result;
    }
}
