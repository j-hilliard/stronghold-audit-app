using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(AuthorizationRole.AuthenticatedUser)]
public class GetAuditReview : IRequest<AuditReviewDto?>
{
    public int AuditId { get; set; }
}

public class GetAuditReviewHandler : IRequestHandler<GetAuditReview, AuditReviewDto?>
{
    private readonly AppDbContext _context;

    public GetAuditReviewHandler(AppDbContext context) => _context = context;

    public async Task<AuditReviewDto?> Handle(GetAuditReview request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .Include(a => a.Division)
            .Include(a => a.Header)
            .Include(a => a.Responses)
            .Include(a => a.Findings).ThenInclude(f => f.CorrectiveActions)
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, cancellationToken);

        if (audit == null) return null;

        // ── Score calculation ─────────────────────────────────────────────────
        // Formula: Conforming / (Conforming + NonConforming + Warning) * 100
        // N/A and unanswered are excluded from numerator and denominator.
        var responses = audit.Responses.ToList();
        int conforming = responses.Count(r => r.Status == "Conforming");
        int nonConforming = responses.Count(r => r.Status == "NonConforming");
        int warning = responses.Count(r => r.Status == "Warning");
        int na = responses.Count(r => r.Status == "NA");
        int unanswered = responses.Count(r => r.Status == null);

        int denominator = conforming + nonConforming + warning;
        double? score = denominator > 0
            ? Math.Round((double)conforming / denominator * 100, 1)
            : null;

        // ── Email routing — division-specific + global ───────────────────────
        var emailRules = await _context.EmailRoutingRules
            .Where(r => r.DivisionId == audit.DivisionId && r.IsActive)
            .OrderBy(r => r.Id)
            .ToListAsync(cancellationToken);

        return new AuditReviewDto
        {
            Id = audit.Id,
            DivisionCode = audit.Division.Code,
            DivisionName = audit.Division.Name,
            AuditType = audit.AuditType,
            Status = audit.Status,
            Header = audit.Header == null ? null : new AuditHeaderDto
            {
                Id = audit.Header.Id,
                JobNumber = audit.Header.JobNumber,
                Client = audit.Header.Client,
                PM = audit.Header.PM,
                Unit = audit.Header.Unit,
                Time = audit.Header.Time,
                Shift = audit.Header.Shift,
                WorkDescription = audit.Header.WorkDescription,
                Company1 = audit.Header.Company1,
                Company2 = audit.Header.Company2,
                Company3 = audit.Header.Company3,
                ResponsibleParty = audit.Header.ResponsibleParty,
                Location = audit.Header.Location,
                AuditDate = audit.Header.AuditDate?.ToString("yyyy-MM-dd"),
                Auditor = audit.Header.Auditor
            },
            ConformingCount = conforming,
            NonConformingCount = nonConforming,
            WarningCount = warning,
            NaCount = na,
            UnansweredCount = unanswered,
            ScorePercent = score,
            NonConformingItems = audit.Findings
                .Where(f => !f.IsDeleted)
                .OrderBy(f => f.Id)
                .Select(f => new AuditFindingDto
                {
                    Id = f.Id,
                    QuestionText = f.QuestionTextSnapshot,
                    Comment = f.Description,
                    CorrectedOnSite = f.CorrectedOnSite,
                    CorrectiveActions = f.CorrectiveActions
                        .Where(ca => !ca.IsDeleted)
                        .OrderBy(ca => ca.CreatedAt)
                        .Select(ca => new CorrectiveActionDto
                        {
                            Id = ca.Id,
                            FindingId = ca.FindingId,
                            AuditId = ca.AuditId,
                            Description = ca.Description,
                            AssignedTo = ca.AssignedTo,
                            DueDate = ca.DueDate?.ToString("yyyy-MM-dd"),
                            CompletedDate = ca.CompletedDate?.ToString("yyyy-MM-dd"),
                            Status = ca.Status,
                            CreatedBy = ca.CreatedBy,
                            CreatedAt = ca.CreatedAt,
                        }).ToList()
                }).ToList(),
            WarningItems = responses
                .Where(r => r.Status == "Warning")
                .OrderBy(r => r.SortOrderSnapshot ?? 999)
                .Select(r => new ReviewResponseItemDto
                {
                    QuestionId = r.QuestionId,
                    QuestionText = r.QuestionTextSnapshot,
                    Status = r.Status,
                    Comment = r.Comment,
                    CorrectedOnSite = r.CorrectedOnSite,
                    SortOrder = r.SortOrderSnapshot ?? 0,
                }).ToList(),
            Sections = responses
                .GroupBy(r => r.SectionNameSnapshot ?? "General")
                .OrderBy(g => g.Min(r => r.SortOrderSnapshot ?? 999))
                .Select(g => new ReviewSectionDto
                {
                    SectionName = g.Key,
                    Items = g.OrderBy(r => r.SortOrderSnapshot ?? 999)
                        .Select(r => new ReviewResponseItemDto
                        {
                            QuestionId = r.QuestionId,
                            QuestionText = r.QuestionTextSnapshot,
                            Status = r.Status,
                            Comment = r.Comment,
                            CorrectedOnSite = r.CorrectedOnSite,
                            SortOrder = r.SortOrderSnapshot ?? 0,
                        }).ToList()
                }).ToList(),
            ReviewEmailRouting = emailRules
                .Select(r => new EmailRoutingDto { EmailAddress = r.EmailAddress })
                .ToList()
        };
    }
}
