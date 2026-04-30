using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Data;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

public class GetCaByPublicToken : IRequest<CaPublicAccessDto>
{
    public string Token { get; set; } = null!;
}

public class CaPublicAccessDto
{
    public int       TokenId            { get; set; }
    public int       CorrectiveActionId { get; set; }
    public string    Description        { get; set; } = null!;
    public string?   RootCause          { get; set; }
    public string    Status             { get; set; } = null!;
    public string    Priority           { get; set; } = null!;
    public string?   DueDate            { get; set; }
    public string?   AuditTitle         { get; set; }
    public string?   AssignedTo         { get; set; }
    public string?   SentToName         { get; set; }
    public DateTime  CreatedAt          { get; set; }
    public DateTime? ExpiresAt          { get; set; }
}

public class GetCaByPublicTokenHandler : IRequestHandler<GetCaByPublicToken, CaPublicAccessDto>
{
    private readonly AppDbContext _context;

    public GetCaByPublicTokenHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CaPublicAccessDto> Handle(GetCaByPublicToken request, CancellationToken cancellationToken)
    {
        var record = await _context.CaPublicTokens
            .Include(t => t.CorrectiveAction)
                .ThenInclude(c => c.Audit)
            .FirstOrDefaultAsync(t => t.Token == request.Token, cancellationToken)
            ?? throw new ArgumentException("Invalid or expired access link.");

        if (record.IsRevoked)
            throw new UnauthorizedAccessException("This access link has been revoked.");

        if (record.ExpiresAt.HasValue && record.ExpiresAt.Value < DateTime.UtcNow)
            throw new UnauthorizedAccessException("This access link has expired.");

        var ca = record.CorrectiveAction;

        if (ca.Status is "Closed" or "Voided")
            throw new InvalidOperationException("This corrective action is no longer open for updates.");

        return new CaPublicAccessDto
        {
            TokenId            = record.Id,
            CorrectiveActionId = ca.Id,
            Description        = ca.Description,
            RootCause          = ca.RootCause,
            Status             = ca.Status,
            Priority           = ca.Priority,
            DueDate            = ca.DueDate?.ToString("yyyy-MM-dd"),
            AuditTitle         = ca.Audit?.TrackingNumber ?? (ca.Audit != null ? $"Audit #{ca.Audit.Id}" : null),
            AssignedTo         = ca.AssignedTo,
            SentToName         = record.SentToName,
            CreatedAt          = record.CreatedAt,
            ExpiresAt          = record.ExpiresAt,
        };
    }
}
