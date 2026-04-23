using MediatR;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Reports;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager,
    AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin)]
public class DeleteScheduledReport : IRequest
{
    public int Id { get; set; }
}

public class DeleteScheduledReportHandler : IRequestHandler<DeleteScheduledReport>
{
    private readonly AppDbContext _db;

    public DeleteScheduledReportHandler(AppDbContext db) => _db = db;

    public async Task Handle(DeleteScheduledReport request, CancellationToken ct)
    {
        var entity = await _db.ScheduledReports.FindAsync(new object[] { request.Id }, ct)
                     ?? throw new KeyNotFoundException($"Scheduled report {request.Id} not found.");

        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }
}
