using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Templates;

[AllowedAuthorizationRole(AuthorizationRole.AuthenticatedUser)]
public class GetActiveTemplate : IRequest<TemplateDto?>
{
    public int DivisionId { get; set; }
}

public class GetActiveTemplateHandler : IRequestHandler<GetActiveTemplate, TemplateDto?>
{
    private readonly AppDbContext _context;

    public GetActiveTemplateHandler(AppDbContext context) => _context = context;

    public async Task<TemplateDto?> Handle(GetActiveTemplate request, CancellationToken cancellationToken)
    {
        var division = await _context.Divisions
            .FirstOrDefaultAsync(d => d.Id == request.DivisionId, cancellationToken);

        if (division == null) return null;

        var version = await _context.AuditTemplateVersions
            .Include(v => v.Template)
            .FirstOrDefaultAsync(v =>
                v.Template.DivisionId == request.DivisionId &&
                v.Status == "Active",
                cancellationToken);

        if (version == null) return null;

        var sections = await _context.AuditSections
            .Where(s => s.TemplateVersionId == version.Id)
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(cancellationToken);

        var versionQuestions = await _context.AuditVersionQuestions
            .Include(vq => vq.Question)
            .Where(vq => vq.TemplateVersionId == version.Id)
            .OrderBy(vq => vq.DisplayOrder)
            .ToListAsync(cancellationToken);

        var logicRules = await _context.QuestionLogicRules
            .Where(r => r.TemplateVersionId == version.Id && r.IsActive)
            .ToListAsync(cancellationToken);

        var sectionDtos = sections.Select(s => new TemplateSectionDto
        {
            Id = s.Id,
            Name = s.Name,
            DisplayOrder = s.DisplayOrder,
            IsOptional = s.IsOptional,
            OptionalGroupKey = s.OptionalGroupKey,
            Questions = versionQuestions
                .Where(vq => vq.SectionId == s.Id)
                .Select(vq => new TemplateQuestionDto
                {
                    VersionQuestionId = vq.Id,
                    QuestionId = vq.QuestionId,
                    QuestionText = vq.Question.QuestionText,
                    DisplayOrder = vq.DisplayOrder,
                    AllowNA = vq.AllowNA,
                    RequireCommentOnNC = vq.RequireCommentOnNC,
                    IsScoreable = vq.IsScoreable,
                    IsLifeCritical = vq.Question.IsLifeCritical
                }).ToList()
        }).ToList();

        return new TemplateDto
        {
            VersionId = version.Id,
            VersionNumber = version.VersionNumber,
            DivisionCode = division.Code,
            DivisionName = division.Name,
            AuditType = division.AuditType,
            Sections = sectionDtos,
            LogicRules = logicRules.Select(r => new LogicRuleDto
            {
                Id = r.Id,
                TriggerVersionQuestionId = r.TriggerVersionQuestionId,
                TriggerResponse = r.TriggerResponse,
                Action = r.Action,
                TargetSectionId = r.TargetSectionId,
            }).ToList(),
        };
    }
}
