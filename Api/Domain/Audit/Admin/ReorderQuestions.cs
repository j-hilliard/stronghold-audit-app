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
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator)]
public class ReorderQuestions : IRequest<Unit>
{
    public int DraftVersionId { get; set; }
    public ReorderQuestionsRequest Payload { get; set; } = null!;
    public string ReorderedBy { get; set; } = null!;
}

public class ReorderQuestionsHandler : IRequestHandler<ReorderQuestions, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public ReorderQuestionsHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Unit> Handle(ReorderQuestions request, CancellationToken cancellationToken)
    {
        var version = await _context.AuditTemplateVersions
            .FirstOrDefaultAsync(v => v.Id == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Template version {request.DraftVersionId} not found.");

        if (version.Status != "Draft")
            throw new InvalidOperationException("Questions can only be reordered in Draft versions.");

        var versionQuestions = await _context.AuditVersionQuestions
            .Where(vq => vq.TemplateVersionId == request.DraftVersionId)
            .ToListAsync(cancellationToken);

        var questionMap = versionQuestions.ToDictionary(vq => vq.Id);
        var now = DateTime.UtcNow;

        for (int i = 0; i < request.Payload.VersionQuestionIds.Count; i++)
        {
            var id = request.Payload.VersionQuestionIds[i];
            if (!questionMap.TryGetValue(id, out var vq))
                throw new ArgumentException($"Version question {id} not found in draft version {request.DraftVersionId}.");

            vq.DisplayOrder = i + 1;
            vq.UpdatedAt = now;
            vq.UpdatedBy = request.ReorderedBy;
        }

        _context.TemplateChangeLogs.Add(new TemplateChangeLog
        {
            TemplateVersionId = request.DraftVersionId,
            ChangedBy = request.ReorderedBy,
            ChangedAt = now,
            ChangeType = "Reorder",
            ChangeNote = $"Reordered {request.Payload.VersionQuestionIds.Count} questions",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("ReorderQuestions", "AuditTemplateVersion", "Info",
            $"Questions reordered in draft version {request.DraftVersionId} by {request.ReorderedBy}",
            relatedObject: request.DraftVersionId.ToString());

        return Unit.Value;
    }
}
