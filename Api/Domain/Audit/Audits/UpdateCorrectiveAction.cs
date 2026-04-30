using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.Auditor, AuthorizationRole.AuditAdmin, AuthorizationRole.NormalUser)]
public class UpdateCorrectiveAction : IRequest
{
    public int     CorrectiveActionId { get; set; }
    public string? Description       { get; set; }
    public string? AssignedTo        { get; set; }
    public int?    AssignedToUserId  { get; set; }
    public string? Priority          { get; set; }  // "Normal" | "Urgent" — null = no change
    public string? DueDate           { get; set; }  // ISO "YYYY-MM-DD" or null to clear
    public string? RootCause         { get; set; }
    public string  UpdatedBy         { get; set; } = null!;
}

public class UpdateCorrectiveActionHandler : IRequestHandler<UpdateCorrectiveAction>
{
    private readonly AppDbContext _context;
    private readonly IAuditUserContext _userContext;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;
    private readonly ILogger<UpdateCorrectiveActionHandler> _logger;

    public UpdateCorrectiveActionHandler(
        AppDbContext context,
        IAuditUserContext userContext,
        IEmailService email,
        IConfiguration config,
        ILogger<UpdateCorrectiveActionHandler> logger)
    {
        _context     = context;
        _userContext = userContext;
        _email       = email;
        _config      = config;
        _logger      = logger;
    }

    public async Task Handle(UpdateCorrectiveAction request, CancellationToken cancellationToken)
    {
        var ca = await _context.CorrectiveActions
            .Include(c => c.Audit).ThenInclude(a => a!.Header)
            .Include(c => c.Audit).ThenInclude(a => a!.Division)
            .FirstOrDefaultAsync(c => c.Id == request.CorrectiveActionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Corrective action #{request.CorrectiveActionId} not found.");

        // Division scope enforcement (prevents IDOR across division boundaries)
        if (!_userContext.IsGlobal
            && _userContext.AllowedDivisionIds is { Count: > 0 } allowed
            && ca.Audit != null
            && !allowed.Contains(ca.Audit.DivisionId))
            throw new UnauthorizedAccessException("You do not have access to this corrective action.");

        if (ca.Status is "Closed" or "Voided")
            throw new InvalidOperationException($"Cannot edit a corrective action with status '{ca.Status}'.");

        if (request.Description is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Description))
                throw new ArgumentException("Description cannot be empty.");
            ca.Description = request.Description.Trim();
        }

        // Track previous assignee to detect a reassignment
        var previousAssignedTo = ca.AssignedTo;
        string? newAssignee = null;

        if (request.AssignedTo is not null)
        {
            newAssignee = string.IsNullOrWhiteSpace(request.AssignedTo) ? null : request.AssignedTo.Trim();
            ca.AssignedTo       = newAssignee;
            ca.AssignedToUserId = request.AssignedToUserId;
        }

        if (request.Priority is not null)
        {
            if (request.Priority is not ("Normal" or "Urgent"))
                throw new ArgumentException("Priority must be 'Normal' or 'Urgent'.");
            ca.Priority = request.Priority;
        }

        if (request.DueDate is not null)
        {
            ca.DueDate = string.IsNullOrEmpty(request.DueDate)
                ? null
                : DateOnly.Parse(request.DueDate);
        }

        if (!string.IsNullOrWhiteSpace(request.RootCause))
            ca.RootCause = request.RootCause;

        ca.UpdatedAt = DateTime.UtcNow;
        ca.UpdatedBy = request.UpdatedBy;

        await _context.SaveChangesAsync(cancellationToken);

        // Email new assignee when reassigned to someone different
        var reassigned = newAssignee is not null
            && !string.Equals(newAssignee, previousAssignedTo, StringComparison.OrdinalIgnoreCase);

        if (reassigned)
            _ = SendReassignmentEmailAsync(ca, updatedBy: request.UpdatedBy, cancellationToken);
    }

    private async Task SendReassignmentEmailAsync(CorrectiveAction ca, string updatedBy, CancellationToken ct)
    {
        try
        {
            var jobNumber  = ca.Audit?.Header?.JobNumber ?? "—";
            var location   = ca.Audit?.Header?.Location  ?? "—";
            var division   = ca.Audit?.Division?.Code    ?? "—";
            var appBaseUrl = _config.GetValue<string>("App:BaseUrl") ?? "http://localhost:7220";
            var caLink     = $"{appBaseUrl}/audit-management/corrective-actions";
            var dueDateStr = ca.DueDate?.ToString("MM/dd/yyyy") ?? "None";
            var subject    = $"[Stronghold] Corrective Action Reassigned: {ca.Description?[..Math.Min(60, ca.Description?.Length ?? 0)]}";

            var body = $"""
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;">
                  <div style="background:#1e3a5f;padding:20px;border-radius:4px 4px 0 0;">
                    <h2 style="color:#fff;margin:0;">Corrective Action Reassigned to You</h2>
                  </div>
                  <div style="border:1px solid #ddd;border-top:none;padding:20px;border-radius:0 0 4px 4px;">
                    <p style="color:#444;">A corrective action has been reassigned to you by <strong>{updatedBy}</strong>.</p>
                    <table style="width:100%;border-collapse:collapse;">
                      <tr><td style="padding:6px 0;color:#666;width:140px;">Description</td><td style="padding:6px 0;font-weight:bold;">{ca.Description}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Due Date</td><td style="padding:6px 0;color:#d97706;font-weight:bold;">{dueDateStr}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Division</td><td style="padding:6px 0;">{division}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Job Number</td><td style="padding:6px 0;">{jobNumber}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Location</td><td style="padding:6px 0;">{location}</td></tr>
                    </table>
                    <div style="margin-top:24px;">
                      <a href="{caLink}" style="background:#1e3a5f;color:#fff;padding:10px 20px;border-radius:4px;text-decoration:none;display:inline-block;">
                        View Corrective Actions →
                      </a>
                    </div>
                    <p style="color:#888;font-size:12px;margin-top:24px;">
                      This is an automated notification from the Stronghold Compliance Audit system.
                    </p>
                  </div>
                </div>
                """;

            await _email.SendAsync(subject, body, [ca.AssignedTo!], ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[UpdateCA] Failed to send reassignment email for CA {CaId}.", ca.Id);
        }
    }
}
