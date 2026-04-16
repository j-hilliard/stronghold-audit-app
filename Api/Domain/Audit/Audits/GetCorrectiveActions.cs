using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.ReadOnlyViewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class GetCorrectiveActions : IRequest<List<CorrectiveActionListItemDto>>
{
    public int? DivisionId { get; set; }
    public string? Status { get; set; }  // null = all
}

public class CorrectiveActionListItemDto
{
    public int Id { get; set; }
    public int AuditId { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string DivisionName { get; set; } = null!;
    public string? JobNumber { get; set; }
    public string? AuditDate { get; set; }
    public string Description { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? AssignedTo { get; set; }
    public string? DueDate { get; set; }
    public string? CompletedDate { get; set; }
    public bool IsOverdue { get; set; }
    public string QuestionText { get; set; } = null!;
    public string SectionName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class GetCorrectiveActionsHandler : IRequestHandler<GetCorrectiveActions, List<CorrectiveActionListItemDto>>
{
    private readonly AppDbContext _context;
    private readonly IAuditUserContext _userContext;

    public GetCorrectiveActionsHandler(AppDbContext context, IAuditUserContext userContext)
    {
        _context = context;
        _userContext = userContext;
    }

    public async Task<List<CorrectiveActionListItemDto>> Handle(GetCorrectiveActions request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = _context.CorrectiveActions
            .Include(ca => ca.Finding)
                .ThenInclude(f => f.Audit)
                    .ThenInclude(a => a.Division)
            .Include(ca => ca.Finding)
                .ThenInclude(f => f.Audit)
                    .ThenInclude(a => a.Header)
            .AsNoTracking()
            .AsQueryable();

        // Division scope: scoped users only see their assigned divisions
        if (!_userContext.IsGlobal && _userContext.AllowedDivisionIds is { Count: > 0 } allowed)
            query = query.Where(ca => allowed.Contains(ca.Finding.Audit.DivisionId));

        if (request.DivisionId.HasValue)
            query = query.Where(ca => ca.Finding.Audit.DivisionId == request.DivisionId.Value);

        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(ca => ca.Status == request.Status);

        var items = await query
            .OrderByDescending(ca => ca.CreatedAt)
            .Select(ca => new CorrectiveActionListItemDto
            {
                Id = ca.Id,
                AuditId = ca.Finding.AuditId,
                DivisionCode = ca.Finding.Audit.Division.Code,
                DivisionName = ca.Finding.Audit.Division.Name,
                JobNumber = ca.Finding.Audit.Header != null ? ca.Finding.Audit.Header.JobNumber : null,
                AuditDate = ca.Finding.Audit.Header != null && ca.Finding.Audit.Header.AuditDate.HasValue
                    ? ca.Finding.Audit.Header.AuditDate.Value.ToString("yyyy-MM-dd")
                    : null,
                Description = ca.Description,
                Status = ca.Status,
                AssignedTo = ca.AssignedTo,
                DueDate = ca.DueDate.HasValue ? ca.DueDate.Value.ToString("yyyy-MM-dd") : null,
                CompletedDate = ca.CompletedDate.HasValue ? ca.CompletedDate.Value.ToString("yyyy-MM-dd") : null,
                IsOverdue = ca.Status != "Closed" && ca.DueDate.HasValue && ca.DueDate.Value < today,
                QuestionText = ca.Finding.QuestionTextSnapshot,
                SectionName = "",
                CreatedAt = ca.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return items;
    }
}
