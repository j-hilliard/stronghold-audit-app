using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator)]
public class SaveDivisionJobPrefixes : IRequest
{
    public int DivisionId { get; set; }
    public List<DivisionJobPrefixUpsertDto> Prefixes { get; set; } = new();
}

public class SaveDivisionJobPrefixesHandler : IRequestHandler<SaveDivisionJobPrefixes>
{
    private readonly AppDbContext _context;

    public SaveDivisionJobPrefixesHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SaveDivisionJobPrefixes request, CancellationToken cancellationToken)
    {
        var existing = await _context.DivisionJobPrefixes
            .Where(p => p.DivisionId == request.DivisionId)
            .ToListAsync(cancellationToken);

        _context.DivisionJobPrefixes.RemoveRange(existing);

        for (var i = 0; i < request.Prefixes.Count; i++)
        {
            var dto = request.Prefixes[i];
            _context.DivisionJobPrefixes.Add(new DivisionJobPrefix
            {
                DivisionId = request.DivisionId,
                Prefix     = dto.Prefix.Trim(),
                Label      = dto.Label.Trim(),
                IsDefault  = dto.IsDefault,
                SortOrder  = i,
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
