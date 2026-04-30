using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin)]
public class PublishTemplateVersion : IRequest<Unit>
{
    public int DraftVersionId { get; set; }
    public string PublishedBy { get; set; } = null!;
}

public class PublishTemplateVersionHandler : IRequestHandler<PublishTemplateVersion, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public PublishTemplateVersionHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Unit> Handle(PublishTemplateVersion request, CancellationToken cancellationToken)
    {
        var draft = await _context.AuditTemplateVersions
            .FirstOrDefaultAsync(v => v.Id == request.DraftVersionId, cancellationToken)
            ?? throw new ArgumentException($"Template version {request.DraftVersionId} not found.");

        if (draft.Status != "Draft")
            throw new InvalidOperationException($"Version {request.DraftVersionId} is not a Draft.");

        var now = DateTime.UtcNow;

        // Supersede any currently Active version for this template
        var currentActive = await _context.AuditTemplateVersions
            .Where(v => v.TemplateId == draft.TemplateId && v.Status == "Active")
            .ToListAsync(cancellationToken);

        foreach (var active in currentActive)
        {
            active.Status = "Superseded";
            active.UpdatedAt = now;
            active.UpdatedBy = request.PublishedBy;
        }

        draft.Status = "Active";
        draft.PublishedAt = now;
        draft.PublishedBy = request.PublishedBy;
        draft.UpdatedAt = now;
        draft.UpdatedBy = request.PublishedBy;

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("PublishTemplateVersion", "AuditTemplateVersion", "Info",
            $"Draft version {request.DraftVersionId} published as Active by {request.PublishedBy}. {currentActive.Count} version(s) superseded.",
            relatedObject: request.DraftVersionId.ToString());

        return Unit.Value;
    }
}
