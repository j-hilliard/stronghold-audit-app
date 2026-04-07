using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.ReportDrafts;

[AllowedAuthorizationRole(AuthorizationRole.AuthenticatedUser)]
public class GetReportDraft : IRequest<ReportDraftDto?>
{
    public int DraftId { get; set; }
    public string RequestedBy { get; set; } = null!;
}

public class GetReportDraftHandler : IRequestHandler<GetReportDraft, ReportDraftDto?>
{
    private readonly AppDbContext _context;

    public GetReportDraftHandler(AppDbContext context) => _context = context;

    public async Task<ReportDraftDto?> Handle(GetReportDraft request, CancellationToken cancellationToken)
    {
        var draft = await _context.ReportDrafts
            .Include(d => d.Division)
            .FirstOrDefaultAsync(d => d.Id == request.DraftId, cancellationToken);

        if (draft == null) return null;

        // Derive division from the persisted record — do not trust an external param
        await DivisionAuth.AssertAccessAsync(_context, request.RequestedBy, draft.DivisionId, cancellationToken);

        return new ReportDraftDto
        {
            Id = draft.Id,
            DivisionId = draft.DivisionId,
            DivisionCode = draft.Division.Code,
            Title = draft.Title,
            Period = draft.Period,
            DateFrom = draft.DateFrom.HasValue ? draft.DateFrom.Value.ToString("yyyy-MM-dd") : null,
            DateTo = draft.DateTo.HasValue ? draft.DateTo.Value.ToString("yyyy-MM-dd") : null,
            BlocksJson = draft.BlocksJson,
            RowVersion = Convert.ToBase64String(draft.RowVersion),
            CreatedAt = draft.CreatedAt,
            UpdatedAt = draft.UpdatedAt,
            CreatedBy = draft.CreatedBy,
        };
    }
}
