using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

// ── Add ───────────────────────────────────────────────────────────────────────

[AllowedAuthorizationRole(AuthorizationRole.AuditAdmin, AuthorizationRole.Administrator, AuthorizationRole.TemplateAdmin)]
public class AddDistributionRecipient : IRequest<DistributionRecipientDto>
{
    public int AuditId { get; set; }
    public string Email { get; set; } = null!;
    public string? Name { get; set; }
    public string AddedBy { get; set; } = null!;
}

public class AddDistributionRecipientHandler : IRequestHandler<AddDistributionRecipient, DistributionRecipientDto>
{
    private readonly AppDbContext _context;

    public AddDistributionRecipientHandler(AppDbContext context) => _context = context;

    public async Task<DistributionRecipientDto> Handle(AddDistributionRecipient request, CancellationToken cancellationToken)
    {
        var exists = await _context.Audits.AnyAsync(a => a.Id == request.AuditId, cancellationToken);
        if (!exists) throw new KeyNotFoundException($"Audit {request.AuditId} not found.");

        var entry = new AuditDistributionRecipient
        {
            AuditId = request.AuditId,
            EmailAddress = request.Email.Trim().ToLowerInvariant(),
            Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name.Trim(),
            AddedBy = request.AddedBy,
            AddedAt = DateTime.UtcNow,
        };

        _context.AuditDistributionRecipients.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return new DistributionRecipientDto { Id = entry.Id, EmailAddress = entry.EmailAddress, Name = entry.Name };
    }
}

// ── Remove ────────────────────────────────────────────────────────────────────

[AllowedAuthorizationRole(AuthorizationRole.AuditAdmin, AuthorizationRole.Administrator, AuthorizationRole.TemplateAdmin)]
public class RemoveDistributionRecipient : IRequest<Unit>
{
    public int AuditId { get; set; }
    public int RecipientId { get; set; }
}

public class RemoveDistributionRecipientHandler : IRequestHandler<RemoveDistributionRecipient, Unit>
{
    private readonly AppDbContext _context;

    public RemoveDistributionRecipientHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(RemoveDistributionRecipient request, CancellationToken cancellationToken)
    {
        var entry = await _context.AuditDistributionRecipients
            .FirstOrDefaultAsync(r => r.Id == request.RecipientId && r.AuditId == request.AuditId, cancellationToken)
            ?? throw new KeyNotFoundException($"Recipient {request.RecipientId} not found on audit {request.AuditId}.");

        _context.AuditDistributionRecipients.Remove(entry);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
