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
    /// <summary>Per-version weight stored on AuditVersionQuestion.</summary>
    public decimal Weight { get; set; } = 1.0m;
    public bool IsLifeCritical { get; set; } = false;
    public bool AllowNA { get; set; } = true;
    public bool RequireCommentOnNC { get; set; } = true;
    public bool IsScoreable { get; set; } = true;
    public bool RequirePhotoOnNc { get; set; } = false;
    public bool AutoCreateCa { get; set; } = false;
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

        // Update question text + per-question flags on the master question record
        vq.Question.QuestionText = request.QuestionText.Trim();
        vq.Question.IsLifeCritical = request.IsLifeCritical;
        vq.Question.RequirePhotoOnNc = request.RequirePhotoOnNc;
        vq.Question.AutoCreateCa = request.AutoCreateCa;
        vq.Question.UpdatedAt = now;
        vq.Question.UpdatedBy = request.UpdatedBy;

        // Update per-version rules on the version question record
        vq.Weight = request.Weight;
        vq.AllowNA = request.AllowNA;
        vq.RequireCommentOnNC = request.RequireCommentOnNC;
        vq.IsScoreable = request.IsScoreable;
        vq.UpdatedAt = now;
        vq.UpdatedBy = request.UpdatedBy;

        var changes = new List<string>();
        if (oldText != vq.Question.QuestionText) changes.Add($"text: \"{oldText}\" → \"{vq.Question.QuestionText}\"");

        _context.TemplateChangeLogs.Add(new TemplateChangeLog
        {
            TemplateVersionId = request.DraftVersionId,
            ChangedBy = request.UpdatedBy,
            ChangedAt = now,
            ChangeType = "UpdateQuestion",
            ChangeNote = changes.Any()
                ? $"Question updated: {string.Join("; ", changes)}"
                : $"Question settings updated (weight={request.Weight}, lifeCritical={request.IsLifeCritical})",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("UpdateQuestion", "AuditQuestion", "Info",
            $"Question {vq.QuestionId} updated in draft version {request.DraftVersionId} by {request.UpdatedBy}",
            relatedObject: vq.QuestionId.ToString());

        return Unit.Value;
    }
}

// ── Batch weight update ───────────────────────────────────────────────────────

[AllowedAuthorizationRole(AuthorizationRole.Administrator, AuthorizationRole.TemplateAdmin)]
public class BatchUpdateQuestionWeights : IRequest<Unit>
{
    public int DraftVersionId { get; set; }
    public List<(int VersionQuestionId, decimal Weight)> Weights { get; set; } = new();
    public string UpdatedBy { get; set; } = null!;
}

public class BatchUpdateQuestionWeightsHandler : IRequestHandler<BatchUpdateQuestionWeights, Unit>
{
    private readonly AppDbContext _context;

    public BatchUpdateQuestionWeightsHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(BatchUpdateQuestionWeights request, CancellationToken cancellationToken)
    {
        var version = await _context.AuditTemplateVersions
            .FirstOrDefaultAsync(v => v.Id == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Template version {request.DraftVersionId} not found.");

        if (version.Status != "Draft")
            throw new InvalidOperationException("Questions can only be edited on Draft versions.");

        var ids = request.Weights.Select(w => w.VersionQuestionId).ToHashSet();
        var vqs = await _context.AuditVersionQuestions
            .Where(vq => vq.TemplateVersionId == request.DraftVersionId && ids.Contains(vq.Id))
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var weightMap = request.Weights.ToDictionary(w => w.VersionQuestionId, w => w.Weight);

        foreach (var vq in vqs)
        {
            if (weightMap.TryGetValue(vq.Id, out var w))
            {
                vq.Weight = w;
                vq.UpdatedAt = now;
                vq.UpdatedBy = request.UpdatedBy;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
