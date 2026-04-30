using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

public class UpdateCaByPublicToken : IRequest<Unit>
{
    public string  Token         { get; set; } = null!;
    /// <summary>Only "InProgress" is permitted — external parties cannot close a CA.</summary>
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

        // External parties may only mark as InProgress — closing requires an authenticated Admin/Auditor
        if (request.NewStatus != "InProgress")
            throw new ArgumentException("External access is limited to marking corrective actions as 'InProgress'.");

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
