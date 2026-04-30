using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(AuthorizationRole.Administrator, AuthorizationRole.TemplateAdmin, AuthorizationRole.AuditAdmin)]
public class CopySection : IRequest<int>
{
    public int DraftVersionId { get; set; }
    public CopySectionRequest Payload { get; set; } = null!;
    public string CopiedBy { get; set; } = null!;
}

public class CopySectionHandler : IRequestHandler<CopySection, int>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public CopySectionHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<int> Handle(CopySection request, CancellationToken cancellationToken)
    {
        // Validate target draft
        var targetVersion = await _context.AuditTemplateVersions
            .FirstOrDefaultAsync(v => v.Id == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Template version {request.DraftVersionId} not found.");

        if (targetVersion.Status != "Draft")
            throw new InvalidOperationException("Sections can only be added to Draft versions.");

        // Load source section with questions
        var source = await _context.AuditSections
            .Include(s => s.VersionQuestions.Where(vq => !vq.IsDeleted))
            .FirstOrDefaultAsync(s => s.Id == request.Payload.SourceSectionId && !s.IsDeleted, cancellationToken)
            ?? throw new ArgumentException($"Source section {request.Payload.SourceSectionId} not found.");

        var now = DateTime.UtcNow;

        // Determine next display order in target draft
        var maxOrder = await _context.AuditSections
            .Where(s => s.TemplateVersionId == request.DraftVersionId && !s.IsDeleted)
            .Select(s => (int?)s.DisplayOrder)
            .MaxAsync(cancellationToken) ?? 0;

        // Create the new section in the draft
        var newSection = new AuditSection
        {
            TemplateVersionId = request.DraftVersionId,
            Name = source.Name,
            SectionCode = source.SectionCode,
            ReportingCategoryId = source.ReportingCategoryId,
            DisplayOrder = maxOrder + 1,
            IsRequired = source.IsRequired,
            CreatedAt = now,
            CreatedBy = request.CopiedBy,
        };
        _context.AuditSections.Add(newSection);
        await _context.SaveChangesAsync(cancellationToken);

        // Copy each question (link to same QuestionId — questions are shared across versions)
        for (int i = 0; i < source.VersionQuestions.Count; i++)
        {
            var srcVq = source.VersionQuestions.OrderBy(vq => vq.DisplayOrder).ElementAt(i);
            _context.AuditVersionQuestions.Add(new AuditVersionQuestion
            {
                TemplateVersionId = request.DraftVersionId,
                SectionId = newSection.Id,
                QuestionId = srcVq.QuestionId,
                DisplayOrder = i + 1,
                AllowNA = srcVq.AllowNA,
                RequireCommentOnNC = srcVq.RequireCommentOnNC,
                IsScoreable = srcVq.IsScoreable,
                Weight = srcVq.Weight,
                CreatedAt = now,
                CreatedBy = request.CopiedBy,
            });
        }

        _context.TemplateChangeLogs.Add(new TemplateChangeLog
        {
            TemplateVersionId = request.DraftVersionId,
            ChangedBy = request.CopiedBy,
            ChangedAt = now,
            ChangeType = "CopySection",
            ChangeNote = $"Copied section \"{source.Name}\" ({source.VersionQuestions.Count} questions) from section #{request.Payload.SourceSectionId}",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("CopySection", "AuditTemplateVersion", "Info",
            $"Section \"{source.Name}\" copied into draft version {request.DraftVersionId} by {request.CopiedBy}",
            relatedObject: newSection.Id.ToString());

        return newSection.Id;
    }
}
