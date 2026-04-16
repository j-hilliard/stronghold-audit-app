using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;
using AuditEntity = Stronghold.AppDashboard.Data.Models.Audit.Audit;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class CreateAudit : IRequest<int>
{
    public int DivisionId { get; set; }
    public string CreatedBy { get; set; } = null!;

    /// <summary>
    /// Optional section group keys to enable for this audit.
    /// Empty = no optional sections enabled. Immutable after creation.
    /// </summary>
    public List<string> EnabledOptionalGroupKeys { get; set; } = new();
}

public class CreateAuditHandler : IRequestHandler<CreateAudit, int>
{
    private readonly AppDbContext _context;

    public CreateAuditHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateAudit request, CancellationToken cancellationToken)
    {
        var division = await _context.Divisions
            .FirstOrDefaultAsync(d => d.Id == request.DivisionId, cancellationToken)
            ?? throw new ArgumentException($"Division {request.DivisionId} not found.");

        // Lock to the current active template version at creation time — never changes
        var version = await _context.AuditTemplateVersions
            .Include(v => v.Template)
            .FirstOrDefaultAsync(v =>
                v.Template.DivisionId == request.DivisionId &&
                v.Status == "Active",
                cancellationToken)
            ?? throw new ArgumentException($"No active template found for division {division.Code}.");

        var now = DateTime.UtcNow;
        var audit = new AuditEntity
        {
            DivisionId = request.DivisionId,
            TemplateVersionId = version.Id,
            AuditType = division.AuditType,
            Status = "Draft",
            CreatedAt = now,
            CreatedBy = request.CreatedBy
        };

        _context.Audits.Add(audit);
        await _context.SaveChangesAsync(cancellationToken);

        // Persist enabled optional section groups (immutable snapshot)
        foreach (var key in request.EnabledOptionalGroupKeys.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _context.AuditEnabledSections.Add(new AuditEnabledSection
            {
                AuditId = audit.Id,
                OptionalGroupKey = key,
            });
        }

        if (request.EnabledOptionalGroupKeys.Any())
            await _context.SaveChangesAsync(cancellationToken);

        return audit.Id;
    }
}
