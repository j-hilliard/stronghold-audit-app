using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

public class GetDivisionJobPrefixes : IRequest<List<DivisionJobPrefixDto>>
{
    public int DivisionId { get; set; }
}

public class GetDivisionJobPrefixesHandler : IRequestHandler<GetDivisionJobPrefixes, List<DivisionJobPrefixDto>>
{
    private readonly AppDbContext _context;

    public GetDivisionJobPrefixesHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DivisionJobPrefixDto>> Handle(GetDivisionJobPrefixes request, CancellationToken cancellationToken)
    {
        return await _context.DivisionJobPrefixes
            .Where(p => p.DivisionId == request.DivisionId)
            .OrderBy(p => p.SortOrder)
            .Select(p => new DivisionJobPrefixDto
            {
                Id        = p.Id,
                Prefix    = p.Prefix,
                Label     = p.Label,
                IsDefault = p.IsDefault,
            })
            .ToListAsync(cancellationToken);
    }
}
