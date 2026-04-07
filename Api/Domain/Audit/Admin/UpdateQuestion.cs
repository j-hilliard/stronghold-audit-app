using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(AuthorizationRole.Administrator, AuthorizationRole.TemplateAdmin)]
public class UpdateQuestion : IRequest<Unit>
{
    public int DraftVersionId { get; set; }
    public int VersionQuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public string UpdatedBy { get; set; } = null!;
}

public class UpdateQuestionHandler : IRequestHandler<UpdateQuestion, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public UpdateQuestionHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Unit> Handle(UpdateQuestion request, CancellationToken cancellationToken)
    {
        var version = await _context.AuditTemplateVersions
            .FirstOrDefaultAsync(v => v.Id == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Template version {request.DraftVersionId} not found.");

        if (version.Status != "Draft")
            throw new InvalidOperationException("Questions can only be edited on Draft versions.");

        var vq = await _context.AuditVersionQuestions
            .Include(vq => vq.Question)
            .FirstOrDefaultAsync(vq => vq.Id == request.VersionQuestionId && vq.TemplateVersionId == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Question {request.VersionQuestionId} not found in this draft.");

        var oldText = vq.Question.QuestionText;
        var now = DateTime.UtcNow;

        vq.Question.QuestionText = request.QuestionText.Trim();
        vq.Question.UpdatedAt = now;
        vq.Question.UpdatedBy = request.UpdatedBy;

        _context.TemplateChangeLogs.Add(new TemplateChangeLog
        {
            TemplateVersionId = request.DraftVersionId,
            ChangedBy = request.UpdatedBy,
            ChangedAt = now,
            ChangeType = "UpdateQuestion",
            ChangeNote = $"Question text updated: \"{oldText}\" → \"{vq.Question.QuestionText}\"",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("UpdateQuestion", "AuditQuestion", "Info",
            $"Question {vq.QuestionId} text updated in draft version {request.DraftVersionId} by {request.UpdatedBy}",
            relatedObject: vq.QuestionId.ToString());

        return Unit.Value;
    }
}
