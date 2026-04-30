using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(AuthorizationRole.Administrator, AuthorizationRole.TemplateAdmin, AuthorizationRole.AuditAdmin)]
public class GetSectionLibrary : IRequest<List<SectionLibraryItemDto>> { }

public class GetSectionLibraryHandler : IRequestHandler<GetSectionLibrary, List<SectionLibraryItemDto>>
{
    private readonly AppDbContext _context;

    public GetSectionLibraryHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SectionLibraryItemDto>> Handle(GetSectionLibrary request, CancellationToken cancellationToken)
    {
        var sections = await _context.AuditSections
            .Where(s => !s.IsDeleted && s.TemplateVersion.Status == "Active")
            .Include(s => s.TemplateVersion)
                .ThenInclude(v => v.Template)
                    .ThenInclude(t => t.Division)
            .OrderBy(s => s.TemplateVersion.Template.Division.Name)
            .ThenBy(s => s.DisplayOrder)
            .Select(s => new SectionLibraryItemDto
            {
                SectionId = s.Id,
                Name = s.Name,
                SectionCode = s.SectionCode,
                DivisionCode = s.TemplateVersion.Template.Division.Code,
                DivisionName = s.TemplateVersion.Template.Division.Name,
                QuestionCount = s.VersionQuestions.Count(vq => !vq.IsDeleted),
            })
            .ToListAsync(cancellationToken);

        return sections;
    }
}
