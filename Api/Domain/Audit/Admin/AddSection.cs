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
public class AddSection : IRequest<int>
{
    public int DraftVersionId { get; set; }
    public AddSectionRequest Payload { get; set; } = null!;
    public string AddedBy { get; set; } = null!;
}

public class AddSectionHandler : IRequestHandler<AddSection, int>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public AddSectionHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<int> Handle(AddSection request, CancellationToken cancellationToken)
    {
        var version = await _context.AuditTemplateVersions
            .FirstOrDefaultAsync(v => v.Id == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Template version {request.DraftVersionId} not found.");

        if (version.Status != "Draft")
            throw new InvalidOperationException("Sections can only be added to Draft versions.");

        var maxOrder = await _context.AuditSections
            .Where(s => s.TemplateVersionId == request.DraftVersionId)
            .Select(s => (int?)s.DisplayOrder)
            .MaxAsync(cancellationToken) ?? 0;

        var now = DateTime.UtcNow;

        var section = new AuditSection
        {
            TemplateVersionId = request.DraftVersionId,
            Name = request.Payload.Name.Trim(),
            DisplayOrder = maxOrder + 1,
            IsRequired = false,
            Weight = request.Payload.Weight,
            IsOptional = request.Payload.IsOptional,
            OptionalGroupKey = request.Payload.OptionalGroupKey?.Trim(),
            CreatedAt = now,
            CreatedBy = request.AddedBy,
        };
        _context.AuditSections.Add(section);

        _context.TemplateChangeLogs.Add(new TemplateChangeLog
        {
            TemplateVersionId = request.DraftVersionId,
            ChangedBy = request.AddedBy,
            ChangedAt = now,
            ChangeType = "AddSection",
            ChangeNote = $"Added section \"{request.Payload.Name}\"",
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("AddSection", "AuditTemplateVersion", "Info",
            $"Section \"{request.Payload.Name}\" added to draft version {request.DraftVersionId} by {request.AddedBy}",
            relatedObject: section.Id.ToString());

        return section.Id;
    }
}
