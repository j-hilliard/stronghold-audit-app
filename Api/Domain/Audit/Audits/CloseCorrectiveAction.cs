using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class CloseCorrectiveAction : IRequest<Unit>
{
    public int CorrectiveActionId { get; set; }
    public CloseCorrectiveActionRequest Payload { get; set; } = null!;
    public string ClosedBy { get; set; } = null!;
}

public class CloseCorrectiveActionHandler : IRequestHandler<CloseCorrectiveAction, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public CloseCorrectiveActionHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Unit> Handle(CloseCorrectiveAction request, CancellationToken cancellationToken)
    {
        var ca = await _context.CorrectiveActions
            .Include(c => c.Audit!).ThenInclude(a => a.Division)
            .FirstOrDefaultAsync(c => c.Id == request.CorrectiveActionId, cancellationToken)
            ?? throw new ArgumentException($"Corrective action {request.CorrectiveActionId} not found.");

        if (ca.Status == "Closed")
            throw new InvalidOperationException("Corrective action is already closed.");

        // Enforce per-division closure photo requirement
        if (ca.Audit?.Division?.RequireClosurePhoto == true)
        {
            var hasPhoto = await _context.CorrectiveActionPhotos
                .AnyAsync(p => p.CorrectiveActionId == ca.Id, cancellationToken);
            if (!hasPhoto)
                throw new InvalidOperationException(
                    "A closure photo is required before this corrective action can be closed.");
        }

        DateOnly? completedDate = null;
        if (!string.IsNullOrWhiteSpace(request.Payload.CompletedDate) &&
            DateOnly.TryParse(request.Payload.CompletedDate, out var parsed))
            completedDate = parsed;

        var now = DateTime.UtcNow;
        ca.Status = "Closed";
        ca.Description = string.IsNullOrWhiteSpace(request.Payload.Notes)
            ? ca.Description
            : $"{ca.Description}\n\nResolution: {request.Payload.Notes}";
        ca.CompletedDate = completedDate ?? DateOnly.FromDateTime(now);
        ca.ClosedDate = now;
        ca.UpdatedAt = now;
        ca.UpdatedBy = request.ClosedBy;

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("CloseCA", "CorrectiveAction", "Info",
            $"CA {ca.Id} closed by {request.ClosedBy}.",
            relatedObject: ca.Id.ToString());

        return Unit.Value;
    }
}
