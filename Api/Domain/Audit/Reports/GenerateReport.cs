using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Domain.Audit.Audits;
using Stronghold.AppDashboard.Shared.Enumerations;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using System.Globalization;
using System.Text;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Reports;

public class GenerateReportRequest
{
    /// <summary>"annual-review" | "quarterly-summary" | "post-audit-summary" | "ncr-report" | "executive-dashboard" | "ca-aging"</summary>
    public string TemplateId { get; set; } = null!;
    public int? DivisionId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Title { get; set; }
    /// <summary>Hex color e.g. #1e3a5f — defaults to #1e3a5f.</summary>
    public string? PrimaryColor { get; set; }
}

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.ReadOnlyViewer, AuthorizationRole.ExecutiveViewer,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator)]
public class GenerateReport : IRequest<byte[]>
{
    public GenerateReportRequest Payload { get; set; } = null!;
}

public class GenerateReportHandler : IRequestHandler<GenerateReport, byte[]>
{
    private readonly AppDbContext _db;
    private readonly IPdfGeneratorService _pdf;
    private readonly IAuditUserContext _userContext;

    public GenerateReportHandler(AppDbContext db, IPdfGeneratorService pdf, IAuditUserContext userContext)
    {
        _db = db;
        _pdf = pdf;
        _userContext = userContext;
    }

