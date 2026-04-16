using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Stronghold.AppDashboard.Api.Services;

public interface IAuditSummaryService
{
    Task<string?> GenerateSummaryAsync(AuditSummaryInput input, CancellationToken ct = default);
}

public class AuditSummaryInput
{
    public string DivisionCode { get; set; } = null!;
    public double? Score { get; set; }
    public double? PriorScore { get; set; }
    public List<(string Section, string QuestionText)> NcItems { get; set; } = new();
    public List<(string Section, string QuestionText)> WarningItems { get; set; } = new();
    public int CorrectedOnSiteCount { get; set; }
    public int TotalNcCount { get; set; }
}

/// <summary>
/// Calls the Claude API to generate a 150-200 word plain-language compliance audit summary.
/// Always fails gracefully — null returned when AI is disabled or the API call fails.
/// </summary>
public class AuditSummaryService : IAuditSummaryService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<AuditSummaryService> _logger;

    public AuditSummaryService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<AuditSummaryService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task<string?> GenerateSummaryAsync(AuditSummaryInput input, CancellationToken ct = default)
    {
        try
        {
            var aiEnabled = _config.GetValue<bool>("Ai:Enabled", true);
            if (!aiEnabled) return null;

            var apiKey = _config["Anthropic:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey)) return null;

            var ncList = input.NcItems.Any()
                ? string.Join("; ", input.NcItems.Take(10).Select(x => $"{x.Section}: {x.QuestionText}"))
                : "none";

            var warnList = input.WarningItems.Any()
                ? string.Join("; ", input.WarningItems.Take(5).Select(x => $"{x.Section}: {x.QuestionText}"))
                : "none";

            var scoreLine   = input.Score.HasValue      ? $"{input.Score:F1}%"      : "N/A";
            var priorLine   = input.PriorScore.HasValue ? $"{input.PriorScore:F1}%" : "no prior data";
            var trendPhrase = (input.Score.HasValue && input.PriorScore.HasValue)
                ? input.Score > input.PriorScore ? " (an improvement from prior)"
                : input.Score < input.PriorScore ? " (a decline from prior)"
                : " (unchanged from prior)"
                : string.Empty;

            var prompt =
                $"You are writing a compliance audit executive summary for a safety audit system. " +
                $"Division: {input.DivisionCode} | Score: {scoreLine}{trendPhrase} | Prior score: {priorLine} | " +
                $"Non-conforming items ({input.TotalNcCount} total, {input.CorrectedOnSiteCount} corrected on-site): {ncList} | " +
                $"Warning items: {warnList} | " +
                "Write a 150–200 word professional plain-language summary. Cover: overall score trend, the most significant non-conformances, " +
                "which items were corrected on-site, and the highest-priority corrective actions needed. " +
                "Do not use bullet points or section headers. Use a professional safety management tone.";

            var requestBody = new
            {
                model    = "claude-haiku-4-5-20251001",
                max_tokens = 400,
                messages = new[] { new { role = "user", content = prompt } }
            };

            var client = _httpClientFactory.CreateClient("Anthropic");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };

            using var response = await client.SendAsync(httpRequest, ct);
            var json = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Anthropic API returned {Status} for audit summary.", response.StatusCode);
                return null;
            }

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString()
                ?.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI audit summary generation failed — continuing without summary.");
            return null;
        }
    }
}
