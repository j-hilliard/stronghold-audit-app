using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

public class QuestionHistoryItemDto
{
    public int      AuditId    { get; set; }
    /// <summary>ISO date "YYYY-MM-DD" or null</summary>
    public DateOnly? AuditDate  { get; set; }
    public string?  Auditor    { get; set; }
    /// <summary>"Conforming" | "NonConforming" | "Warning" | "NA"</summary>
    public string?  Status     { get; set; }
    public string?  Comment    { get; set; }
    public string?  JobNumber  { get; set; }
}

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.ReadOnlyViewer,
    AuthorizationRole.Auditor, AuthorizationRole.AuditAdmin, AuthorizationRole.Executive)]
public class GetQuestionHistory : IRequest<List<QuestionHistoryItemDto>>
{
    public int QuestionId  { get; set; }
    public int DivisionId  { get; set; }
    public int Limit       { get; set; } = 3;
}

public class GetQuestionHistoryHandler : IRequestHandler<GetQuestionHistory, List<QuestionHistoryItemDto>>
{
    private readonly AppDbContext _context;

    public GetQuestionHistoryHandler(AppDbContext context) => _context = context;

    public async Task<List<QuestionHistoryItemDto>> Handle(GetQuestionHistory request, CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(request.Limit, 1, 10);

        return await _context.AuditResponses
            .Where(r =>
                r.QuestionId == request.QuestionId
                && r.Audit.DivisionId == request.DivisionId
                && (r.Audit.Status == "Submitted" || r.Audit.Status == "Closed"))
            .OrderByDescending(r => r.Audit.SubmittedAt)
            .Take(limit)
            .Select(r => new QuestionHistoryItemDto
            {
                AuditId   = r.AuditId,
                AuditDate = r.Audit.Header != null ? r.Audit.Header.AuditDate : null,
                Auditor   = r.Audit.Header != null ? r.Audit.Header.Auditor   : null,
                Status    = r.Status,
                Comment   = r.Comment,
                JobNumber = r.Audit.Header != null ? r.Audit.Header.JobNumber : null,
            })
            .ToListAsync(cancellationToken);
    }
}
