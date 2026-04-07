using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(AuthorizationRole.Administrator, AuthorizationRole.TemplateAdmin)]
public class ReorderSections : IRequest<Unit>
{
    public int DraftVersionId { get; set; }
    public ReorderSectionsRequest Payload { get; set; } = null!;
    public string ReorderedBy { get; set; } = null!;
}

public class ReorderSectionsHandler : IRequestHandler<ReorderSections, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public ReorderSectionsHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Unit> Handle(ReorderSections request, CancellationToken cancellationToken)
    {
        var version = await _context.AuditTemplateVersions
            .FirstOrDefaultAsync(v => v.Id == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Template version {request.DraftVersionId} not found.");

        if (version.Status != "Draft")
            throw new InvalidOperationException("Sections can only be reordered in Draft versions.");

        var sections = await _context.AuditSections
            .Where(s => s.TemplateVersionId == request.DraftVersionId)
            .ToListAsync(cancellationToken);

        var sectionMap = sections.ToDictionary(s => s.Id);
        var now = DateTime.UtcNow;

        for (int i = 0; i < request.Payload.SectionIds.Count; i++)
        {
            var id = request.Payload.SectionIds[i];
            if (!sectionMap.TryGetValue(id, out var section))
                throw new ArgumentException($"Section {id} not found in draft version {request.DraftVersionId}.");

            section.DisplayOrder = i + 1;
            section.UpdatedAt = now;
            section.UpdatedBy = request.ReorderedBy;
        }

        _context.TemplateChangeLogs.Add(new TemplateChangeLog
        {
            TemplateVersionId = request.DraftVersionId,
            ChangedBy = request.ReorderedBy,
            ChangedAt = now,
            ChangeType = "ReorderSections",
            ChangeNote = $"Reordered {request.Payload.SectionIds.Count} sections",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("ReorderSections", "AuditTemplateVersion", "Info",
            $"Sections reordered in draft version {request.DraftVersionId} by {request.ReorderedBy}",
            relatedObject: request.DraftVersionId.ToString());

        return Unit.Value;
    }
}
