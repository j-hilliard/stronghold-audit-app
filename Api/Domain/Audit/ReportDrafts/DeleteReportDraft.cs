using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.ReportDrafts;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin)]
public class DeleteReportDraft : IRequest
{
    public int DraftId { get; set; }
    public string DeletedBy { get; set; } = null!;
}

public class DeleteReportDraftHandler : IRequestHandler<DeleteReportDraft>
{
    private readonly AppDbContext _context;

    public DeleteReportDraftHandler(AppDbContext context) => _context = context;

    public async Task Handle(DeleteReportDraft request, CancellationToken cancellationToken)
    {
        var draft = await _context.ReportDrafts
            .FirstOrDefaultAsync(d => d.Id == request.DraftId, cancellationToken)
            ?? throw new KeyNotFoundException($"Report draft {request.DraftId} not found.");

        // Division scope — derived from persisted record
        await DivisionAuth.AssertAccessAsync(_context, request.DeletedBy, draft.DivisionId, cancellationToken);

        var now = DateTime.UtcNow;
        draft.IsDeleted = true;
        draft.DeletedAt = now;
        draft.DeletedBy = request.DeletedBy;
        draft.UpdatedAt = now;
        draft.UpdatedBy = request.DeletedBy;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
