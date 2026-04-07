using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(AuthorizationRole.AuthenticatedUser)]
public class GetAuditList : IRequest<List<AuditListItemDto>>
{
    public int? DivisionId { get; set; }
    public string? Status { get; set; }
    /// <summary>Filters by AuditHeader.AuditDate (the date on the form, not CreatedAt)</summary>
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    /// <summary>Filters by AuditHeader.Auditor (partial match)</summary>
    public string? Auditor { get; set; }
}

public class GetAuditListHandler : IRequestHandler<GetAuditList, List<AuditListItemDto>>
{
    private readonly AppDbContext _context;

    public GetAuditListHandler(AppDbContext context) => _context = context;

    public async Task<List<AuditListItemDto>> Handle(GetAuditList request, CancellationToken cancellationToken)
    {
        var query = _context.Audits
            .Include(a => a.Division)
            .Include(a => a.Header)
            .AsQueryable();

        if (request.DivisionId.HasValue)
            query = query.Where(a => a.DivisionId == request.DivisionId.Value);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(a => a.Status == request.Status);

        if (request.DateFrom.HasValue)
            query = query.Where(a => a.Header != null && a.Header.AuditDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(a => a.Header != null && a.Header.AuditDate <= request.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(request.Auditor))
            query = query.Where(a => a.Header != null && a.Header.Auditor != null &&
                a.Header.Auditor.Contains(request.Auditor));

        var audits = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        return audits.Select(a => new AuditListItemDto
        {
            Id = a.Id,
            DivisionCode = a.Division.Code,
            DivisionName = a.Division.Name,
            AuditType = a.AuditType,
            Status = a.Status,
            CreatedBy = a.CreatedBy,
            CreatedAt = a.CreatedAt,
            SubmittedAt = a.SubmittedAt,
            Auditor = a.Header?.Auditor,
            AuditDate = a.Header?.AuditDate?.ToString("yyyy-MM-dd"),
            JobNumber = a.Header?.JobNumber,
            Location = a.Header?.Location
        }).ToList();
    }
}
