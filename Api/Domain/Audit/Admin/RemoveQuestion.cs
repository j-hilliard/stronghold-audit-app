using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(AuthorizationRole.Administrator)]
public class RemoveQuestion : IRequest<Unit>
{
    public int DraftVersionId { get; set; }
    public int VersionQuestionId { get; set; }
    public string RemovedBy { get; set; } = null!;
}

public class RemoveQuestionHandler : IRequestHandler<RemoveQuestion, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public RemoveQuestionHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Unit> Handle(RemoveQuestion request, CancellationToken cancellationToken)
    {
        var version = await _context.AuditTemplateVersions
            .FirstOrDefaultAsync(v => v.Id == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Template version {request.DraftVersionId} not found.");

        if (version.Status != "Draft")
            throw new InvalidOperationException("Questions can only be removed from Draft versions.");

        var vq = await _context.AuditVersionQuestions
            .Include(q => q.Question)
            .Include(q => q.Section)
            .FirstOrDefaultAsync(q => q.Id == request.VersionQuestionId && q.TemplateVersionId == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Version question {request.VersionQuestionId} not found in this draft.");

        var questionText = vq.Question.QuestionText;
        var sectionName = vq.Section.Name;
        var questionId = vq.QuestionId;

        // Remove the junction record (never deletes the question master)
        _context.AuditVersionQuestions.Remove(vq);

        // Archive the question if it is not used in any other active or draft version
        var usedElsewhere = await _context.AuditVersionQuestions
            .AnyAsync(q => q.QuestionId == questionId && q.Id != request.VersionQuestionId, cancellationToken);

        if (!usedElsewhere)
        {
            var question = await _context.AuditQuestions.FindAsync(new object[] { questionId }, cancellationToken);
            if (question != null && !question.IsArchived)
            {
                question.IsArchived = true;
                question.ArchivedAt = DateTime.UtcNow;
                question.ArchivedBy = request.RemovedBy;
            }
        }

        var now = DateTime.UtcNow;
        _context.TemplateChangeLogs.Add(new TemplateChangeLog
        {
            TemplateVersionId = request.DraftVersionId,
            ChangedBy = request.RemovedBy,
            ChangedAt = now,
            ChangeType = "RemoveQuestion",
            ChangeNote = $"Removed: \"{questionText}\" from section \"{sectionName}\"",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("RemoveQuestion", "AuditTemplateVersion", "Info",
            $"Question removed from draft version {request.DraftVersionId} by {request.RemovedBy}. Question archived: {!usedElsewhere}",
            relatedObject: request.DraftVersionId.ToString());

        return Unit.Value;
    }
}
