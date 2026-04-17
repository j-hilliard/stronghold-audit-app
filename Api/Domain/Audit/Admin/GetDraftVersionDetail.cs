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
public class GetDraftVersionDetail : IRequest<DraftVersionDetailDto?>
{
    public int DraftVersionId { get; set; }
}

public class GetDraftVersionDetailHandler : IRequestHandler<GetDraftVersionDetail, DraftVersionDetailDto?>
{
    private readonly AppDbContext _context;

    public GetDraftVersionDetailHandler(AppDbContext context) => _context = context;

    public async Task<DraftVersionDetailDto?> Handle(GetDraftVersionDetail request, CancellationToken cancellationToken)
    {
        var version = await _context.AuditTemplateVersions
            .Include(v => v.Template)
                .ThenInclude(t => t.Division)
            .FirstOrDefaultAsync(v => v.Id == request.DraftVersionId, cancellationToken);

        if (version == null)
            return null;

        var sections = await _context.AuditSections
            .Where(s => s.TemplateVersionId == request.DraftVersionId)
            .Include(s => s.ReportingCategory)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(cancellationToken);

        var sectionIds = sections.Select(s => s.Id).ToList();

        var versionQuestions = await _context.AuditVersionQuestions
            .Where(vq => vq.TemplateVersionId == request.DraftVersionId && sectionIds.Contains(vq.SectionId))
            .Include(vq => vq.Question)
                .ThenInclude(q => q.ResponseType)
            .OrderBy(vq => vq.SectionId)
            .ThenBy(vq => vq.DisplayOrder)
            .ToListAsync(cancellationToken);

        var questionsBySectionId = versionQuestions
            .GroupBy(vq => vq.SectionId)
            .ToDictionary(g => g.Key, g => g.ToList());

        return new DraftVersionDetailDto
        {
            Id = version.Id,
            VersionNumber = version.VersionNumber,
            DivisionCode = version.Template.Division.Code,
            DivisionName = version.Template.Division.Name,
            Sections = sections.Select(s => new DraftSectionDto
            {
                Id = s.Id,
                Name = s.Name,
                SectionCode = s.SectionCode,
                DisplayOrder = s.DisplayOrder,
                IsRequired = s.IsRequired,
                Weight = s.Weight,
                IsOptional = s.IsOptional,
                OptionalGroupKey = s.OptionalGroupKey,
                ReportingCategoryId = s.ReportingCategoryId,
                ReportingCategoryName = s.ReportingCategory?.Name,
                Questions = questionsBySectionId.TryGetValue(s.Id, out var qs)
                    ? qs.Select(vq => new DraftQuestionDto
                    {
                        VersionQuestionId = vq.Id,
                        QuestionId = vq.QuestionId,
                        QuestionText = vq.Question.QuestionText,
                        ShortLabel = vq.Question.ShortLabel,
                        HelpText = vq.Question.HelpText,
                        DisplayOrder = vq.DisplayOrder,
                        AllowNA = vq.AllowNA,
                        RequireCommentOnNC = vq.RequireCommentOnNC,
                        IsScoreable = vq.IsScoreable,
                        IsArchived = vq.Question.IsArchived,
                        IsLifeCritical = vq.Question.IsLifeCritical,
                        RequirePhotoOnNc = vq.Question.RequirePhotoOnNc,
                        AutoCreateCa = vq.Question.AutoCreateCa,
                        ResponseTypeId = vq.Question.ResponseTypeId,
                        ResponseTypeCode = vq.Question.ResponseType?.Code,
                        Weight = vq.Weight ?? vq.Question.Weight,
                    }).ToList()
                    : new List<DraftQuestionDto>()
            }).ToList()
        };
    }
}
