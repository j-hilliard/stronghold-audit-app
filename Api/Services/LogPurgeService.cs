namespace Stronghold.AppDashboard.Api.Services;

public class LogPurgeService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LogPurgeService> _logger;
    private readonly IConfiguration _configuration;

    public LogPurgeService(
        IServiceScopeFactory scopeFactory,
        ILogger<LogPurgeService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await PurgeOldLogsAsync();
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown — host is stopping
        }
    }

    private Task PurgeOldLogsAsync()
    {
        return Task.CompletedTask;
    }
}
