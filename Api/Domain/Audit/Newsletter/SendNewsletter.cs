using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Newsletter;

[AllowedAuthorizationRole(AuthorizationRole.AuthenticatedUser)]
public class SendNewsletter : IRequest<NewsletterSendResult>
{
    public int DivisionId { get; set; }
    public string Subject { get; set; } = null!;
    public string HtmlBody { get; set; } = null!;
}

public class SendNewsletterHandler : IRequestHandler<SendNewsletter, NewsletterSendResult>
{
    private readonly AppDbContext _context;
    private readonly ILogger<SendNewsletterHandler> _logger;

    public SendNewsletterHandler(AppDbContext context, ILogger<SendNewsletterHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<NewsletterSendResult> Handle(
        SendNewsletter request,
        CancellationToken cancellationToken)
    {
        // Pull active email routing rules for this division
        var recipients = await _context.EmailRoutingRules
            .Where(r => r.DivisionId == request.DivisionId && r.IsActive && !r.IsDeleted)
            .Select(r => r.EmailAddress)
            .ToListAsync(cancellationToken);

        // Phase 1 — dry run: log to application log and return metadata.
        // Real SMTP wiring happens in Phase 2 once credentials are confirmed.
        _logger.LogInformation(
            "[Newsletter DRY RUN] Subject: {Subject} | Division: {DivisionId} | Recipients ({Count}): {Recipients} | Body length: {BodyLen}",
            request.Subject,
            request.DivisionId,
            recipients.Count,
            string.Join(", ", recipients),
            request.HtmlBody.Length);

        return new NewsletterSendResult
        {
            Sent = 0,
            DryRun = true,
            Recipients = recipients,
        };
    }
}