    public async Task<byte[]> Handle(GenerateReport request, CancellationToken cancellationToken)
    {
        var p = request.Payload;
        var color = string.IsNullOrWhiteSpace(p.PrimaryColor) ? "#1e3a5f" : p.PrimaryColor;
        var landscape = p.TemplateId is "annual-review" or "executive-dashboard";

        // ── Fetch report data ────────────────────────────────────────────────────
        var query = _db.Audits
            .Include(a => a.Division)
            .Include(a => a.Header)
            .Include(a => a.Responses)
            .AsQueryable()
            .Where(a => a.Status == "Submitted" || a.Status == "Closed");

        if (!_userContext.IsGlobal && _userContext.AllowedDivisionIds is { Count: > 0 } allowed)
            query = query.Where(a => allowed.Contains(a.DivisionId));

        if (p.DivisionId.HasValue)
            query = query.Where(a => a.DivisionId == p.DivisionId.Value);

        if (p.DateFrom.HasValue)
            query = query.Where(a => (a.SubmittedAt ?? a.CreatedAt) >= p.DateFrom.Value);

        if (p.DateTo.HasValue)
            query = query.Where(a => (a.SubmittedAt ?? a.CreatedAt) <= p.DateTo.Value);

        var audits = await query.OrderByDescending(a => a.SubmittedAt ?? a.CreatedAt)
                                .ToListAsync(cancellationToken);

        var divisionName = p.DivisionId.HasValue
            ? (await _db.Divisions.FindAsync(new object[] { p.DivisionId.Value }, cancellationToken))?.Name ?? "All Divisions"
            : "All Divisions";

        var rows = audits.Select(a =>
        {
            double? score = GetAuditReportHandler.ComputeTwoLevelScore(a.Responses);
            return new AuditReportRowDto
            {
                Id = a.Id,
                DivisionCode = a.Division.Code,
                Status = a.Status,
                AuditDate = a.Header?.AuditDate?.ToString("yyyy-MM-dd"),
                Auditor = a.Header?.Auditor,
                JobNumber = a.Header?.JobNumber,
                Location = a.Header?.Location,
                ScorePercent = score,
                NonConformingCount = a.Responses.Count(r => r.Status == "NonConforming"),
                WarningCount = a.Responses.Count(r => r.Status == "Warning"),
            };
        }).ToList();

        var scoredRows = rows.Where(r => r.ScorePercent.HasValue).ToList();
        var avgScore = scoredRows.Any() ? Math.Round(scoredRows.Average(r => r.ScorePercent!.Value), 1) : (double?)null;
        var totalNc = rows.Sum(r => r.NonConformingCount);
        var allResponses = audits.SelectMany(a => a.Responses).ToList();

        var sectionBreakdown = allResponses
            .Where(r => r.Status == "NonConforming" && !string.IsNullOrWhiteSpace(r.SectionNameSnapshot))
            .GroupBy(r => r.SectionNameSnapshot!)
            .Select(g => new SectionNcBreakdownDto { SectionName = g.Key, NcCount = g.Count() })
            .OrderByDescending(x => x.NcCount)
            .ToList();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var auditIds = audits.Select(a => a.Id).ToList();
        var openCas = await _db.CorrectiveActions
            .Where(ca => ca.AuditId != null && auditIds.Contains(ca.AuditId.Value) && ca.Status != "Closed")
            .OrderBy(ca => ca.DueDate)
            .ToListAsync(cancellationToken);

        // Trend — weekly
        var trend = audits
            .GroupBy(a => { var d = a.SubmittedAt ?? a.CreatedAt; return $"{d.Year}-W{ISOWeek.GetWeekOfYear(d):D2}"; })
            .OrderBy(g => g.Key)
            .Select(g =>
            {
                var s = g.Select(a => GetAuditReportHandler.ComputeTwoLevelScore(a.Responses))
                          .Where(x => x.HasValue).Select(x => x!.Value).ToList();
                return (Week: g.Key, AvgScore: s.Any() ? (double?)Math.Round(s.Average(), 1) : null, Count: g.Count());
            }).ToList();

        // Division stats for multi-division views
        var divStats = rows
            .GroupBy(r => r.DivisionCode)
            .Select(g => new
            {
                Division = g.Key,
                AvgScore = g.Where(r => r.ScorePercent.HasValue).Select(r => r.ScorePercent!.Value).DefaultIfEmpty().Average(),
                Audits = g.Count(),
                TotalNc = g.Sum(r => r.NonConformingCount),
            })
            .OrderByDescending(x => x.AvgScore)
            .ToList();

        // ── Title ────────────────────────────────────────────────────────────────
        var periodLabel = BuildPeriodLabel(p.DateFrom, p.DateTo);
        var title = !string.IsNullOrWhiteSpace(p.Title)
            ? p.Title
            : BuildDefaultTitle(p.TemplateId, divisionName, periodLabel);

        // ── Render HTML ──────────────────────────────────────────────────────────
        var html = ReportHtmlBuilder.Build(p.TemplateId, new ReportData
        {
            Title = title,
            DivisionName = divisionName,
            PeriodLabel = periodLabel,
            PrimaryColor = color,
            Rows = rows,
            AvgScore = avgScore,
            TotalAudits = rows.Count,
            TotalNc = totalNc,
            TotalWarnings = rows.Sum(r => r.WarningCount),
            SectionBreakdown = sectionBreakdown,
            Trend = trend.Select(t => (t.Week, t.AvgScore, t.Count)).ToList(),
            OpenCas = openCas.Select(ca => new OpenCorrectiveActionSummaryDto
            {
                Id = ca.Id,
                AuditId = ca.AuditId ?? 0,
                Description = ca.Description,
                AssignedTo = ca.AssignedTo,
                DueDate = ca.DueDate?.ToString("yyyy-MM-dd"),
                Status = ca.Status,
                IsOverdue = ca.DueDate.HasValue && ca.DueDate.Value < today,
                DaysOpen = (int)(DateTime.UtcNow - ca.CreatedAt).TotalDays,
            }).ToList(),
            DivStats = divStats.Select(d => (d.Division, d.AvgScore, d.Audits, d.TotalNc)).ToList(),
        });

        return await _pdf.GeneratePdfAsync(html, landscape, cancellationToken);
    }

    private static string BuildPeriodLabel(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue)
            return $"{from.Value:MMMM d, yyyy} – {to.Value:MMMM d, yyyy}";
        if (from.HasValue)
            return $"From {from.Value:MMMM d, yyyy}";
        if (to.HasValue)
            return $"Through {to.Value:MMMM d, yyyy}";
        return $"Through {DateTime.UtcNow:MMMM d, yyyy}";
    }

    private static string BuildDefaultTitle(string templateId, string division, string period) => templateId switch
    {
        "annual-review"        => $"{division} — Annual Review",
        "quarterly-summary"    => $"{division} — Quarterly Summary",
        "post-audit-summary"   => $"{division} — Audit Summary",
        "ncr-report"           => $"{division} — Non-Conformance Report",
        "executive-dashboard"  => $"Executive Dashboard — {period}",
        "ca-aging"             => $"Corrective Action Aging Report",
        _                      => $"{division} — Compliance Report",
    };
}

