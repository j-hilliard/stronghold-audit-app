using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer)]
public class CreateCaPublicToken : IRequest<CreateCaPublicTokenResult>
{
    public int       CorrectiveActionId { get; set; }
    public string?   SentToName        { get; set; }
    public string?   SentToEmail       { get; set; }
    public DateTime? ExpiresAt         { get; set; }
    public string    CreatedBy         { get; set; } = null!;
}

public class CreateCaPublicTokenResult
{
    public int    Id    { get; set; }
    public string Token { get; set; } = null!;
}

public class CreateCaPublicTokenHandler : IRequestHandler<CreateCaPublicToken, CreateCaPublicTokenResult>
{
    private readonly AppDbContext       _context;
    private readonly IProcessLogService _log;

    public CreateCaPublicTokenHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log     = log;
    }

    public async Task<CreateCaPublicTokenResult> Handle(CreateCaPublicToken request, CancellationToken cancellationToken)
    {
        var ca = await _context.CorrectiveActions
            .FirstOrDefaultAsync(c => c.Id == request.CorrectiveActionId, cancellationToken)
            ?? throw new ArgumentException($"Corrective action {request.CorrectiveActionId} not found.");

        if (ca.Status is "Closed" or "Voided")
            throw new InvalidOperationException("Cannot create a public token for a closed or voided corrective action.");

        var token = new CaPublicToken
        {
            CorrectiveActionId = request.CorrectiveActionId,
            Token              = Guid.NewGuid().ToString("N"),
            SentToName         = request.SentToName,
            SentToEmail        = request.SentToEmail,
            CreatedAt          = DateTime.UtcNow,
            CreatedBy          = request.CreatedBy,
            ExpiresAt          = request.ExpiresAt,
            IsRevoked          = false,
        };

        _context.CaPublicTokens.Add(token);
        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("CreateCaPublicToken", "CorrectiveAction", "Info",
            $"Public access token created for CA {ca.Id} by {request.CreatedBy}. Sent to: {request.SentToEmail ?? "N/A"}",
            relatedObject: ca.Id.ToString());

        return new CreateCaPublicTokenResult { Id = token.Id, Token = token.Token };
    }
}
