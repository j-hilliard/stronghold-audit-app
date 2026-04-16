using System.Net;
using System.Net.Mail;

namespace Stronghold.AppDashboard.Api.Services;

public interface IEmailService
{
    Task SendAsync(string subject, string htmlBody, IEnumerable<string> recipients, CancellationToken ct = default);
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

    public async Task SendAsync(string subject, string htmlBody, IEnumerable<string> recipients, CancellationToken ct = default)
    {
        var to = recipients.Where(r => !string.IsNullOrWhiteSpace(r)).ToList();
        if (to.Count == 0) return;

        // Structural safety: Local and any Test environments NEVER send real emails,
        // regardless of config. Only Production + explicit DryRun=false sends.
        var isTestEnv = _env.IsEnvironment("Local")
                     || _env.IsEnvironment("Test")
                     || _env.IsDevelopment();
        var isDryRun = isTestEnv || _config.GetValue<bool>("Email:DryRun", true);

        if (isDryRun)
        {
            _logger.LogInformation(
                "[EmailService DryRun] Subject: {Subject} | To: {To} | BodyLength: {Len}",
                subject, string.Join(", ", to), htmlBody.Length);
            return;
        }

        var host    = _config.GetValue<string>("Email:Host")     ?? throw new InvalidOperationException("Email:Host not configured.");
        var port    = _config.GetValue<int>("Email:Port", 587);
        var from    = _config.GetValue<string>("Email:From")     ?? throw new InvalidOperationException("Email:From not configured.");
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
            From = new MailAddress(from),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };

        foreach (var addr in to)
            message.To.Add(addr);

        await client.SendMailAsync(message, ct);
        _logger.LogInformation("[EmailService] Sent '{Subject}' to {Count} recipient(s).", subject, to.Count);
    }
}
