using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.ReportDrafts;

[AllowedAuthorizationRole(AuthorizationRole.AuthenticatedUser)]
public class GetReportDrafts : IRequest<List<ReportDraftListItemDto>>
{
    public int? DivisionId { get; set; }
    public string RequestedBy { get; set; } = null!;
}

public class GetReportDraftsHandler : IRequestHandler<GetReportDrafts, List<ReportDraftListItemDto>>
{
    private readonly AppDbContext _context;

    public GetReportDraftsHandler(AppDbContext context) => _context = context;

    public async Task<List<ReportDraftListItemDto>> Handle(GetReportDrafts request, CancellationToken cancellationToken)
    {
        await DivisionAuth.AssertAccessAsync(_context, request.RequestedBy, request.DivisionId, cancellationToken);

        var query = _context.ReportDrafts
            .Include(d => d.Division)
            .AsQueryable();

        if (request.DivisionId.HasValue)
            query = query.Where(d => d.DivisionId == request.DivisionId.Value);

        return await query
            .OrderByDescending(d => d.UpdatedAt ?? d.CreatedAt)
            .Select(d => new ReportDraftListItemDto
            {
                Id = d.Id,
                DivisionId = d.DivisionId,
                DivisionCode = d.Division.Code,
                Title = d.Title,
                Period = d.Period,
                DateFrom = d.DateFrom.HasValue ? d.DateFrom.Value.ToString("yyyy-MM-dd") : null,
                DateTo = d.DateTo.HasValue ? d.DateTo.Value.ToString("yyyy-MM-dd") : null,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                CreatedBy = d.CreatedBy,
            })
            .ToListAsync(cancellationToken);
    }
}
