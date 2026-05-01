using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;
using AuditHeaderEntity = Stronghold.AppDashboard.Data.Models.Audit.AuditHeader;
using AuditResponseEntity = Stronghold.AppDashboard.Data.Models.Audit.AuditResponse;
using AuditSectionNaOverrideEntity = Stronghold.AppDashboard.Data.Models.Audit.AuditSectionNaOverride;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.Auditor, AuthorizationRole.AuditAdmin)]
public class SaveAuditResponses : IRequest<Unit>
{
    public int AuditId { get; set; }
    public string SavedBy { get; set; } = null!;
    /// <summary>True when the caller holds an admin/reviewer role. Governs which statuses allow editing.</summary>
    public bool SaverIsReviewer { get; set; }
    public AuditHeaderDto? Header { get; set; }
    public List<AuditResponseUpsertDto> Responses { get; set; } = new();
    public List<SectionNaOverrideDto> SectionNaOverrides { get; set; } = new();
}

public class SaveAuditResponsesHandler : IRequestHandler<SaveAuditResponses, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public SaveAuditResponsesHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Unit> Handle(SaveAuditResponses request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .Include(a => a.Header)
            .Include(a => a.Responses)
            .Include(a => a.SectionNaOverrides)
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, cancellationToken)
            ?? throw new ArgumentException($"Audit {request.AuditId} not found.");

        var canEdit = audit.Status switch
        {
            "Draft"       => true,
            "Reopened"    => true,
            "UnderReview" => request.SaverIsReviewer,
            _             => false,
        };

        if (!canEdit)
        {
            var reason = audit.Status == "UnderReview"
                ? "Cannot save audit responses: only admins and reviewers can edit an audit that is Under Review."
                : $"Cannot save audit responses: audit is '{audit.Status}' and cannot be edited.";
            throw new InvalidOperationException(reason);
        }

        var now = DateTime.UtcNow;

        // ── Header ────────────────────────────────────────────────────────────
        if (request.Header != null)
        {
            DateOnly? parsedDate = null;
            if (!string.IsNullOrWhiteSpace(request.Header.AuditDate) &&
                DateOnly.TryParse(request.Header.AuditDate, out var d))
                parsedDate = d;

            if (audit.Header == null)
            {
                var header = new AuditHeaderEntity
                {
                    AuditId = audit.Id,
                    JobNumber = request.Header.JobNumber,
                    Client = request.Header.Client,
                    PM = request.Header.PM,
                    Unit = request.Header.Unit,
                    Time = request.Header.Time,
                    Shift = request.Header.Shift,
                    WorkDescription = request.Header.WorkDescription,
                    Company1 = request.Header.Company1,
                    Company2 = request.Header.Company2,
                    Company3 = request.Header.Company3,
                    ResponsibleParty = request.Header.ResponsibleParty,
                    Location = request.Header.Location,
                    AuditDate = parsedDate,
                    Auditor = request.Header.Auditor,
                    SiteCode = request.Header.SiteCode,
                    CreatedAt = now,
                    CreatedBy = request.SavedBy
                };
                _context.AuditHeaders.Add(header);
            }
            else
            {
                var h = audit.Header;
                h.JobNumber = request.Header.JobNumber;
                h.Client = request.Header.Client;
                h.PM = request.Header.PM;
                h.Unit = request.Header.Unit;
                h.Time = request.Header.Time;
                h.Shift = request.Header.Shift;
                h.WorkDescription = request.Header.WorkDescription;
                h.Company1 = request.Header.Company1;
                h.Company2 = request.Header.Company2;
                h.Company3 = request.Header.Company3;
                h.ResponsibleParty = request.Header.ResponsibleParty;
                h.Location = request.Header.Location;
                h.AuditDate = parsedDate;
                h.Auditor = request.Header.Auditor;
                h.SiteCode = request.Header.SiteCode;
                h.UpdatedAt = now;
                h.UpdatedBy = request.SavedBy;
            }

            // Keep Audit.TrackingNumber in sync with the (possibly changed) site code.
            if (audit.TrackingNumber != null)
            {
                var parts = audit.TrackingNumber.Split('-');
                var trackingBase = string.Join("-", parts.Take(2));
                var newSite = request.Header.SiteCode?.Trim().ToUpperInvariant();
                audit.TrackingNumber = string.IsNullOrEmpty(newSite)
                    ? trackingBase
                    : $"{trackingBase}-{newSite}";
            }
        }

        // ── Weight lookup — snapshot at save time so scores stay deterministic ─
        // Effective question weight = version-level override if set, else question default.
        var versionQuestions = await _context.AuditVersionQuestions
            .Include(vq => vq.Question)
            .Include(vq => vq.Section)
            .Where(vq => vq.TemplateVersionId == audit.TemplateVersionId)
            .ToListAsync(cancellationToken);

        var weightByQuestionId = versionQuestions.ToDictionary(
            vq => vq.QuestionId,
            vq => (
                QuestionWeight: vq.Weight ?? vq.Question.Weight,
                SectionWeight: vq.Section.Weight,
                IsLifeCritical: vq.Question.IsLifeCritical
            )
        );

        // ── Responses — full upsert ───────────────────────────────────────────
        var existingByQuestionId = audit.Responses.ToDictionary(r => r.QuestionId);

        foreach (var dto in request.Responses)
        {
            var (qw, sw, lc) = weightByQuestionId.TryGetValue(dto.QuestionId, out var w)
                ? w : (1.0m, 1.0m, false);

            if (existingByQuestionId.TryGetValue(dto.QuestionId, out var existing))
            {
                existing.QuestionTextSnapshot = dto.QuestionTextSnapshot;
                existing.Status = dto.Status;
                existing.Comment = dto.Comment;
                existing.CorrectedOnSite = dto.Status == "NonConforming" && dto.CorrectedOnSite;
                existing.QuestionWeightSnapshot = qw;
                existing.SectionWeightSnapshot = sw;
                existing.IsLifeCriticalSnapshot = lc;
                existing.UpdatedAt = now;
                existing.UpdatedBy = request.SavedBy;
            }
            else
            {
                _context.AuditResponses.Add(new AuditResponseEntity
                {
                    AuditId = audit.Id,
                    QuestionId = dto.QuestionId,
                    QuestionTextSnapshot = dto.QuestionTextSnapshot,
                    Status = dto.Status,
                    Comment = dto.Comment,
                    CorrectedOnSite = dto.Status == "NonConforming" && dto.CorrectedOnSite,
                    QuestionWeightSnapshot = qw,
                    SectionWeightSnapshot = sw,
                    IsLifeCriticalSnapshot = lc,
                    CreatedAt = now,
                    CreatedBy = request.SavedBy
                });
            }
        }

        // ── Section N/A overrides — full replace ─────────────────────────────
        var existingNa = audit.SectionNaOverrides.ToDictionary(n => n.SectionId);
        var incomingNa = request.SectionNaOverrides.ToDictionary(n => n.SectionId);

        // Remove overrides no longer in the incoming set
        foreach (var sectionId in existingNa.Keys.Except(incomingNa.Keys).ToList())
            _context.AuditSectionNaOverrides.Remove(existingNa[sectionId]);

        // Add or update
        foreach (var dto in request.SectionNaOverrides)
        {
            if (existingNa.TryGetValue(dto.SectionId, out var existing))
            {
                existing.Reason = dto.Reason;
            }
            else
            {
                _context.AuditSectionNaOverrides.Add(new AuditSectionNaOverrideEntity
                {
                    AuditId   = audit.Id,
                    SectionId = dto.SectionId,
                    Reason    = dto.Reason,
                    CreatedAt = now,
                    CreatedBy = request.SavedBy,
                });
            }
        }

        audit.UpdatedAt = now;
        audit.UpdatedBy = request.SavedBy;

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("SaveAuditResponses", "Audit", "Info",
            $"Audit {audit.Id} draft saved by {request.SavedBy} ({request.Responses.Count} responses)",
            relatedObject: audit.Id.ToString());

        return Unit.Value;
    }
}
