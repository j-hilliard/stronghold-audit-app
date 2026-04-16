using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class AssignCorrectiveAction : IRequest<int>
{
    public AssignCorrectiveActionRequest Payload { get; set; } = null!;
    public string AssignedBy { get; set; } = null!;
}

public class AssignCorrectiveActionHandler : IRequestHandler<AssignCorrectiveAction, int>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public AssignCorrectiveActionHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<int> Handle(AssignCorrectiveAction request, CancellationToken cancellationToken)
    {
        var finding = await _context.AuditFindings
            .FirstOrDefaultAsync(f => f.Id == request.Payload.FindingId, cancellationToken)
            ?? throw new ArgumentException($"Finding {request.Payload.FindingId} not found.");

        // Default to 14 days from today when no due date provided (per workflow requirement)
        DateOnly? dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(14));
        if (!string.IsNullOrWhiteSpace(request.Payload.DueDate) &&
            DateOnly.TryParse(request.Payload.DueDate, out var parsed))
            dueDate = parsed;

        var ca = new CorrectiveAction
        {
            FindingId = finding.Id,
            AuditId = finding.AuditId,
            Description = request.Payload.Description,
            AssignedTo = request.Payload.AssignedTo,
            DueDate = dueDate,
            Status = "Open",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.AssignedBy,
        };

        _context.CorrectiveActions.Add(ca);
        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("AssignCA", "CorrectiveAction", "Info",
            $"CA assigned to '{ca.AssignedTo}' for finding {finding.Id} on audit {finding.AuditId} by {request.AssignedBy}.",
            relatedObject: ca.Id.ToString());

        return ca.Id;
    }
}
