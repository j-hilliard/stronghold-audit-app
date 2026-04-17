using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator)]
public class BulkUpdateCorrectiveActions : IRequest<BulkUpdateCorrectiveActionsResult>
{
    public List<int> CorrectiveActionIds { get; set; } = new();
    /// <summary>"status" or "reassign"</summary>
    public string Action { get; set; } = null!;
    /// <summary>For action="status": "InProgress" | "Closed" | "Voided"</summary>
    public string? NewStatus { get; set; }
    /// <summary>Required when NewStatus="Closed"</summary>
    public string? ClosureNotes { get; set; }
    /// <summary>For action="reassign"</summary>
    public string? NewAssignee { get; set; }
    public int?    NewAssigneeUserId { get; set; }
    public string  UpdatedBy { get; set; } = null!;
}

public class BulkUpdateCorrectiveActionsResult
{
    public int         SuccessCount { get; set; }
    public List<int>   FailedIds    { get; set; } = new();
    public List<string> Errors      { get; set; } = new();
}

public class BulkUpdateCorrectiveActionsHandler
    : IRequestHandler<BulkUpdateCorrectiveActions, BulkUpdateCorrectiveActionsResult>
{
    private readonly AppDbContext _context;
    private readonly IAuditUserContext _userContext;
    private readonly IProcessLogService _log;

    public BulkUpdateCorrectiveActionsHandler(
        AppDbContext context,
        IAuditUserContext userContext,
        IProcessLogService log)
    {
        _context    = context;
        _userContext = userContext;
        _log        = log;
    }

    public async Task<BulkUpdateCorrectiveActionsResult> Handle(
        BulkUpdateCorrectiveActions request, CancellationToken cancellationToken)
    {
        if (!request.CorrectiveActionIds.Any())
            throw new ArgumentException("No corrective action IDs provided.");

        if (request.CorrectiveActionIds.Count > 500)
            throw new ArgumentException("Bulk operations are limited to 500 corrective actions at a time.");

        if (request.Action is not ("status" or "reassign"))
            throw new ArgumentException($"Unknown bulk action '{request.Action}'. Expected 'status' or 'reassign'.");

        var now    = DateTime.UtcNow;
        var today  = DateOnly.FromDateTime(now);
        var result = new BulkUpdateCorrectiveActionsResult();

        // Load only CAs the user is allowed to see (include Division for RequireClosurePhoto check)
        var caQuery = _context.CorrectiveActions
            .Include(ca => ca.Audit).ThenInclude(a => a!.Division)
            .Where(ca => request.CorrectiveActionIds.Contains(ca.Id));

        if (!_userContext.IsGlobal && _userContext.AllowedDivisionIds is { Count: > 0 } allowed)
            caQuery = caQuery.Where(ca => ca.Audit != null && allowed.Contains(ca.Audit.DivisionId));

        var cas = await caQuery.ToListAsync(cancellationToken);

        // ── Pre-batch closure photo existence for bulk-close (avoids N+1) ────────
        // Only needed when the action is "status"→"Closed" and at least one CA is in a photo-required division
        var caIdsWithPhotos = new HashSet<int>();
        if (request.Action == "status" && request.NewStatus == "Closed")
        {
            var requirePhotoCaIds = cas
                .Where(ca => ca.Audit?.Division?.RequireClosurePhoto == true)
                .Select(ca => ca.Id)
                .ToList();

            if (requirePhotoCaIds.Count > 0)
            {
                var idsWithPhotos = await _context.CorrectiveActionPhotos
                    .Where(p => requirePhotoCaIds.Contains(p.CorrectiveActionId))
                    .Select(p => p.CorrectiveActionId)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                caIdsWithPhotos = idsWithPhotos.ToHashSet();
            }
        }

        foreach (var ca in cas)
        {
            try
            {
                if (request.Action == "status")
                {
                    var newStatus = request.NewStatus
                        ?? throw new ArgumentException("NewStatus is required for action='status'.");

                    if (ca.Status is "Closed" or "Voided")
                    {
                        result.FailedIds.Add(ca.Id);
                        result.Errors.Add($"CA #{ca.Id}: cannot change status from '{ca.Status}'.");
                        continue;
                    }

                    if (newStatus == "Closed")
                    {
                        if (string.IsNullOrWhiteSpace(request.ClosureNotes))
                        {
                            result.FailedIds.Add(ca.Id);
                            result.Errors.Add($"CA #{ca.Id}: closure notes are required when closing.");
                            continue;
                        }
                        if (ca.Audit?.Division?.RequireClosurePhoto == true && !caIdsWithPhotos.Contains(ca.Id))
                        {
                            result.FailedIds.Add(ca.Id);
                            result.Errors.Add($"CA #{ca.Id}: a closure photo is required before closing.");
                            continue;
                        }
                        ca.Description  += $"\n\n[Resolved] {request.ClosureNotes}";
                        ca.Status        = "Closed";
                        ca.CompletedDate = today;
                        ca.ClosedDate    = now;
                    }
                    else if (newStatus == "Voided")
                    {
                        ca.Status = "Voided";
                    }
                    else if (newStatus == "InProgress")
                    {
                        ca.Status = "InProgress";
                    }
                    else
                    {
                        result.FailedIds.Add(ca.Id);
                        result.Errors.Add($"CA #{ca.Id}: unrecognised status '{newStatus}'.");
                        continue;
                    }
                }
                else // reassign
                {
                    if (ca.Status is "Closed" or "Voided")
                    {
                        result.FailedIds.Add(ca.Id);
                        result.Errors.Add($"CA #{ca.Id}: cannot reassign a closed/voided action.");
                        continue;
                    }

                    ca.AssignedTo       = request.NewAssignee;
                    ca.AssignedToUserId = request.NewAssigneeUserId;
                }

                ca.UpdatedAt = now;
                ca.UpdatedBy = request.UpdatedBy;
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.FailedIds.Add(ca.Id);
                result.Errors.Add($"CA #{ca.Id}: unexpected error — {ex.Message}");
            }
        }

        // CAs requested but not found (outside scope or don't exist)
        var foundIds  = cas.Select(c => c.Id).ToHashSet();
        var missingIds = request.CorrectiveActionIds.Where(id => !foundIds.Contains(id)).ToList();
        foreach (var id in missingIds)
        {
            result.FailedIds.Add(id);
            result.Errors.Add($"CA #{id}: not found or access denied.");
        }

        if (result.SuccessCount > 0)
            await _context.SaveChangesAsync(cancellationToken);

        return result;
    }
}
