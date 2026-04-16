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
public class UpdateSection : IRequest<Unit>
{
    public int DraftVersionId { get; set; }
    public int SectionId { get; set; }
    public UpdateSectionRequest Payload { get; set; } = null!;
    public string UpdatedBy { get; set; } = null!;
}

public class UpdateSectionHandler : IRequestHandler<UpdateSection, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public UpdateSectionHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Unit> Handle(UpdateSection request, CancellationToken cancellationToken)
    {
        var version = await _context.AuditTemplateVersions
            .FirstOrDefaultAsync(v => v.Id == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Template version {request.DraftVersionId} not found.");

        if (version.Status != "Draft")
            throw new InvalidOperationException("Sections can only be edited on Draft versions.");

        var section = await _context.AuditSections
            .FirstOrDefaultAsync(s => s.Id == request.SectionId && s.TemplateVersionId == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Section {request.SectionId} not found in this draft.");

        var oldName = section.Name;
        var now = DateTime.UtcNow;

        section.Name = request.Payload.Name.Trim();
        section.IsRequired = request.Payload.IsRequired;
        section.Weight = request.Payload.Weight;
        section.IsOptional = request.Payload.IsOptional;
        section.OptionalGroupKey = request.Payload.OptionalGroupKey?.Trim();
        section.ReportingCategoryId = request.Payload.ReportingCategoryId;
        section.UpdatedAt = now;
        section.UpdatedBy = request.UpdatedBy;

        _context.TemplateChangeLogs.Add(new TemplateChangeLog
        {
            TemplateVersionId = request.DraftVersionId,
            ChangedBy = request.UpdatedBy,
            ChangedAt = now,
            ChangeType = "UpdateSection",
            ChangeNote = oldName != section.Name
                ? $"Renamed section \"{oldName}\" → \"{section.Name}\""
                : $"Updated section \"{section.Name}\" properties",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("UpdateSection", "AuditSection", "Info",
            $"Section {request.SectionId} updated in draft version {request.DraftVersionId} by {request.UpdatedBy}",
            relatedObject: request.SectionId.ToString());

        return Unit.Value;
    }
}
