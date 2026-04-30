using MediatR;
using Microsoft.Extensions.Configuration;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Newsletter;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin)]
public class GenerateNewsletterSummary : IRequest<NewsletterAiSummaryResult>
{
    public string DivisionCode { get; set; } = null!;
    public int Quarter { get; set; }
    public int Year { get; set; }
    public double? AvgScore { get; set; }
    public int TotalAudits { get; set; }
    public int TotalNcs { get; set; }
    public List<SectionNcItem> TopSections { get; set; } = new();
    public int OpenCaCount { get; set; }
    public int OverdueCaCount { get; set; }
}

public class SectionNcItem
{
    public string SectionName { get; set; } = null!;
    public int NcCount { get; set; }
}

public class GenerateNewsletterSummaryHandler
    : IRequestHandler<GenerateNewsletterSummary, NewsletterAiSummaryResult>
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;

    public GenerateNewsletterSummaryHandler(
        IHttpClientFactory httpClientFactory,
        IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _config = config;
    }

    public async Task<NewsletterAiSummaryResult> Handle(
        GenerateNewsletterSummary request,
        CancellationToken cancellationToken)
    {
        try
        {
            var apiKey = _config["Anthropic:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return new NewsletterAiSummaryResult { Success = false, Text = string.Empty };

            var topSectionLines = request.TopSections.Any()
                ? string.Join(", ", request.TopSections.Select(s => $"{s.SectionName} ({s.NcCount} NCs)"))
                : "no materially elevated sections";

            var prompt =
                $"You are writing a compliance audit executive summary for {request.DivisionCode} Q{request.Quarter} {request.Year}. "
                + $"Key metrics: {request.AvgScore ?? 0:F1}% average conformance, {request.TotalAudits} audits conducted, {request.TotalNcs} non-conformances recorded. "
                + $"Top findings by section: {topSectionLines}. "
                + $"{request.OpenCaCount} corrective actions remain open, {request.OverdueCaCount} are past the 14-day resolution deadline. "
                + "Write 3-4 professional sentences summarizing overall performance, the top risk areas requiring immediate attention, and any notable improvements or concerns vs prior period. "
                + "Be concise and direct. Do not use bullet points.";

            var body = new
            {
                model = "claude-sonnet-4-6",
                max_tokens = 500,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };

            var client = _httpClientFactory.CreateClient("Anthropic");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post,
                "https://api.anthropic.com/v1/messages")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(body),
                    Encoding.UTF8,
                    "application/json")
            };

            using var response = await client.SendAsync(httpRequest, cancellationToken);
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
                return new NewsletterAiSummaryResult { Success = false, Text = string.Empty };

            using var doc = JsonDocument.Parse(json);
            var text = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString() ?? string.Empty;

            return new NewsletterAiSummaryResult { Success = true, Text = text };
        }
        catch
        {
            return new NewsletterAiSummaryResult { Success = false, Text = string.Empty };
        }
    }
}
