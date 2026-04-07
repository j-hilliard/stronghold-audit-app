using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;
using AuditFindingEntity = Stronghold.AppDashboard.Data.Models.Audit.AuditFinding;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(AuthorizationRole.AuthenticatedUser)]
public class SubmitAudit : IRequest<Unit>
{
    public int AuditId { get; set; }
    public string SubmittedBy { get; set; } = null!;
}

public class SubmitAuditHandler : IRequestHandler<SubmitAudit, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public SubmitAuditHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Unit> Handle(SubmitAudit request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .Include(a => a.Responses)
            .Include(a => a.Findings)
            .FirstOrDefaultAsync(a => a.Id == request.AuditId, cancellationToken)
            ?? throw new ArgumentException($"Audit {request.AuditId} not found.");

        if (audit.Status != "Draft" && audit.Status != "Reopened")
            throw new InvalidOperationException($"Audit {request.AuditId} is already {audit.Status}.");

        var now = DateTime.UtcNow;

        // ── Generate AuditFinding records for each NonConforming response ─────
        // Remove any existing findings first (safe on resubmit after reopen)
        _context.AuditFindings.RemoveRange(audit.Findings);

        var nonConforming = audit.Responses
            .Where(r => r.Status == "NonConforming")
            .ToList();

        foreach (var response in nonConforming)
        {
            _context.AuditFindings.Add(new AuditFindingEntity
            {
                AuditId = audit.Id,
                QuestionId = response.QuestionId,
                QuestionTextSnapshot = response.QuestionTextSnapshot,
                Description = response.Comment,
                CorrectedOnSite = response.CorrectedOnSite,
                CreatedAt = now,
                CreatedBy = request.SubmittedBy
            });
        }

        audit.Status = "Submitted";
        audit.SubmittedAt = now;
        audit.UpdatedAt = now;
        audit.UpdatedBy = request.SubmittedBy;

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("SubmitAudit", "Audit", "Info",
            $"Audit {audit.Id} submitted by {request.SubmittedBy}. {nonConforming.Count} finding(s) generated.",
            relatedObject: audit.Id.ToString());

        return Unit.Value;
    }
}
