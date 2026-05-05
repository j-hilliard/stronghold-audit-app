using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Domain.Audit.Reports;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;

namespace Stronghold.AppDashboard.Api.BackgroundServices;

/// <summary>
/// Runs every 5 minutes, fires any ScheduledReport whose NextRunAt is in the past.
/// Generates the PDF and emails it to the recipient list.
/// </summary>
public class ScheduledReportService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduledReportService> _logger;
    private readonly IConfiguration _config;

    public ScheduledReportService(
        IServiceScopeFactory scopeFactory,
        ILogger<ScheduledReportService> logger,
        IConfiguration config)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Gate on ScheduledReports:Enabled — defaults to true (production behavior preserved).
        // Set ScheduledReports:Enabled=false in appsettings.Local.json to prevent unwanted
        // PDF generation and email sends during local development.
        if (!_config.GetValue<bool>("ScheduledReports:Enabled", true))
        {
            _logger.LogInformation("ScheduledReportService: disabled via ScheduledReports:Enabled=false. No scheduled reports will run.");
            return;
        }

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                await RunDueReportsAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown — host is stopping
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ScheduledReportService: error in run cycle");
        }
    }

    private async Task RunDueReportsAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db       = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var email    = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var due = await db.ScheduledReports
            .Where(r => r.IsActive && r.NextRunAt <= DateTime.UtcNow)
            .ToListAsync(ct);

        foreach (var report in due)
        {
            try
            {
                _logger.LogInformation("ScheduledReportService: generating {Title}", report.Title);

                var pdfBytes = await mediator.Send(new GenerateReport
                {
                    Payload = new GenerateReportRequest
                    {
                        TemplateId   = report.TemplateId,
                        DivisionId   = report.DivisionId,
                        DateFrom     = ResolveDateFrom(report.DateRangePreset),
                        DateTo       = DateTime.UtcNow,
                        Title        = report.Title,
                        PrimaryColor = report.PrimaryColor,
                    },
                }, ct);

                var recipients = System.Text.Json.JsonSerializer.Deserialize<List<string>>(report.RecipientsJson)
                                 ?? new List<string>();

                var sizeKb = pdfBytes.Length / 1024;
                if (recipients.Count > 0)
                {
                    var body = $@"<p>Your scheduled report <strong>{System.Net.WebUtility.HtmlEncode(report.Title)}</strong>
has been generated ({sizeKb} KB) and is attached to this email.</p>
<p><em>Generated: {DateTime.UtcNow:MMMM d, yyyy 'at' HH:mm} UTC</em></p>";

                    var tempPath = Path.GetTempFileName() + ".pdf";
                    try
                    {
                        await File.WriteAllBytesAsync(tempPath, pdfBytes, ct);
                        var fileName = $"{SanitizeFileName(report.Title)}-{DateTime.UtcNow:yyyy-MM-dd}.pdf";

                        await email.SendAsync(
                            subject: $"{report.Title} — {DateTime.UtcNow:MMMM d, yyyy}",
                            htmlBody: body,
                            recipients: recipients,
                            attachments: new[] { (fileName, tempPath) },
                            ct: ct
                        );
                    }
                    finally
                    {
                        if (File.Exists(tempPath)) File.Delete(tempPath);
                    }
                    _logger.LogInformation("ScheduledReportService: sent PDF for {Title} to {Count} recipient(s)", report.Title, recipients.Count);
                }
                else
                {
                    _logger.LogInformation("ScheduledReportService: generated {Title} ({Size} KB), no recipients configured", report.Title, sizeKb);
                }

                report.LastRunAt = DateTime.UtcNow;
                report.NextRunAt = SaveScheduledReportHandler.ComputeNextRun(
                    report.Frequency, report.TimeUtc, report.DayOfWeek, report.DayOfMonth);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ScheduledReportService: failed to process report {Id}", report.Id);
            }
        }

        if (due.Count > 0)
            await db.SaveChangesAsync(ct);
    }

    private static DateTime? ResolveDateFrom(string? preset)
    {
        var now = DateTime.UtcNow;
        return preset switch
        {
            "last30days"  => now.AddDays(-30),
            "thisquarter" => StartOfCurrentQuarter(now),
            "lastquarter" => StartOfLastQuarter(now),
            "thisyear"    => new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            "lastyear"    => new DateTime(now.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            _             => null,
        };
    }

    private static DateTime StartOfCurrentQuarter(DateTime d)
    {
        var m = ((d.Month - 1) / 3) * 3 + 1;
        return new DateTime(d.Year, m, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    private static DateTime StartOfLastQuarter(DateTime d)
    {
        var start = StartOfCurrentQuarter(d).AddMonths(-3);
        return start;
    }

    private static string SanitizeFileName(string title) =>
        string.Concat(title.Select(c => char.IsLetterOrDigit(c) || c == '-' || c == ' ' ? c : '_'))
              .Replace(' ', '-')
              .ToLowerInvariant();
}
