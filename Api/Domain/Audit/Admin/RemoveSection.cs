using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(AuthorizationRole.Administrator, AuthorizationRole.TemplateAdmin)]
public class RemoveSection : IRequest<Unit>
{
    public int DraftVersionId { get; set; }
    public int SectionId { get; set; }
    public string RemovedBy { get; set; } = null!;
}

public class RemoveSectionHandler : IRequestHandler<RemoveSection, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public RemoveSectionHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Unit> Handle(RemoveSection request, CancellationToken cancellationToken)
    {
        var version = await _context.AuditTemplateVersions
            .FirstOrDefaultAsync(v => v.Id == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Template version {request.DraftVersionId} not found.");

        if (version.Status != "Draft")
            throw new InvalidOperationException("Sections can only be removed from Draft versions.");

        var section = await _context.AuditSections
            .Include(s => s.VersionQuestions)
                .ThenInclude(vq => vq.Question)
            .FirstOrDefaultAsync(s => s.Id == request.SectionId && s.TemplateVersionId == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Section {request.SectionId} not found in this draft.");

        var sectionName = section.Name;
        var now = DateTime.UtcNow;

        // Archive questions that are not used in any other version
        foreach (var vq in section.VersionQuestions.ToList())
        {
            var usedElsewhere = await _context.AuditVersionQuestions
                .AnyAsync(q => q.QuestionId == vq.QuestionId && q.Id != vq.Id, cancellationToken);

            if (!usedElsewhere && !vq.Question.IsArchived)
            {
                vq.Question.IsArchived = true;
                vq.Question.ArchivedAt = now;
                vq.Question.ArchivedBy = request.RemovedBy;
            }
        }

        // Remove all version questions in this section, then the section itself
        _context.AuditVersionQuestions.RemoveRange(section.VersionQuestions);
        _context.AuditSections.Remove(section);

        _context.TemplateChangeLogs.Add(new TemplateChangeLog
        {
            TemplateVersionId = request.DraftVersionId,
            ChangedBy = request.RemovedBy,
            ChangedAt = now,
            ChangeType = "RemoveSection",
            ChangeNote = $"Removed section \"{sectionName}\" and its {section.VersionQuestions.Count} question(s)",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("RemoveSection", "AuditTemplateVersion", "Info",
            $"Section \"{sectionName}\" removed from draft version {request.DraftVersionId} by {request.RemovedBy}",
            relatedObject: request.DraftVersionId.ToString());

        return Unit.Value;
    }
}
