using System.Net;
using System.Net.Mail;

namespace Stronghold.AppDashboard.Api.Services;

public interface IEmailService
{
    Task SendAsync(string subject, string htmlBody, IEnumerable<string> recipients, CancellationToken ct = default);
    Task SendAsync(string subject, string htmlBody, IEnumerable<string> recipients, IEnumerable<(string FileName, string FilePath)> attachments, CancellationToken ct = default);
}

/// <summary>
/// Thin email abstraction. Reads config from Email:* section.
/// When Email:DryRun = true (the default for local/dev), logs the email
/// instead of sending — the app never fails because email is not configured.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, IWebHostEnvironment env, ILogger<EmailService> logger)
    {
        _config = config;
        _env = env;
        _logger = logger;
    }

    public Task SendAsync(string subject, string htmlBody, IEnumerable<string> recipients, IEnumerable<(string FileName, string FilePath)> attachments, CancellationToken ct = default)
        => SendCoreAsync(subject, htmlBody, recipients, attachments, ct);

    public Task SendAsync(string subject, string htmlBody, IEnumerable<string> recipients, CancellationToken ct = default)
        => SendCoreAsync(subject, htmlBody, recipients, Enumerable.Empty<(string, string)>(), ct);

    private async Task SendCoreAsync(string subject, string htmlBody, IEnumerable<string> recipients, IEnumerable<(string FileName, string FilePath)> attachments, CancellationToken ct)
    {
        var to = recipients.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
        if (to.Count == 0) return;

        // When Email:DevRedirectAddress is set, override the env dry-run lock so live
        // SMTP delivery is enabled but ALL recipients are replaced with the safe inbox.
        // This lets the developer receive real emails without spamming actual users.
        var devRedirect  = _config.GetValue<string>("Email:DevRedirectAddress");
        var hasDevRedirect = !string.IsNullOrWhiteSpace(devRedirect);

        var isTestEnv = _env.IsEnvironment("Local")
                     || _env.IsEnvironment("Test")
                     || _env.IsDevelopment();

        // Without a DevRedirectAddress, local/test environments always dry-run.
        var isDryRun = !hasDevRedirect && (isTestEnv || _config.GetValue<bool>("Email:DryRun", true));

        if (isDryRun)
        {
            _logger.LogInformation(
                "[EmailService DryRun] Subject: {Subject} | To: {To} | BodyLength: {Len}",
                subject, string.Join(", ", to), htmlBody.Length);
            return;
        }

        // In local/test envs with DevRedirectAddress: intercept all recipients.
        IList<string> effectiveTo;
        string effectiveSubject;
        if (hasDevRedirect && isTestEnv)
        {
            effectiveTo     = [devRedirect!];
            effectiveSubject = $"[DEV → {string.Join(", ", to)}] {subject}";
            _logger.LogInformation(
                "[EmailService] Dev redirect: real recipients [{Real}] → {Redirect}",
                string.Join(", ", to), devRedirect);
        }
        else
        {
            effectiveTo      = to;
            effectiveSubject = subject;
        }

        var host    = _config.GetValue<string>("Email:Host");
        var from    = _config.GetValue<string>("Email:From");

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            _logger.LogWarning(
                "[EmailService] SMTP not configured (Email:Host or Email:From is empty). " +
                "Set Email:Host, Email:From, Email:Username, Email:Password in appsettings.Local.json. " +
                "Would have sent '{Subject}' to: {To}", subject, string.Join(", ", effectiveTo));
            return;
        }

        var port    = _config.GetValue<int>("Email:Port", 587);
        var user    = _config.GetValue<string>("Email:Username");
        var pass    = _config.GetValue<string>("Email:Password");
        var enableSsl = _config.GetValue<bool>("Email:EnableSsl", true);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Credentials = (user != null && pass != null)
                ? new NetworkCredential(user, pass)
                : null,
        };

        using var message = new MailMessage
        {
            From       = new MailAddress(from),
            Subject    = effectiveSubject,
            Body       = htmlBody,
            IsBodyHtml = true,
        };

        foreach (var addr in effectiveTo)
            message.To.Add(addr);

        foreach (var (fileName, filePath) in attachments)
        {
            if (File.Exists(filePath))
                message.Attachments.Add(new Attachment(filePath) { Name = fileName });
        }

        await client.SendMailAsync(message, ct);
        _logger.LogInformation("[EmailService] Sent '{Subject}' to {Count} recipient(s).", effectiveSubject, effectiveTo.Count);
    }
}
