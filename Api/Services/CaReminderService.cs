using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;

namespace Stronghold.AppDashboard.Api.Services;

/// <summary>
/// Daily background service that sends CA reminder emails.
/// Rules:
///   - DueSoon:       DueDate = today + 3 days (daily)
///   - DueToday:      DueDate = today (daily)
///   - Overdue:       DueDate &lt; today (sent every day until CA is closed)
///   - Escalation15:  15+ days overdue — sent once per CA, all-time deduplicated
///   - Escalation30:  30+ days overdue — sent once per CA, all-time deduplicated
/// Deduplication via CaNotificationLog — one log entry per CA + type per calendar day
/// (escalation types are all-time deduplicated).
/// </summary>
public class CaReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CaReminderService> _logger;
    private readonly IConfiguration _config;

    public CaReminderService(
        IServiceScopeFactory scopeFactory,
        ILogger<CaReminderService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _config = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await WaitUntilMidnightAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await SendRemindersAsync(stoppingToken);
                await WaitUntilMidnightAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown — host is stopping
        }
    }

    private static async Task WaitUntilMidnightAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var nextMidnight = now.Date.AddDays(1);
        var delay = nextMidnight - now;
        // Clamp to 10 minutes max during dev so restarts don't wait all night
        await Task.Delay(delay < TimeSpan.FromMinutes(10) ? delay : delay, ct).ConfigureAwait(false);
    }

    private async Task SendRemindersAsync(CancellationToken ct)
    {
        _logger.LogInformation("CaReminderService: Starting reminder run.");
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var dueSoonDate = today.AddDays(3);
            var todayDt = today.ToDateTime(TimeOnly.MinValue);

            // Load open CAs with a DueDate
            var openCas = await db.CorrectiveActions
                .Include(ca => ca.Finding)
                    .ThenInclude(f => f.Audit)
                        .ThenInclude(a => a.Header)
                .Include(ca => ca.Finding)
                    .ThenInclude(f => f.Audit)
                        .ThenInclude(a => a.Division)
                .Where(ca => ca.Status != "Closed" && ca.DueDate.HasValue)
                .ToListAsync(ct);

            // Load today's existing log entries for daily notifications (DueSoon / DueToday / Overdue)
            var todayLogs = await db.CaNotificationLogs
                .Where(l => l.SentAt >= todayDt)
                .Select(l => new { l.CorrectiveActionId, l.NotificationType })
                .ToListAsync(ct);

            // Key: "{caId}:{notifType}" — one send per CA per type per day
            var alreadySent = todayLogs
                .Select(l => $"{l.CorrectiveActionId}:{l.NotificationType}")
                .ToHashSet();

            // Escalation types are sent at most once ever — check all-time logs
            var escalationLogs = await db.CaNotificationLogs
                .Where(l => l.NotificationType == "Escalation15" || l.NotificationType == "Escalation30")
                .Select(l => new { l.CorrectiveActionId, l.NotificationType })
                .ToListAsync(ct);

            var escalationSent = escalationLogs
                .Select(l => $"{l.CorrectiveActionId}:{l.NotificationType}")
                .ToHashSet();

            var appBaseUrl = _config.GetValue<string>("App:BaseUrl") ?? "http://localhost:7220";
            int sent = 0;

            foreach (var ca in openCas)
            {
                if (string.IsNullOrWhiteSpace(ca.AssignedTo)) continue;

                string? notifType = null;
                if (ca.DueDate == dueSoonDate) notifType = "DueSoon";
                else if (ca.DueDate == today) notifType = "DueToday";
                else if (ca.DueDate < today) notifType = "Overdue";

                if (notifType == null) continue;

                var key = $"{ca.Id}:{notifType}";
                if (!alreadySent.Contains(key))
                {
                    await SendCaReminderAsync(emailService, ca, notifType, appBaseUrl, ct);
                    db.CaNotificationLogs.Add(new CaNotificationLog
                    {
                        CorrectiveActionId = ca.Id,
                        NotificationType = notifType,
                        SentAt = DateTime.UtcNow,
                        Recipient = ca.AssignedTo,
                    });
                    sent++;
                }

                // One-time escalation milestones for overdue CAs
                if (notifType == "Overdue" && ca.DueDate.HasValue)
                {
                    var daysOverdue = today.DayNumber - ca.DueDate.Value.DayNumber;

                    if (daysOverdue >= 30)
                    {
                        var e30Key = $"{ca.Id}:Escalation30";
                        if (!escalationSent.Contains(e30Key))
                        {
                            await SendCaReminderAsync(emailService, ca, "Escalation30", appBaseUrl, ct);
                            db.CaNotificationLogs.Add(new CaNotificationLog
                            {
                                CorrectiveActionId = ca.Id,
                                NotificationType = "Escalation30",
                                SentAt = DateTime.UtcNow,
                                Recipient = ca.AssignedTo,
                            });
                            escalationSent.Add(e30Key);
                            sent++;
                        }
                    }
                    else if (daysOverdue >= 15)
                    {
                        var e15Key = $"{ca.Id}:Escalation15";
                        if (!escalationSent.Contains(e15Key))
                        {
                            await SendCaReminderAsync(emailService, ca, "Escalation15", appBaseUrl, ct);
                            db.CaNotificationLogs.Add(new CaNotificationLog
                            {
                                CorrectiveActionId = ca.Id,
                                NotificationType = "Escalation15",
                                SentAt = DateTime.UtcNow,
                                Recipient = ca.AssignedTo,
                            });
                            escalationSent.Add(e15Key);
                            sent++;
                        }
                    }
                }
            }

            if (sent > 0)
                await db.SaveChangesAsync(ct);

            _logger.LogInformation("CaReminderService: Sent {Count} reminder(s).", sent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CaReminderService: Error during reminder run.");
        }
    }

    private async Task SendCaReminderAsync(
        IEmailService emailService,
        CorrectiveAction ca,
        string notifType,
        string appBaseUrl,
        CancellationToken ct)
    {
        var finding = ca.Finding;
        var audit = finding?.Audit;
        var jobNumber = audit?.Header?.JobNumber ?? "—";
        var location = audit?.Header?.Location ?? "—";
        var division = audit?.Division?.Code ?? "—";
        var caLink = $"{appBaseUrl}/audits/{ca.AuditId}/review";

        var (label, urgencyColor) = notifType switch
        {
            "DueSoon"       => ("Due in 3 Days",    "#d97706"),
            "DueToday"      => ("Due Today",         "#dc2626"),
            "Overdue"       => ("OVERDUE",           "#dc2626"),
            "Escalation15"  => ("15-Day Escalation", "#b91c1c"),
            "Escalation30"  => ("30-Day Escalation", "#7f1d1d"),
            _               => ("Reminder",          "#1e3a5f"),
        };

        var subject = $"[Stronghold CA {label}] {ca.Description?[..Math.Min(60, ca.Description?.Length ?? 0)]}...";
        var daysOverdue = notifType == "Overdue" && ca.DueDate.HasValue
            ? (DateOnly.FromDateTime(DateTime.UtcNow).DayNumber - ca.DueDate.Value.DayNumber)
            : 0;

        var body = $"""
            <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;">
              <div style="background:{urgencyColor};padding:20px;border-radius:4px 4px 0 0;">
                <h2 style="color:#fff;margin:0;">Corrective Action {label}</h2>
              </div>
              <div style="border:1px solid #ddd;border-top:none;padding:20px;border-radius:0 0 4px 4px;">
                <table style="width:100%;border-collapse:collapse;">
                  <tr><td style="padding:6px 0;color:#666;width:140px;">CA Description</td><td style="padding:6px 0;font-weight:bold;">{ca.Description}</td></tr>
                  <tr><td style="padding:6px 0;color:#666;">Assigned To</td><td style="padding:6px 0;">{ca.AssignedTo}</td></tr>
                  <tr><td style="padding:6px 0;color:#666;">Due Date</td><td style="padding:6px 0;color:{urgencyColor};font-weight:bold;">{ca.DueDate:MM/dd/yyyy}{(daysOverdue > 0 ? $" ({daysOverdue} days overdue)" : "")}</td></tr>
                  <tr><td style="padding:6px 0;color:#666;">Division</td><td style="padding:6px 0;">{division}</td></tr>
                  <tr><td style="padding:6px 0;color:#666;">Job Number</td><td style="padding:6px 0;">{jobNumber}</td></tr>
                  <tr><td style="padding:6px 0;color:#666;">Location</td><td style="padding:6px 0;">{location}</td></tr>
                </table>
                <div style="margin-top:24px;">
                  <a href="{caLink}" style="background:#1e3a5f;color:#fff;padding:10px 20px;border-radius:4px;text-decoration:none;display:inline-block;">
                    View Corrective Action →
                  </a>
                </div>
                <p style="color:#888;font-size:12px;margin-top:24px;">
                  This is an automated reminder from the Stronghold Compliance Audit system.
                </p>
              </div>
            </div>
            """;

        await emailService.SendAsync(subject, body, new[] { ca.AssignedTo! }, ct);
    }
}