// ── Data bag passed to the HTML builder ──────────────────────────────────────────

public class ReportData
{
    public string Title { get; set; } = null!;
    public string DivisionName { get; set; } = null!;
    public string PeriodLabel { get; set; } = null!;
    public string PrimaryColor { get; set; } = "#1e3a5f";
    public int TotalAudits { get; set; }
    public double? AvgScore { get; set; }
    public int TotalNc { get; set; }
    public int TotalWarnings { get; set; }
    public List<AuditReportRowDto> Rows { get; set; } = new();
    public List<SectionNcBreakdownDto> SectionBreakdown { get; set; } = new();
    public List<(string Week, double? AvgScore, int Count)> Trend { get; set; } = new();
    public List<OpenCorrectiveActionSummaryDto> OpenCas { get; set; } = new();
    public List<(string Division, double AvgScore, int Audits, int TotalNc)> DivStats { get; set; } = new();
}

// ── Pure HTML string builder — no Razor, no external renderer ────────────────────

public static class ReportHtmlBuilder
{
    private const string BaseStyle = @"
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { font-family: 'Segoe UI', Arial, sans-serif; font-size: 11px; color: #1a202c; background: #fff; }
        h1 { font-size: 22px; font-weight: 700; }
        h2 { font-size: 15px; font-weight: 600; margin-bottom: 8px; }
        h3 { font-size: 12px; font-weight: 600; margin-bottom: 6px; }
        .page { padding: 0.5in; min-height: 100vh; }
        .cover { display: flex; flex-direction: column; justify-content: flex-end; min-height: 9in; padding: 0.6in; }
        .kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; margin-bottom: 20px; }
        .kpi-card { border: 1px solid #e2e8f0; border-radius: 8px; padding: 12px; text-align: center; }
        .kpi-value { font-size: 26px; font-weight: 700; }
        .kpi-label { font-size: 10px; color: #718096; margin-top: 2px; }
        table { width: 100%; border-collapse: collapse; font-size: 10px; margin-bottom: 16px; }
        th { background: #f7fafc; font-weight: 600; padding: 6px 8px; text-align: left; border-bottom: 2px solid #e2e8f0; }
        td { padding: 5px 8px; border-bottom: 1px solid #edf2f7; }
        tr:nth-child(even) td { background: #f9fafb; }
        .badge { display: inline-block; padding: 1px 6px; border-radius: 4px; font-size: 9px; font-weight: 600; }
        .badge-red { background: #fed7d7; color: #c53030; }
        .badge-amber { background: #feebc8; color: #c05621; }
        .badge-green { background: #c6f6d5; color: #276749; }
        .badge-gray { background: #edf2f7; color: #4a5568; }
        .section-header { padding: 8px 12px; border-radius: 6px; margin: 20px 0 10px; color: #fff; font-size: 13px; font-weight: 600; }
        .chart-wrap { margin-bottom: 16px; }
        .meta { font-size: 9px; color: #718096; }
        .two-col { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
        .overdue { color: #c53030; font-weight: 600; }
        @media print { body { print-color-adjust: exact; -webkit-print-color-adjust: exact; } }
    ";

    public static string Build(string templateId, ReportData d)
    {
        var body = templateId switch
        {
            "annual-review"       => BuildAnnualReview(d),
            "quarterly-summary"   => BuildQuarterlySummary(d),
            "post-audit-summary"  => BuildPostAuditSummary(d),
            "ncr-report"          => BuildNcrReport(d),
            "executive-dashboard" => BuildExecutiveDashboard(d),
            "ca-aging"            => BuildCaAging(d),
            _                     => BuildQuarterlySummary(d),
        };

        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8""/>
<style>{BaseStyle}
.accent {{ color: {d.PrimaryColor}; }}
.section-header {{ background: {d.PrimaryColor}; }}
.kpi-card.accent-card {{ border-color: {d.PrimaryColor}; border-top: 3px solid {d.PrimaryColor}; }}
.cover-overlay {{ background: {d.PrimaryColor}; }}
</style>
</head>
<body>{body}</body>
</html>";
    }

    // ── Annual Review ─────────────────────────────────────────────────────────

    private static string BuildAnnualReview(ReportData d)
    {
        var sb = new StringBuilder();

        // Cover strip
        sb.Append($@"
<div style=""background:{d.PrimaryColor}; color:#fff; padding:32px 48px 28px;"">
  <div style=""font-size:10px; opacity:.7; letter-spacing:1px; text-transform:uppercase;"">Stronghold Companies</div>
  <h1 style=""font-size:28px; margin:6px 0 4px;"">{Enc(d.Title)}</h1>
  <div style=""font-size:12px; opacity:.8;"">{Enc(d.PeriodLabel)}</div>
</div>
<div class=""page"">
");

        // KPIs
        sb.Append(KpiGrid(d));

        // Trend chart
        if (d.Trend.Count >= 2)
        {
            var trendData = d.Trend
                .Select(t => (t.Week, t.AvgScore ?? 0d))
                .ToList<(string Label, double Value)>();
            sb.Append($@"<h2>Conformance Trend</h2>
<div class=""chart-wrap"">{SvgChartBuilder.LineChart(trendData, 700, 200)}</div>");
        }

        // Section breakdown
        if (d.SectionBreakdown.Any())
        {
            var barData = d.SectionBreakdown
                .Take(10)
                .Select(s => (s.SectionName.Length > 28 ? s.SectionName[..28] + "…" : s.SectionName, (double)s.NcCount))
                .ToList<(string Label, double Value)>();
            sb.Append($@"<h2>Non-Conformances by Section</h2>
<div class=""chart-wrap"">{SvgChartBuilder.BarChart(barData, 700, 200, colorHex: "#ef4444")}</div>");
        }

        // Division comparison (grouped bar) — only when multi-division
        if (d.DivStats.Count > 1)
        {
            var groups = d.DivStats
                .Take(8)
                .Select(s => (s.Division, (IReadOnlyList<double>)new[] { s.AvgScore, (double)s.TotalNc }.ToList()))
                .ToList<(string GroupLabel, IReadOnlyList<double> Values)>();
            sb.Append($@"<h2>Division Comparison</h2>
<div class=""chart-wrap"">{SvgChartBuilder.GroupedBarChart(groups, new[] { "Avg Score %", "Total NCs" }, width: 700, height: 240)}</div>");
        }

        // Section breakdown table
        sb.Append(SectionBreakdownTable(d));

        // Open CAs
        if (d.OpenCas.Any())
            sb.Append(CaTable(d.OpenCas, "Open Corrective Actions", d.PrimaryColor));

        // Audit detail table (last 20)
        sb.Append(AuditDetailTable(d.Rows.Take(30).ToList(), d.PrimaryColor));

        sb.Append("</div>");
        return sb.ToString();
    }

    // ── Quarterly Summary ─────────────────────────────────────────────────────

    private static string BuildQuarterlySummary(ReportData d)
    {
        var sb = new StringBuilder();
        sb.Append($@"
<div style=""background:{d.PrimaryColor}; color:#fff; padding:20px 40px 16px;"">
  <h1>{Enc(d.Title)}</h1>
  <div class=""meta"" style=""color:rgba(255,255,255,.75);margin-top:4px;"">{Enc(d.PeriodLabel)}</div>
</div>
<div class=""page"">
");
        sb.Append(KpiGrid(d));

        if (d.Trend.Count >= 2)
        {
            var trendData = d.Trend.Select(t => (t.Week, t.AvgScore ?? 0d)).ToList<(string Label, double Value)>();
            sb.Append($@"<h2>Weekly Conformance Trend</h2>
<div class=""chart-wrap"">{SvgChartBuilder.LineChart(trendData, 660, 180)}</div>");
        }

        sb.Append(SectionBreakdownTable(d));

        if (d.OpenCas.Any())
            sb.Append(CaTable(d.OpenCas, "Open Corrective Actions", d.PrimaryColor));

        sb.Append("</div>");
        return sb.ToString();
    }

    // ── Post-Audit Summary ────────────────────────────────────────────────────

    private static string BuildPostAuditSummary(ReportData d)
    {
        var sb = new StringBuilder();
        sb.Append($@"
<div style=""background:{d.PrimaryColor}; color:#fff; padding:20px 40px 16px;"">
  <h1>{Enc(d.Title)}</h1>
  <div class=""meta"" style=""color:rgba(255,255,255,.75);margin-top:4px;"">{Enc(d.PeriodLabel)}</div>
</div>
<div class=""page"">
");

        // Score banner
        var scoreColor = d.AvgScore >= 90 ? "#276749" : d.AvgScore >= 75 ? "#c05621" : "#c53030";
        var scoreLabel = d.AvgScore.HasValue ? $"{d.AvgScore:F1}%" : "N/A";
        sb.Append($@"
<div style=""display:flex;align-items:center;gap:24px;margin-bottom:20px;padding:16px;border:1px solid #e2e8f0;border-radius:8px;"">
  <div style=""font-size:48px;font-weight:700;color:{scoreColor};"">{scoreLabel}</div>
  <div>
    <div style=""font-size:14px;font-weight:600;"">Avg Conformance Score</div>
    <div class=""meta"">{d.TotalAudits} audit(s) · {d.TotalNc} NCs · {d.TotalWarnings} warnings</div>
  </div>
</div>
");

        sb.Append(SectionBreakdownTable(d));
        sb.Append(AuditDetailTable(d.Rows, d.PrimaryColor));
        sb.Append("</div>");
        return sb.ToString();
    }

    // ── NCR Report ────────────────────────────────────────────────────────────

    private static string BuildNcrReport(ReportData d)
    {
        var sb = new StringBuilder();
        sb.Append($@"
<div style=""background:{d.PrimaryColor}; color:#fff; padding:20px 40px 16px;"">
  <h1>{Enc(d.Title)}</h1>
  <div class=""meta"" style=""color:rgba(255,255,255,.75);margin-top:4px;"">{Enc(d.PeriodLabel)}</div>
</div>
<div class=""page"">
");

        sb.Append(SectionBreakdownTable(d));

        // NC detail table
        var ncRows = d.Rows.Where(r => r.NonConformingCount > 0).OrderByDescending(r => r.NonConformingCount).ToList();
        if (ncRows.Any())
            sb.Append(AuditDetailTable(ncRows, d.PrimaryColor));

        if (d.OpenCas.Any())
            sb.Append(CaTable(d.OpenCas, "Open Corrective Actions", d.PrimaryColor));

        sb.Append("</div>");
        return sb.ToString();
    }

    // ── Executive Dashboard ───────────────────────────────────────────────────

    private static string BuildExecutiveDashboard(ReportData d)
    {
        var sb = new StringBuilder();
        sb.Append($@"
<div style=""background:{d.PrimaryColor}; color:#fff; padding:20px 40px 16px;"">
  <h1>{Enc(d.Title)}</h1>
  <div class=""meta"" style=""color:rgba(255,255,255,.75);margin-top:4px;"">{Enc(d.PeriodLabel)}</div>
</div>
<div class=""page"">
");
        sb.Append(KpiGrid(d));

        if (d.DivStats.Count > 1)
        {
            var barData = d.DivStats
                .Select(s => (s.Division, s.AvgScore))
                .ToList<(string Label, double Value)>();
            sb.Append($@"<h2>Conformance by Division</h2>
<div class=""chart-wrap"">{SvgChartBuilder.BarChart(barData, 700, 200)}</div>");
        }

        // Division summary table
        if (d.DivStats.Any())
        {
            sb.Append($@"<div class=""section-header"">Division Summary</div>
<table><thead><tr><th>Division</th><th>Audits</th><th>Avg Score</th><th>Total NCs</th></tr></thead><tbody>");
            foreach (var s in d.DivStats)
            {
                var sc = s.AvgScore >= 90 ? "badge-green" : s.AvgScore >= 75 ? "badge-amber" : "badge-red";
                sb.Append($@"<tr><td><strong>{Enc(s.Division)}</strong></td><td>{s.Audits}</td>
<td><span class=""badge {sc}"">{s.AvgScore:F1}%</span></td><td>{s.TotalNc}</td></tr>");
            }
            sb.Append("</tbody></table>");
        }

        // Top risk sections
        if (d.SectionBreakdown.Any())
        {
            sb.Append($@"<div class=""section-header"">Top Non-Conformance Categories</div>
<table><thead><tr><th>Section</th><th>NC Count</th><th>NCs / Audit</th></tr></thead><tbody>");
            foreach (var s in d.SectionBreakdown.Take(5))
            {
                var rate = d.TotalAudits > 0 ? (double)s.NcCount / d.TotalAudits : 0;
                sb.Append($"<tr><td>{Enc(s.SectionName)}</td><td>{s.NcCount}</td><td>{rate:F2}</td></tr>");
            }
            sb.Append("</tbody></table>");
        }

        sb.Append("</div>");
        return sb.ToString();
    }

    // ── CA Aging Report ───────────────────────────────────────────────────────

    private static string BuildCaAging(ReportData d)
    {
        var sb = new StringBuilder();
        var overdue = d.OpenCas.Count(c => c.IsOverdue);
        sb.Append($@"
<div style=""background:{d.PrimaryColor}; color:#fff; padding:20px 40px 16px;"">
  <h1>{Enc(d.Title)}</h1>
  <div class=""meta"" style=""color:rgba(255,255,255,.75);margin-top:4px;"">{Enc(d.PeriodLabel)}</div>
</div>
<div class=""page"">
");

        sb.Append($@"
<div class=""kpi-grid"" style=""grid-template-columns:repeat(3,1fr);"">
  <div class=""kpi-card accent-card"">
    <div class=""kpi-value"">{d.OpenCas.Count}</div>
    <div class=""kpi-label"">Open CAs</div>
  </div>
  <div class=""kpi-card"" style=""border-top:3px solid #c53030;"">
    <div class=""kpi-value"" style=""color:#c53030;"">{overdue}</div>
    <div class=""kpi-label"">Past 14-Day Rule</div>
  </div>
  <div class=""kpi-card"" style=""border-top:3px solid #c05621;"">
    <div class=""kpi-value"" style=""color:#c05621;"">{(d.OpenCas.Any() ? $"{d.OpenCas.Average(c => c.DaysOpen):F0}d" : "—")}</div>
    <div class=""kpi-label"">Avg Age</div>
  </div>
</div>
");

        if (d.OpenCas.Any())
            sb.Append(CaTable(d.OpenCas, "All Open Corrective Actions", d.PrimaryColor));

        sb.Append("</div>");
        return sb.ToString();
    }

    // ── Shared fragments ──────────────────────────────────────────────────────

    private static string KpiGrid(ReportData d)
    {
        var scoreColor = d.AvgScore >= 90 ? "#276749" : d.AvgScore >= 75 ? "#c05621" : "#c53030";
        var scoreDisplay = d.AvgScore.HasValue ? $"{d.AvgScore:F1}%" : "—";
        var openOverdue = d.OpenCas.Count(c => c.IsOverdue);
        return $@"
<div class=""kpi-grid"">
  <div class=""kpi-card accent-card"">
    <div class=""kpi-value"">{d.TotalAudits}</div>
    <div class=""kpi-label"">Total Audits</div>
  </div>
  <div class=""kpi-card accent-card"">
    <div class=""kpi-value"" style=""color:{scoreColor};"">{scoreDisplay}</div>
    <div class=""kpi-label"">Avg Conformance</div>
  </div>
  <div class=""kpi-card"" style=""border-top:3px solid #c53030;"">
    <div class=""kpi-value"" style=""color:#c53030;"">{d.TotalNc}</div>
    <div class=""kpi-label"">Non-Conformances</div>
  </div>
  <div class=""kpi-card"" style=""border-top:3px solid #c05621;"">
    <div class=""kpi-value"" style=""color:#c05621;"">{openOverdue}</div>
    <div class=""kpi-label"">CAs Past Due</div>
  </div>
</div>
";
    }

    private static string SectionBreakdownTable(ReportData d)
    {
        if (!d.SectionBreakdown.Any()) return "";
        var sb = new StringBuilder();
        sb.Append($@"<div class=""section-header"">Findings by Section</div>
<table><thead><tr>
  <th>Section</th><th style=""width:80px;text-align:right;"">NC Count</th><th style=""width:100px;text-align:right;"">NCs / Audit</th>
</tr></thead><tbody>");
        foreach (var s in d.SectionBreakdown)
        {
            var rate = d.TotalAudits > 0 ? (double)s.NcCount / d.TotalAudits : 0;
            var badge = rate >= 0.5 ? "badge-red" : rate >= 0.2 ? "badge-amber" : "badge-green";
            sb.Append($@"<tr>
  <td>{Enc(s.SectionName)}</td>
  <td style=""text-align:right;"">{s.NcCount}</td>
  <td style=""text-align:right;""><span class=""badge {badge}"">{rate:F2}</span></td>
</tr>");
        }
        sb.Append("</tbody></table>");
        return sb.ToString();
    }

    private static string CaTable(List<OpenCorrectiveActionSummaryDto> cas, string heading, string color)
    {
        var sb = new StringBuilder();
        sb.Append($@"<div class=""section-header"">{Enc(heading)}</div>
<table><thead><tr>
  <th style=""width:45px;"">CA #</th>
  <th>Description</th>
  <th style=""width:120px;"">Assigned To</th>
  <th style=""width:90px;"">Due Date</th>
  <th style=""width:55px;"">Age</th>
  <th style=""width:80px;"">Status</th>
</tr></thead><tbody>");
        foreach (var ca in cas.OrderByDescending(c => c.DaysOpen).Take(50))
        {
            var overdueClass = ca.IsOverdue ? " overdue" : "";
            var badge = ca.IsOverdue ? "badge-red" : ca.Status == "InProgress" ? "badge-amber" : "badge-gray";
            sb.Append($@"<tr>
  <td class=""{overdueClass}"">{ca.Id}</td>
  <td style=""max-width:300px;"">{Enc(ca.Description.Length > 100 ? ca.Description[..100] + "…" : ca.Description)}</td>
  <td>{Enc(ca.AssignedTo ?? "—")}</td>
  <td class=""{overdueClass}"">{Enc(ca.DueDate ?? "—")}</td>
  <td class=""{overdueClass}"">{ca.DaysOpen}d</td>
  <td><span class=""badge {badge}"">{Enc(ca.IsOverdue ? "Overdue" : ca.Status)}</span></td>
</tr>");
        }
        sb.Append("</tbody></table>");
        return sb.ToString();
    }

    private static string AuditDetailTable(List<AuditReportRowDto> rows, string color)
    {
        if (!rows.Any()) return "";
        var sb = new StringBuilder();
        sb.Append($@"<div class=""section-header"">Audit Log</div>
<table><thead><tr>
  <th style=""width:40px;"">#</th>
  <th style=""width:55px;"">Division</th>
  <th style=""width:90px;"">Date</th>
  <th>Auditor</th>
  <th>Location</th>
  <th style=""width:75px;"">Score</th>
  <th style=""width:35px;"">NCs</th>
  <th style=""width:45px;"">Warns</th>
</tr></thead><tbody>");
        foreach (var r in rows)
        {
            var sc = r.ScorePercent >= 90 ? "badge-green" : r.ScorePercent >= 75 ? "badge-amber" : r.ScorePercent.HasValue ? "badge-red" : "badge-gray";
            sb.Append($@"<tr>
  <td>{r.Id}</td>
  <td>{Enc(r.DivisionCode)}</td>
  <td>{Enc(r.AuditDate ?? "—")}</td>
  <td>{Enc(r.Auditor ?? "—")}</td>
  <td>{Enc(r.Location ?? r.JobNumber ?? "—")}</td>
  <td><span class=""badge {sc}"">{(r.ScorePercent.HasValue ? $"{r.ScorePercent:F1}%" : "—")}</span></td>
  <td style=""text-align:center;{(r.NonConformingCount > 0 ? "color:#c53030;font-weight:600;" : "")}"">{r.NonConformingCount}</td>
  <td style=""text-align:center;{(r.WarningCount > 0 ? "color:#c05621;" : "")}"">{r.WarningCount}</td>
</tr>");
        }
        sb.Append("</tbody></table>");
        return sb.ToString();
    }

    private static string Enc(string? s) =>
        System.Security.SecurityElement.Escape(s ?? "") ?? "";
}
