using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Divisions;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.ReadOnlyViewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.Auditor, AuthorizationRole.AuditAdmin, AuthorizationRole.Executive, AuthorizationRole.NormalUser)]
public class GetDivisions : IRequest<List<DivisionDto>> { }

public class GetDivisionsHandler : IRequestHandler<GetDivisions, List<DivisionDto>>
{
    private readonly AppDbContext _context;

    public GetDivisionsHandler(AppDbContext context) => _context = context;

    public async Task<List<DivisionDto>> Handle(GetDivisions request, CancellationToken cancellationToken)
    {
        var divisions = await _context.Divisions
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);

        return divisions.Select(d => new DivisionDto
        {
            Id = d.Id,
            Code = d.Code,
            Name = d.Name,
            AuditType = d.AuditType
        }).ToList();
    }
}
