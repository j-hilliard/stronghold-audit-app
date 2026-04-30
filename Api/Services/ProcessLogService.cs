namespace Stronghold.AppDashboard.Api.Services;

public interface IProcessLogService
{
    Task LogAsync(
        string processName,
        string processType,
        string logType,
        string message,
        Guid? incidentReportId = null,
        string? messageDetail = null,
        string? relatedObject = null);
}

public class ProcessLogService : IProcessLogService
{
    private readonly ILogger<ProcessLogService> _logger;
    private readonly string _runId;

    public ProcessLogService(ILogger<ProcessLogService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _runId = httpContextAccessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
    }

    public Task LogAsync(
        string processName,
        string processType,
        string logType,
        string message,
        Guid? incidentReportId = null,
        string? messageDetail = null,
        string? relatedObject = null)
    {
        _logger.LogInformation(
            "[{RunId}] [{LogType}] {ProcessName}/{ProcessType}: {Message} | Detail={MessageDetail} | Related={RelatedObject} | IncidentId={IncidentReportId}",
            _runId, logType, processName, processType, message, messageDetail, relatedObject, incidentReportId);
        return Task.CompletedTask;
    }
}
