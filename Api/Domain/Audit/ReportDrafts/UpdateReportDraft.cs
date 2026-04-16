using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.ReportDrafts;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator)]
public class UpdateReportDraft : IRequest
{
    public int DraftId { get; set; }
    public UpdateReportDraftRequest Payload { get; set; } = null!;
    public string UpdatedBy { get; set; } = null!;
}

public class UpdateReportDraftHandler : IRequestHandler<UpdateReportDraft>
{
    private readonly AppDbContext _context;

    public UpdateReportDraftHandler(AppDbContext context) => _context = context;

    public async Task Handle(UpdateReportDraft request, CancellationToken cancellationToken)
    {
        var p = request.Payload;

        BlocksJsonValidator.Validate(p.BlocksJson);

        var draft = await _context.ReportDrafts
            .FirstOrDefaultAsync(d => d.Id == request.DraftId, cancellationToken)
            ?? throw new KeyNotFoundException($"Report draft {request.DraftId} not found.");

        // Division scope — derived from persisted record, not caller-supplied
        await DivisionAuth.AssertAccessAsync(_context, request.UpdatedBy, draft.DivisionId, cancellationToken);

        // Optimistic concurrency: set the original row version so EF checks it in the UPDATE WHERE clause
        _context.Entry(draft).Property(d => d.RowVersion).OriginalValue =
            Convert.FromBase64String(p.RowVersion);

        var (dateFrom, dateTo) = CreateReportDraftHandler.ParseDates(p.DateFrom, p.DateTo);

        draft.Title = p.Title.Trim();
        draft.Period = p.Period.Trim();
        draft.DateFrom = dateFrom;
        draft.DateTo = dateTo;
        draft.BlocksJson = p.BlocksJson;
        draft.UpdatedAt = DateTime.UtcNow;
        draft.UpdatedBy = request.UpdatedBy;

        await _context.SaveChangesAsync(cancellationToken);
        // DbUpdateConcurrencyException propagates to controller → mapped to 409 Conflict by V1ControllerBase
    }
}
