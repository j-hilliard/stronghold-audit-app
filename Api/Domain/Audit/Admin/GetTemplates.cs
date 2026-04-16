using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class GetTemplates : IRequest<List<TemplateVersionListItemDto>> { }

public class GetTemplatesHandler : IRequestHandler<GetTemplates, List<TemplateVersionListItemDto>>
{
    private readonly AppDbContext _context;

    public GetTemplatesHandler(AppDbContext context) => _context = context;

    public async Task<List<TemplateVersionListItemDto>> Handle(GetTemplates request, CancellationToken cancellationToken)
    {
        var versions = await _context.AuditTemplateVersions
            .Include(v => v.Template)
                .ThenInclude(t => t.Division)
            .Include(v => v.VersionQuestions)
            .OrderBy(v => v.Template.Division.Name)
            .ThenByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken);

        return versions.Select(v => new TemplateVersionListItemDto
        {
            Id = v.Id,
            TemplateId = v.TemplateId,
            DivisionCode = v.Template.Division.Code,
            DivisionName = v.Template.Division.Name,
            VersionNumber = v.VersionNumber,
            Status = v.Status,
            PublishedAt = v.PublishedAt,
            PublishedBy = v.PublishedBy,
            ClonedFromVersionId = v.ClonedFromVersionId,
            QuestionCount = v.VersionQuestions.Count
        }).ToList();
    }
}
