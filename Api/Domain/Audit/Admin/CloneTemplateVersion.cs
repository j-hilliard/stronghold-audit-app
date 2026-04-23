using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin)]
public class CloneTemplateVersion : IRequest<int>
{
    public int VersionId { get; set; }
    public string ClonedBy { get; set; } = null!;
}

public class CloneTemplateVersionHandler : IRequestHandler<CloneTemplateVersion, int>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public CloneTemplateVersionHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<int> Handle(CloneTemplateVersion request, CancellationToken cancellationToken)
    {
        var source = await _context.AuditTemplateVersions
            .Include(v => v.Sections)
            .Include(v => v.VersionQuestions)
            .FirstOrDefaultAsync(v => v.Id == request.VersionId, cancellationToken)
            ?? throw new ArgumentException($"Template version {request.VersionId} not found.");

        // Only one Draft per template is allowed at a time
        var existingDraft = await _context.AuditTemplateVersions
            .AnyAsync(v => v.TemplateId == source.TemplateId && v.Status == "Draft", cancellationToken);

        if (existingDraft)
            throw new InvalidOperationException("A draft version already exists for this template. Publish or delete it before cloning.");

        var now = DateTime.UtcNow;

        var nextVersionNumber = await _context.AuditTemplateVersions
            .Where(v => v.TemplateId == source.TemplateId)
            .MaxAsync(v => v.VersionNumber, cancellationToken) + 1;

        var newVersion = new AuditTemplateVersion
        {
            TemplateId = source.TemplateId,
            VersionNumber = nextVersionNumber,
            Status = "Draft",
            ClonedFromVersionId = source.Id,
            CreatedAt = now,
            CreatedBy = request.ClonedBy
        };
        _context.AuditTemplateVersions.Add(newVersion);
        await _context.SaveChangesAsync(cancellationToken); // get newVersion.Id

        // Clone sections
        var sectionIdMap = new Dictionary<int, int>(); // old → new
        foreach (var section in source.Sections.OrderBy(s => s.DisplayOrder))
        {
            var newSection = new AuditSection
            {
                TemplateVersionId = newVersion.Id,
                Name = section.Name,
                DisplayOrder = section.DisplayOrder,
                CreatedAt = now,
                CreatedBy = request.ClonedBy
            };
            _context.AuditSections.Add(newSection);
            await _context.SaveChangesAsync(cancellationToken);
            sectionIdMap[section.Id] = newSection.Id;
        }

        // Clone version questions
        foreach (var vq in source.VersionQuestions.OrderBy(q => q.DisplayOrder))
        {
            _context.AuditVersionQuestions.Add(new AuditVersionQuestion
            {
                TemplateVersionId = newVersion.Id,
                SectionId = sectionIdMap[vq.SectionId],
                QuestionId = vq.QuestionId,
                DisplayOrder = vq.DisplayOrder,
                AllowNA = vq.AllowNA,
                RequireCommentOnNC = vq.RequireCommentOnNC,
                IsScoreable = vq.IsScoreable,
                CreatedAt = now,
                CreatedBy = request.ClonedBy
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("CloneTemplateVersion", "AuditTemplateVersion", "Info",
            $"Version {source.VersionNumber} cloned into new draft v{nextVersionNumber} by {request.ClonedBy}",
            relatedObject: newVersion.Id.ToString());

        return newVersion.Id;
    }
}
