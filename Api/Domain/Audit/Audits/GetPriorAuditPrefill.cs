using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

public class PriorAuditPrefillDto
{
    public bool HasPrior { get; set; }
    /// <summary>ISO date "YYYY-MM-DD" of the prior audit, or null.</summary>
    public string? AuditDate { get; set; }
    /// <summary>Map of questionId → "Conforming" for each conforming answer in the prior audit.</summary>
    public Dictionary<int, string> Responses { get; set; } = new();
}

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.ReadOnlyViewer,
    AuthorizationRole.Auditor, AuthorizationRole.AuditAdmin)]
public class GetPriorAuditPrefill : IRequest<PriorAuditPrefillDto>
{
    public int DivisionId         { get; set; }
    public int TemplateVersionId  { get; set; }
}

public class GetPriorAuditPrefillHandler : IRequestHandler<GetPriorAuditPrefill, PriorAuditPrefillDto>
{
    private readonly AppDbContext _context;

    public GetPriorAuditPrefillHandler(AppDbContext context) => _context = context;

    public async Task<PriorAuditPrefillDto> Handle(GetPriorAuditPrefill request, CancellationToken cancellationToken)
    {
        // Find the most recent Submitted or Closed audit for the same division + template version.
        var priorAudit = await _context.Audits
            .Where(a =>
                a.DivisionId        == request.DivisionId &&
                a.TemplateVersionId == request.TemplateVersionId &&
                (a.Status == "Submitted" || a.Status == "Closed"))
            .OrderByDescending(a => a.SubmittedAt)
            .Select(a => new
            {
                a.Id,
                AuditDate = a.Header != null ? a.Header.AuditDate : null,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (priorAudit == null)
            return new PriorAuditPrefillDto { HasPrior = false };

        // Fetch only conforming responses from that audit (by stable questionId).
        var conformingResponses = await _context.AuditResponses
            .Where(r => r.AuditId == priorAudit.Id && r.Status == "Conforming")
            .Select(r => r.QuestionId)
            .ToListAsync(cancellationToken);

        if (conformingResponses.Count == 0)
            return new PriorAuditPrefillDto { HasPrior = true, AuditDate = priorAudit.AuditDate?.ToString("yyyy-MM-dd") };

        var responseMap = conformingResponses.ToDictionary(qId => qId, _ => "Conforming");

        return new PriorAuditPrefillDto
        {
            HasPrior  = true,
            AuditDate = priorAudit.AuditDate?.ToString("yyyy-MM-dd"),
            Responses = responseMap,
        };
    }
}
