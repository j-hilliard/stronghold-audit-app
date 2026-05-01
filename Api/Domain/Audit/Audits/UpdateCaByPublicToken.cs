using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

public class UpdateCaByPublicToken : IRequest<Unit>
{
    public string  Token         { get; set; } = null!;
    /// <summary>"InProgress" or "Submitted" — external parties mark progress or declare work done.</summary>
    public string  NewStatus     { get; set; } = null!;
    public string? Notes         { get; set; }
    public string? UpdatedByName { get; set; }
}

public class UpdateCaByPublicTokenHandler : IRequestHandler<UpdateCaByPublicToken, Unit>
{
    private readonly AppDbContext       _context;
    private readonly IProcessLogService _log;

    public UpdateCaByPublicTokenHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log     = log;
    }

    public async Task<Unit> Handle(UpdateCaByPublicToken request, CancellationToken cancellationToken)
    {
        var record = await _context.CaPublicTokens
            .Include(t => t.CorrectiveAction)
            .FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken)
            ?? throw new ArgumentException("Invalid or expired access link.");

        if (record.IsRevoked)
            throw new UnauthorizedAccessException("This access link has been revoked.");

        if (record.ExpiresAt.HasValue && record.ExpiresAt.Value < DateTime.UtcNow)
            throw new UnauthorizedAccessException("This access link has expired.");

        var ca = record.CorrectiveAction;

        if (ca.Status is "Closed" or "Voided")
            throw new InvalidOperationException("This corrective action is no longer open for updates.");

        // External parties may mark InProgress (started work) or Submitted (work done, pending admin close)
        if (request.NewStatus is not ("InProgress" or "Submitted"))
            throw new ArgumentException("External access allows marking corrective actions as 'InProgress' or 'Submitted'.");

        // Require at least one proof photo before declaring work complete
        if (request.NewStatus == "Submitted")
        {
            var hasPhoto = await _context.CorrectiveActionPhotos
                .AnyAsync(p => p.CorrectiveActionId == ca.Id, cancellationToken);
            if (!hasPhoto)
                throw new InvalidOperationException(
                    "A proof photo is required before submitting work as complete. Please upload a photo first.");
        }

        var now     = DateTime.UtcNow;
        var updater = request.UpdatedByName ?? record.SentToName ?? "External";

        ca.Status    = request.NewStatus;
        ca.UpdatedAt = now;
        ca.UpdatedBy = updater;

        if (!string.IsNullOrWhiteSpace(request.Notes))
            ca.Description = string.IsNullOrWhiteSpace(ca.Description)
                ? request.Notes
                : $"{ca.Description}\n\nExternal Update ({now:yyyy-MM-dd}): {request.Notes}";

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("ExternalCaUpdate", "CorrectiveAction", "Info",
            $"CA {ca.Id} updated to '{request.NewStatus}' via public token by {updater}.",
            relatedObject: ca.Id.ToString());

        return Unit.Value;
    }
}
