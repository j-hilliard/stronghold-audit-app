using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class UpdateCorrectiveAction : IRequest
{
    public int     CorrectiveActionId { get; set; }
    public string? Description       { get; set; }
    public string? AssignedTo        { get; set; }
    public int?    AssignedToUserId  { get; set; }
    public string? DueDate           { get; set; }  // ISO "YYYY-MM-DD" or null to clear
    public string  UpdatedBy         { get; set; } = null!;
}

public class UpdateCorrectiveActionHandler : IRequestHandler<UpdateCorrectiveAction>
{
    private readonly AppDbContext _context;

    public UpdateCorrectiveActionHandler(AppDbContext context) => _context = context;

    public async Task Handle(UpdateCorrectiveAction request, CancellationToken cancellationToken)
    {
        var ca = await _context.CorrectiveActions
            .FirstOrDefaultAsync(c => c.Id == request.CorrectiveActionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Corrective action #{request.CorrectiveActionId} not found.");

        if (ca.Status is "Closed" or "Voided")
            throw new InvalidOperationException($"Cannot edit a corrective action with status '{ca.Status}'.");

        if (request.Description is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Description))
                throw new ArgumentException("Description cannot be empty.");
            ca.Description = request.Description.Trim();
        }

        if (request.AssignedTo is not null)
        {
            ca.AssignedTo       = string.IsNullOrWhiteSpace(request.AssignedTo) ? null : request.AssignedTo.Trim();
            ca.AssignedToUserId = request.AssignedToUserId;
        }

        if (request.DueDate is not null)
        {
            ca.DueDate = string.IsNullOrEmpty(request.DueDate)
                ? null
                : DateOnly.Parse(request.DueDate);
        }

        ca.UpdatedAt = DateTime.UtcNow;
        ca.UpdatedBy = request.UpdatedBy;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
