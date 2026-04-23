using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.ReadOnlyViewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.Auditor, AuthorizationRole.AuditAdmin, AuthorizationRole.Executive)]
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
            .Include(a => a.Attachments)
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, cancellationToken);

        if (audit == null) return null;

        // ── Score calculation (two-level weighted) ────────────────────────────
        // Level 1: within each section → sectionScore = Σ(conforming × qWeight) / Σ(scored × qWeight)
        // Level 2: overall → Σ(sectionScore × sWeight) / Σ(sWeight with answered questions) × 100
        var responses = audit.Responses.ToList();
        int conforming = responses.Count(r => r.Status == "Conforming");
        int nonConforming = responses.Count(r => r.Status == "NonConforming");
        int warning = responses.Count(r => r.Status == "Warning");
        int na = responses.Count(r => r.Status == "NA");
        int unanswered = responses.Count(r => r.Status == null);

        double? score = GetAuditReportHandler.ComputeTwoLevelScore(responses);

        // ── Life-critical failure detection ───────────────────────────────────
        // If ANY life-critical question has a NonConforming response the audit auto-fails.
        var lifeCriticalNcItems = responses
            .Where(r => r.IsLifeCriticalSnapshot && r.Status == "NonConforming")
            .Select(r => r.QuestionTextSnapshot)
            .ToList();
        bool hasLifeCriticalFailure = lifeCriticalNcItems.Count > 0;

        // ── Email routing — division-specific + global ───────────────────────
        var emailRules = await _context.EmailRoutingRules
            .Where(r => r.DivisionId == audit.DivisionId && r.IsActive)
            .OrderBy(r => r.Id)
            .ToListAsync(cancellationToken);

        // ── Per-audit distribution recipients ────────────────────────────────
        var distributionRecipients = await _context.AuditDistributionRecipients
            .Where(r => r.AuditId == request.AuditId)
            .OrderBy(r => r.AddedAt)
            .ToListAsync(cancellationToken);

        // ── Benchmark: avg score of last 10 submitted audits in this division ─
        var refDate = audit.SubmittedAt ?? DateTime.UtcNow;
        var benchmarkAudits = await _context.Audits
            .AsNoTracking()
            .Where(a => a.DivisionId == audit.DivisionId
                     && a.Id != audit.Id
                     && (a.Status == "Submitted" || a.Status == "Closed"))
            .OrderByDescending(a => a.SubmittedAt)
            .Take(10)
            .Include(a => a.Responses)
            .ToListAsync(cancellationToken);

        var benchmarkScores = benchmarkAudits
            .Select(a => GetAuditReportHandler.ComputeTwoLevelScore(a.Responses))
            .Where(s => s.HasValue)
            .Select(s => s!.Value)
            .ToList();

        double? divisionAvgScore = benchmarkScores.Count > 0
            ? Math.Round(benchmarkScores.Average(), 1)
            : null;

        // ── Repeat findings: questions NonConforming in 2+ audits (180 days) ──
        var lookbackFrom = refDate.AddDays(-180);
        var divisionAuditIds = await _context.Audits
            .AsNoTracking()
            .Where(a => a.DivisionId == audit.DivisionId
                     && a.Status != "Draft"
                     && (a.SubmittedAt == null || a.SubmittedAt >= lookbackFrom)
                     && (a.SubmittedAt == null || a.SubmittedAt <= refDate))
            .Select(a => a.Id)
            .ToListAsync(cancellationToken);

        var repeatFindingQuestionIds = new List<int>();
        if (divisionAuditIds.Count > 0)
        {
            var ncResponses = await _context.AuditResponses
                .AsNoTracking()
                .Where(r => divisionAuditIds.Contains(r.AuditId) && r.Status == "NonConforming")
                .Select(r => new { r.AuditId, r.QuestionId, r.QuestionTextSnapshot, r.SectionNameSnapshot })
                .ToListAsync(cancellationToken);

            repeatFindingQuestionIds = ncResponses
                .GroupBy(r => new { r.QuestionTextSnapshot, r.SectionNameSnapshot })
                .Where(g => g.Select(r => r.AuditId).Distinct().Count() >= 2)
                .Select(g => g.Select(r => r.QuestionId).First())
                .ToList();
        }

        return new AuditReviewDto
        {
            Id = audit.Id,
            DivisionCode = audit.Division.Code,
            DivisionName = audit.Division.Name,
            AuditType = audit.AuditType,
            Status = audit.Status,
            AiSummary = audit.AiSummary,
            DivisionAvgScore = divisionAvgScore,
            DivisionScoreTarget = audit.Division.ScoreTarget,
            RepeatFindingQuestionIds = repeatFindingQuestionIds,
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
                Auditor = audit.Header.Auditor,
                SiteCode = audit.Header.SiteCode
            },
            TrackingNumber = audit.TrackingNumber,
            ConformingCount = conforming,
            NonConformingCount = nonConforming,
            WarningCount = warning,
            NaCount = na,
            UnansweredCount = unanswered,
            ScorePercent = score,
            HasLifeCriticalFailure = hasLifeCriticalFailure,
            LifeCriticalFailures = lifeCriticalNcItems,
            NonConformingItems = audit.Findings
                .Where(f => !f.IsDeleted)
                .OrderBy(f => f.Id)
                .Select(f => new AuditFindingDto
                {
                    Id = f.Id,
                    QuestionId = f.QuestionId,
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
                .ToList(),
            ReviewSummary = audit.ReviewSummary,
            ReviewedAt = audit.ReviewedAt,
            ReviewedBy = audit.ReviewedBy,
            DistributionRecipients = distributionRecipients
                .Select(r => new DistributionRecipientDto { Id = r.Id, EmailAddress = r.EmailAddress, Name = r.Name })
                .ToList(),
            Attachments = audit.Attachments
                .Where(a => !a.IsDeleted)
                .OrderByDescending(a => a.UploadedAt)
                .Select(a => new AuditAttachmentRefDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FileSizeBytes = a.FileSizeBytes,
                    HasFile = !string.IsNullOrEmpty(a.BlobPath) && File.Exists(a.BlobPath),
                })
                .ToList()
        };
    }
}
