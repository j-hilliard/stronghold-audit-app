using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin)]
public class GetEmailRouting : IRequest<List<EmailRoutingRuleDto>> { }

public class GetEmailRoutingHandler : IRequestHandler<GetEmailRouting, List<EmailRoutingRuleDto>>
{
    private readonly AppDbContext _context;

    public GetEmailRoutingHandler(AppDbContext context) => _context = context;

    public async Task<List<EmailRoutingRuleDto>> Handle(GetEmailRouting request, CancellationToken cancellationToken)
    {
        var rules = await _context.EmailRoutingRules
            .Include(r => r.Division)
            .OrderBy(r => r.Division.Name)
            .ThenBy(r => r.EmailAddress)
            .ToListAsync(cancellationToken);

        return rules.Select(r => new EmailRoutingRuleDto
        {
            Id = r.Id,
            DivisionId = r.DivisionId,
            DivisionCode = r.Division.Code,
            DivisionName = r.Division.Name,
            EmailAddress = r.EmailAddress,
            IsActive = r.IsActive
        }).ToList();
    }
}
