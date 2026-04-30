using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin)]
public class AddQuestion : IRequest<int>
{
    public int DraftVersionId { get; set; }
    public AddQuestionRequest Payload { get; set; } = null!;
    public string AddedBy { get; set; } = null!;
}

public class AddQuestionHandler : IRequestHandler<AddQuestion, int>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public AddQuestionHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<int> Handle(AddQuestion request, CancellationToken cancellationToken)
    {
        var version = await _context.AuditTemplateVersions
            .FirstOrDefaultAsync(v => v.Id == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Template version {request.DraftVersionId} not found.");

        if (version.Status != "Draft")
            throw new InvalidOperationException("Questions can only be added to Draft versions.");

        var section = await _context.AuditSections
            .FirstOrDefaultAsync(s => s.Id == request.Payload.SectionId && s.TemplateVersionId == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Section {request.Payload.SectionId} not found in this version.");

        var now = DateTime.UtcNow;

        // Create new question master record
        var question = new AuditQuestion
        {
            QuestionText = request.Payload.QuestionText,
            IsArchived = false,
            CreatedAt = now,
            CreatedBy = request.AddedBy
        };
        _context.AuditQuestions.Add(question);
        await _context.SaveChangesAsync(cancellationToken);

        // Get next display order in this section
        var maxOrder = await _context.AuditVersionQuestions
            .Where(vq => vq.TemplateVersionId == request.DraftVersionId && vq.SectionId == section.Id)
            .Select(vq => (int?)vq.DisplayOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var versionQuestion = new AuditVersionQuestion
        {
            TemplateVersionId = request.DraftVersionId,
            SectionId = section.Id,
            QuestionId = question.Id,
            DisplayOrder = maxOrder + 1,
            AllowNA = request.Payload.AllowNA,
            RequireCommentOnNC = request.Payload.RequireCommentOnNC,
            IsScoreable = request.Payload.IsScoreable,
            CreatedAt = now,
            CreatedBy = request.AddedBy
        };
        _context.AuditVersionQuestions.Add(versionQuestion);

        _context.TemplateChangeLogs.Add(new TemplateChangeLog
        {
            TemplateVersionId = request.DraftVersionId,
            ChangedBy = request.AddedBy,
            ChangedAt = now,
            ChangeType = "AddQuestion",
            ChangeNote = $"Added: \"{request.Payload.QuestionText}\" to section \"{section.Name}\"",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("AddQuestion", "AuditTemplateVersion", "Info",
            $"Question added to draft version {request.DraftVersionId} by {request.AddedBy}",
            relatedObject: versionQuestion.Id.ToString());

        return versionQuestion.Id;
    }
}
