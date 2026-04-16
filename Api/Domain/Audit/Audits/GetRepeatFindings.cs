using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

/// <summary>
/// A finding is "recurrent" when the same SectionNameSnapshot + QuestionTextSnapshot
/// combination appears as NonConforming in 2+ submitted audits for the same division
/// within a rolling 180-day window ending at the target audit's date.
/// </summary>
public class RepeatFindingDto
{
    public int QuestionId { get; set; }
    public string QuestionTextSnapshot { get; set; } = null!;
    public string? SectionNameSnapshot { get; set; }
    /// <summary>How many times this question was NonConforming in the lookback window (including the current audit).</summary>
    public int OccurrenceCount { get; set; }
}

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.ReadOnlyViewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class GetRepeatFindings : IRequest<List<RepeatFindingDto>>
{
    public int AuditId { get; set; }
}

public class GetRepeatFindingsHandler : IRequestHandler<GetRepeatFindings, List<RepeatFindingDto>>
{
    private const int LookbackDays = 180;
    private const int MinOccurrences = 2;

    private readonly AppDbContext _db;
    public GetRepeatFindingsHandler(AppDbContext db) => _db = db;

    public async Task<List<RepeatFindingDto>> Handle(GetRepeatFindings request, CancellationToken ct)
    {
        // Load the target audit to get its division and reference date
        var audit = await _db.Audits
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, ct);

        if (audit == null) return new List<RepeatFindingDto>();

        // Use SubmittedAt if available (Submitted/Closed audits), else now (Draft/Reopened)
        var refDate = audit.SubmittedAt ?? DateTime.UtcNow;
        var lookbackFrom = refDate.AddDays(-LookbackDays);

        // Find all submitted audits in the same division within the lookback window
        var divisionAuditIds = await _db.Audits
            .AsNoTracking()
            .Where(a => a.DivisionId == audit.DivisionId
                     && a.Status != "Draft"
                     && (a.SubmittedAt == null || a.SubmittedAt >= lookbackFrom)
                     && (a.SubmittedAt == null || a.SubmittedAt <= refDate))
            .Select(a => a.Id)
            .ToListAsync(ct);

        if (!divisionAuditIds.Any()) return new List<RepeatFindingDto>();

        // Get all NonConforming responses from those audits
        var ncResponses = await _db.AuditResponses
            .AsNoTracking()
            .Where(r => divisionAuditIds.Contains(r.AuditId) && r.Status == "NonConforming")
            .Select(r => new
            {
                r.AuditId,
                r.QuestionId,
                r.QuestionTextSnapshot,
                r.SectionNameSnapshot,
            })
            .ToListAsync(ct);

        // Find combinations that appear in 2+ different audits
        var repeatFindings = ncResponses
            .GroupBy(r => new { r.QuestionTextSnapshot, r.SectionNameSnapshot })
            .Select(g => new
            {
                g.Key.QuestionTextSnapshot,
                g.Key.SectionNameSnapshot,
                DistinctAuditCount = g.Select(r => r.AuditId).Distinct().Count(),
                QuestionId = g.Select(r => r.QuestionId).First(),
            })
            .Where(g => g.DistinctAuditCount >= MinOccurrences)
            .Select(g => new RepeatFindingDto
            {
                QuestionId = g.QuestionId,
                QuestionTextSnapshot = g.QuestionTextSnapshot,
                SectionNameSnapshot = g.SectionNameSnapshot,
                OccurrenceCount = g.DistinctAuditCount,
            })
            .OrderByDescending(r => r.OccurrenceCount)
            .ToList();

        return repeatFindings;
    }
}
