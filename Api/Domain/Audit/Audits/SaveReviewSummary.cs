using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(AuthorizationRole.AuditAdmin, AuthorizationRole.Administrator, AuthorizationRole.TemplateAdmin)]
public class SaveReviewSummary : IRequest<Unit>
{
    public int AuditId { get; set; }
    public string? Summary { get; set; }
}

public class SaveReviewSummaryHandler : IRequestHandler<SaveReviewSummary, Unit>
{
    private readonly AppDbContext _context;

    public SaveReviewSummaryHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(SaveReviewSummary request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, cancellationToken)
            ?? throw new KeyNotFoundException($"Audit {request.AuditId} not found.");

        audit.ReviewSummary = string.IsNullOrWhiteSpace(request.Summary) ? null : request.Summary.Trim();
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
