using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;
using AuditHeaderEntity = Stronghold.AppDashboard.Data.Models.Audit.AuditHeader;
using AuditResponseEntity = Stronghold.AppDashboard.Data.Models.Audit.AuditResponse;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(AuthorizationRole.AuthenticatedUser)]
public class SaveAuditResponses : IRequest<Unit>
{
    public int AuditId { get; set; }
    public string SavedBy { get; set; } = null!;
    public AuditHeaderDto? Header { get; set; }
    public List<AuditResponseUpsertDto> Responses { get; set; } = new();
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
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, cancellationToken)
            ?? throw new ArgumentException($"Audit {request.AuditId} not found.");

        if (audit.Status != "Draft" && audit.Status != "Reopened")
            throw new InvalidOperationException($"Audit {request.AuditId} is {audit.Status} and cannot be edited.");

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
                h.UpdatedAt = now;
                h.UpdatedBy = request.SavedBy;
            }
        }

        // ── Responses — full upsert ───────────────────────────────────────────
        var existingByQuestionId = audit.Responses.ToDictionary(r => r.QuestionId);

        foreach (var dto in request.Responses)
        {
            if (existingByQuestionId.TryGetValue(dto.QuestionId, out var existing))
            {
                existing.QuestionTextSnapshot = dto.QuestionTextSnapshot;
                existing.Status = dto.Status;
                existing.Comment = dto.Comment;
                existing.CorrectedOnSite = dto.Status == "NonConforming" && dto.CorrectedOnSite;
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
                    CreatedAt = now,
                    CreatedBy = request.SavedBy
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
