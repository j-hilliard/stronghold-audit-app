using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;
using System.Security.Cryptography;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.Auditor, AuthorizationRole.AuditAdmin)]
public class AssignCorrectiveAction : IRequest<int>
{
    public AssignCorrectiveActionRequest Payload { get; set; } = null!;
    public string AssignedBy { get; set; } = null!;
}

public class AssignCorrectiveActionHandler : IRequestHandler<AssignCorrectiveAction, int>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;
    private readonly IEmailService _email;
    private readonly IConfiguration _config;

    public AssignCorrectiveActionHandler(AppDbContext context, IProcessLogService log, IEmailService email, IConfiguration config)
    {
        _context = context;
        _log     = log;
        _email   = email;
        _config  = config;
    }

    public async Task<int> Handle(AssignCorrectiveAction request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Payload.AssignedToEmail))
            throw new ArgumentException("Assignee email is required to assign a corrective action.");

        var finding = await _context.AuditFindings
            .Include(f => f.Audit).ThenInclude(a => a!.Header)
            .Include(f => f.Audit).ThenInclude(a => a!.Division)
            .FirstOrDefaultAsync(f => f.Id == request.Payload.FindingId, cancellationToken)
            ?? throw new ArgumentException($"Finding {request.Payload.FindingId} not found.");

        // Determine priority — validate against allowed values
        var priority = request.Payload.Priority is "Urgent" or "Normal"
            ? request.Payload.Priority
            : "Normal";

        // Compute due date: explicit value wins; otherwise derive from division SLA config
        DateOnly? dueDate;
        if (!string.IsNullOrWhiteSpace(request.Payload.DueDate) &&
            DateOnly.TryParse(request.Payload.DueDate, out var parsed))
        {
            dueDate = parsed;
        }
        else
        {
            var division = finding.Audit?.Division;
            var slaDays = priority == "Urgent"
                ? (division?.SlaUrgentDays ?? 7)
                : (division?.SlaNormalDays ?? 14);
            dueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(slaDays));
        }

        var ca = new CorrectiveAction
        {
            FindingId = finding.Id,
            AuditId = finding.AuditId,
            Description = request.Payload.Description,
            RootCause = request.Payload.RootCause,
            AssignedTo = request.Payload.AssignedTo,
            DueDate = dueDate,
            Priority = priority,
            Status = "Open",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.AssignedBy,
        };

        _context.CorrectiveActions.Add(ca);
        await _context.SaveChangesAsync(cancellationToken);

        // ── Auto-create public access token for external assignee ─────────────
        var appBaseUrl = _config.GetValue<string>("App:BaseUrl") ?? "http://localhost:7220";
        var tokenValue = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
        var publicToken = new CaPublicToken
        {
            CorrectiveActionId = ca.Id,
            Token              = tokenValue,
            SentToName         = request.Payload.AssignedTo,
            SentToEmail        = request.Payload.AssignedToEmail.Trim(),
            CreatedBy          = request.AssignedBy,
            CreatedAt          = DateTime.UtcNow,
            ExpiresAt          = DateTime.UtcNow.AddDays(90),
        };
        _context.CaPublicTokens.Add(publicToken);
        await _context.SaveChangesAsync(cancellationToken);

        var caPublicUrl = $"{appBaseUrl}/ca/{tokenValue}";

        await _log.LogAsync("AssignCA", "CorrectiveAction", "Info",
            $"CA assigned to '{ca.AssignedTo}' for finding {finding.Id} on audit {finding.AuditId} by {request.AssignedBy}.",
            relatedObject: ca.Id.ToString());

        // In-app notification with direct public link
        var notifyEmail = request.Payload.AssignedToEmail.Trim();
        try
        {
            _context.AppNotifications.Add(new AppNotification
            {
                RecipientEmail = notifyEmail,
                Type           = "CaAssigned",
                Title          = "Corrective Action Assigned",
                Body           = $"A corrective action has been assigned to you: {ca.Description?[..Math.Min(80, ca.Description?.Length ?? 0)]}. Due: {ca.DueDate?.ToString("MM/dd/yyyy") ?? "TBD"}.",
                EntityType     = "CorrectiveAction",
                EntityId       = ca.Id,
                LinkUrl        = $"/ca/{tokenValue}",
                CreatedAt      = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await _log.LogAsync("AssignCA_NotificationFailed", "CorrectiveAction", "Warning",
                $"Failed to create in-app notification for CA {ca.Id}: {ex.Message}",
                relatedObject: ca.Id.ToString());
        }

        // Email assignee — fire-and-forget so email failure never breaks CA creation
        _ = SendAssignmentEmailAsync(ca, finding, notifyEmail, caPublicUrl, assignedBy: request.AssignedBy, isReassign: false, cancellationToken);

        return ca.Id;
    }

    private async Task SendAssignmentEmailAsync(
        CorrectiveAction ca,
        AuditFinding finding,
        string recipientEmail,
        string caPublicUrl,
        string assignedBy,
        bool isReassign,
        CancellationToken ct)
    {
        try
        {
            var audit       = finding.Audit;
            var jobNumber   = audit?.Header?.JobNumber ?? "—";
            var location    = audit?.Header?.Location  ?? "—";
            var division    = audit?.Division?.Code    ?? "—";
            var dueDateStr  = ca.DueDate?.ToString("MM/dd/yyyy") ?? "None";
            var action      = isReassign ? "reassigned to you" : "assigned to you";
            var subject     = $"[Stronghold] Corrective Action {(isReassign ? "Reassigned" : "Assigned")}: {ca.Description?[..Math.Min(60, ca.Description?.Length ?? 0)]}";

            var body = $"""
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;">
                  <div style="background:#1e3a5f;padding:20px;border-radius:4px 4px 0 0;">
                    <h2 style="color:#fff;margin:0;">Corrective Action {(isReassign ? "Reassigned" : "Assigned")} to You</h2>
                  </div>
                  <div style="border:1px solid #ddd;border-top:none;padding:20px;border-radius:0 0 4px 4px;">
                    <p style="color:#444;">A corrective action has been {action} by <strong>{assignedBy}</strong>.</p>
                    <table style="width:100%;border-collapse:collapse;">
                      <tr><td style="padding:6px 0;color:#666;width:140px;">Description</td><td style="padding:6px 0;font-weight:bold;">{ca.Description}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Assigned To</td><td style="padding:6px 0;">{ca.AssignedTo ?? recipientEmail}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Due Date</td><td style="padding:6px 0;color:#d97706;font-weight:bold;">{dueDateStr}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Division</td><td style="padding:6px 0;">{division}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Job Number</td><td style="padding:6px 0;">{jobNumber}</td></tr>
                      <tr><td style="padding:6px 0;color:#666;">Location</td><td style="padding:6px 0;">{location}</td></tr>
                    </table>
                    <div style="margin-top:24px;">
                      <a href="{caPublicUrl}" style="background:#1e3a5f;color:#fff;padding:10px 20px;border-radius:4px;text-decoration:none;display:inline-block;">
                        Update Your Corrective Action →
                      </a>
                    </div>
                    <p style="color:#64748b;font-size:12px;margin-top:12px;">
                      Use the button above to mark your progress or declare work complete. No account required — this link is unique to you.
                    </p>
                    <p style="color:#888;font-size:12px;margin-top:12px;">
                      This is an automated notification from the Stronghold Compliance Audit system.
                    </p>
                  </div>
                </div>
                """;

            await _email.SendAsync(subject, body, [recipientEmail], ct);
        }
        catch (Exception ex)
        {
            await _log.LogAsync("AssignCA_EmailFailed", "CorrectiveAction", "Warning",
                $"Failed to send assignment email for CA {ca.Id}: {ex.Message}",
                relatedObject: ca.Id.ToString());
        }
    }
}
