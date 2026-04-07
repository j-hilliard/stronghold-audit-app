using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;
using AuditEntity = Stronghold.AppDashboard.Data.Models.Audit.Audit;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(AuthorizationRole.AuthenticatedUser)]
public class CreateAudit : IRequest<int>
{
    public int DivisionId { get; set; }
    public string CreatedBy { get; set; } = null!;
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

        return audit.Id;
    }
}
