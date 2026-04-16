using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

public class ReviewGroupMemberDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
}

[AllowedAuthorizationRole(AuthorizationRole.Administrator, AuthorizationRole.TemplateAdmin)]
public class GetReviewGroup : IRequest<List<ReviewGroupMemberDto>> { }

public class GetReviewGroupHandler : IRequestHandler<GetReviewGroup, List<ReviewGroupMemberDto>>
{
    private readonly AppDbContext _context;

    public GetReviewGroupHandler(AppDbContext context) => _context = context;

    public async Task<List<ReviewGroupMemberDto>> Handle(GetReviewGroup request, CancellationToken cancellationToken)
    {
        return await _context.ReviewGroupMembers
            .OrderBy(m => m.Name)
            .Select(m => new ReviewGroupMemberDto
            {
                Id = m.Id,
                Name = m.Name,
                Email = m.Email,
                IsActive = m.IsActive,
            })
            .ToListAsync(cancellationToken);
    }
}
