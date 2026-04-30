using System.Globalization;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Domain.Audit.Audits;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Reports;

// ── DTOs matching the frontend StructuredReport / StructuredSection ──────────────

public class StructuredSectionPayload
{
    public string Type { get; set; } = null!;
    public bool Enabled { get; set; }
    public string? EditedText { get; set; }
    public string? EditedHighlights { get; set; }
    public string? EditedNotes { get; set; }
}

public class StructuredReportPayload
{
    public int SchemaVersion { get; set; }
    public string TemplateType { get; set; } = null!;
    public int DivisionId { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string Period { get; set; } = null!;
    public string? DateFrom { get; set; }
    public string? DateTo { get; set; }
    public List<StructuredSectionPayload> Sections { get; set; } = new();
}

public class GenerateStructuredReportRequest
{
    public string StructuredReportJson { get; set; } = null!;
    /// <summary>Hex color override, e.g. #1e3a5f.</summary>
    public string? PrimaryColor { get; set; }
}

// ── MediatR command ───────────────────────────────────────────────────────────────

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.ReadOnlyViewer, AuthorizationRole.ExecutiveViewer,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin, AuthorizationRole.Executive)]
public class GenerateStructuredReportCommand : IRequest<byte[]>
{
    public GenerateStructuredReportRequest Payload { get; set; } = null!;
}

// ── Handler ───────────────────────────────────────────────────────────────────────

public class GenerateStructuredReportHandler : IRequestHandler<GenerateStructuredReportCommand, byte[]>
{
    private readonly AppDbContext _db;
    private readonly IPdfGeneratorService _pdf;
    private readonly IAuditUserContext _userContext;

    public GenerateStructuredReportHandler(AppDbContext db, IPdfGeneratorService pdf, IAuditUserContext userContext)
    {
        _db = db;
        _pdf = pdf;
        _userContext = userContext;
    }

    public async Task<byte[]> Handle(GenerateStructuredReportCommand request, CancellationToken ct)
    {
        var p = request.Payload;
        var color = string.IsNullOrWhiteSpace(p.PrimaryColor) ? "#1e3a5f" : p.PrimaryColor;

        var report = JsonSerializer.Deserialize<StructuredReportPayload>(
            p.StructuredReportJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new ArgumentException("Invalid structured report JSON.");

        var dateFrom = TryParseDate(report.DateFrom);
        var dateTo   = TryParseDate(report.DateTo);

        // ── Fetch division audits (full data for all computations) ────────────────
        var query = _db.Audits
            .Include(a => a.Division)
            .Include(a => a.Header)
            .Include(a => a.Responses)
            .AsQueryable()
            .Where(a => a.Status == "Submitted" || a.Status == "Closed");

        if (!_userContext.IsGlobal && _userContext.AllowedDivisionIds is { Count: > 0 } allowed)
            query = query.Where(a => allowed.Contains(a.DivisionId));

        if (report.DivisionId > 0)
            query = query.Where(a => a.DivisionId == report.DivisionId);

        if (dateFrom.HasValue)
            query = query.Where(a => (a.SubmittedAt ?? a.CreatedAt) >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(a => (a.SubmittedAt ?? a.CreatedAt) <= dateTo.Value);

        var audits = await query.OrderByDescending(a => a.SubmittedAt ?? a.CreatedAt)
                                .ToListAsync(ct);

        var divisionName = report.DivisionId > 0
            ? (await _db.Divisions.FindAsync([report.DivisionId], ct))?.Name ?? report.DivisionCode
            : "All Divisions";

        var allResponses = audits.SelectMany(a => a.Responses).ToList();

        // KPI values
        var scoredAudits = audits
            .Select(a => GetAuditReportHandler.ComputeTwoLevelScore(a.Responses))
            .Where(s => s.HasValue).Select(s => s!.Value).ToList();
        var avgScore     = scoredAudits.Any() ? Math.Round(scoredAudits.Average(), 1) : (double?)null;
        var totalNc       = allResponses.Count(r => r.Status == "NonConforming");
        var totalWarnings = allResponses.Count(r => r.Status == "Warning");
        var correctedCt  = allResponses.Count(r => r.Status == "NonConforming" && r.CorrectedOnSite);
        var correctedPct = totalNc > 0 ? (int)Math.Round((double)correctedCt / totalNc * 100) : 0;

        var sectionBreakdown = allResponses
            .Where(r => r.Status == "NonConforming" && !string.IsNullOrWhiteSpace(r.SectionNameSnapshot))
            .GroupBy(r => r.SectionNameSnapshot!)
            .Select(g => new SectionNcBreakdownDto { SectionName = g.Key, NcCount = g.Count() })
            .OrderByDescending(x => x.NcCount)
            .ToList();

        var today    = DateOnly.FromDateTime(DateTime.UtcNow);
        var auditIds = audits.Select(a => a.Id).ToList();

        var openCas = await _db.CorrectiveActions
            .Where(ca => ca.AuditId != null && auditIds.Contains(ca.AuditId.Value) && ca.Status != "Closed")
            .OrderBy(ca => ca.DueDate)
            .ToListAsync(ct);

        var openCaRows = openCas.Select(ca => new OpenCorrectiveActionSummaryDto
        {
            Id          = ca.Id,
            AuditId     = ca.AuditId ?? 0,
            Description = ca.Description,
            AssignedTo  = ca.AssignedTo,
            DueDate     = ca.DueDate?.ToString("yyyy-MM-dd"),
            Status      = ca.Status,
            IsOverdue   = ca.DueDate.HasValue && ca.DueDate.Value < today,
            DaysOpen    = (int)(DateTime.UtcNow - ca.CreatedAt).TotalDays,
        }).ToList();

        // ── Quarterly trend (division) ─────────────────────────────────────────────
        var divQuarterMap = audits
            .GroupBy(a => { var d = a.SubmittedAt ?? a.CreatedAt; return $"{d.Year} Q{(d.Month - 1) / 3 + 1}"; })
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => g.Count() > 0
                    ? (double)g.Sum(a => a.Responses.Count(r => r.Status == "NonConforming")) / g.Count()
                    : 0d);

        // ── Quarterly trend (company-wide, lightweight) ───────────────────────────
        var coTrendRaw = await _db.Audits
            .Where(a => a.Status == "Submitted" || a.Status == "Closed")
            .Where(a => !dateFrom.HasValue || (a.SubmittedAt ?? a.CreatedAt) >= dateFrom.Value)
            .Where(a => !dateTo.HasValue   || (a.SubmittedAt ?? a.CreatedAt) <= dateTo.Value)
            .Select(a => new
            {
                Date    = a.SubmittedAt ?? a.CreatedAt,
                NcCount = a.Responses.Count(r => r.Status == "NonConforming"),
            })
            .ToListAsync(ct);

        var coQuarterMap = coTrendRaw
            .GroupBy(a => $"{a.Date.Year} Q{(a.Date.Month - 1) / 3 + 1}")
            .OrderBy(g => g.Key)
            .ToDictionary(
                g => g.Key,
                g => (double?)((double)g.Sum(a => a.NcCount) / g.Count()));

        var allQuarters = divQuarterMap.Keys.Union(coQuarterMap.Keys).OrderBy(k => k).ToList();

        // ── Build + render ─────────────────────────────────────────────────────────
        var html = StructuredReportHtmlBuilder.Build(report, new StructuredReportBuildData
        {
            DivisionName       = divisionName,
            Color              = color,
            TotalAudits        = audits.Count,
            AvgScore           = avgScore,
            TotalNc            = totalNc,
            TotalWarnings      = totalWarnings,
            CorrectedOnSitePct = correctedPct,
            SectionBreakdown   = sectionBreakdown,
            OpenCas            = openCaRows,
            QuarterLabels      = allQuarters,
            DivNcRates         = allQuarters.Select(q => divQuarterMap.TryGetValue(q, out var v) ? v : 0d).ToList(),
            CoNcRates          = allQuarters.Select(q => coQuarterMap.TryGetValue(q, out var v) ? v : null).ToList(),
        });

        return await _pdf.GeneratePdfAsync(html, landscape: false, ct);
    }

    private static DateTime? TryParseDate(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return DateTime.TryParse(s, null, DateTimeStyles.RoundtripKind, out var dt) ? dt : null;
    }
}

// ── Build-data bag ────────────────────────────────────────────────────────────────

public class StructuredReportBuildData
{
    public string DivisionName { get; set; } = null!;
    public string Color { get; set; } = "#1e3a5f";
    public int TotalAudits { get; set; }
    public double? AvgScore { get; set; }
    public int TotalNc { get; set; }
    public int TotalWarnings { get; set; }
    public int CorrectedOnSitePct { get; set; }
    public List<SectionNcBreakdownDto> SectionBreakdown { get; set; } = new();
    public List<OpenCorrectiveActionSummaryDto> OpenCas { get; set; } = new();
    public List<string> QuarterLabels { get; set; } = new();
    public List<double> DivNcRates { get; set; } = new();
    public List<double?> CoNcRates { get; set; } = new();
}

// ── HTML builder ──────────────────────────────────────────────────────────────────

public static class StructuredReportHtmlBuilder
{
    private const string BaseStyle = @"
* { box-sizing: border-box; margin: 0; padding: 0; }
body { font-family: 'Segoe UI', Arial, sans-serif; font-size: 11px; color: #1a202c; background: #fff; }
.page { padding: 0.45in 0.5in; }
.section-block { margin-bottom: 24px; page-break-inside: avoid; }
.section-hdr { padding: 7px 12px; border-radius: 5px; margin-bottom: 10px;
    color: #fff; font-size: 12px; font-weight: 600; letter-spacing: 0.3px; }
.kpi-grid { display: grid; grid-template-columns: repeat(3,1fr); gap: 10px; }
.kpi-card { border: 1px solid #e2e8f0; border-radius: 7px; padding: 12px; }
.kpi-val { font-size: 24px; font-weight: 700; line-height: 1.1; }
.kpi-lbl { font-size: 9.5px; color: #718096; margin-top: 3px; text-transform: uppercase; letter-spacing: 0.4px; }
.kpi-sub { font-size: 9px; color: #a0aec0; margin-top: 2px; }
table { width: 100%; border-collapse: collapse; font-size: 10px; }
th { background: #f7fafc; font-weight: 600; padding: 6px 8px; text-align: left;
    border-bottom: 2px solid #e2e8f0; font-size: 9.5px; }
td { padding: 5px 8px; border-bottom: 1px solid #edf2f7; vertical-align: top; }
.badge { display: inline-block; padding: 1px 6px; border-radius: 4px; font-size: 9px; font-weight: 600; }
.badge-red { background: #fed7d7; color: #c53030; }
.badge-amber { background: #feebc8; color: #c05621; }
.badge-green { background: #c6f6d5; color: #276749; }
.badge-gray { background: #edf2f7; color: #4a5568; }
.overdue { color: #c53030; font-weight: 600; }
.narrative { font-size: 11px; line-height: 1.6; color: #2d3748; background: #f7fafc;
    border-radius: 0 4px 4px 0; padding: 10px 14px; white-space: pre-wrap; }
.hl-list { list-style: none; padding: 0; }
.hl-list li { display: flex; align-items: flex-start; gap: 8px; margin-bottom: 7px;
    font-size: 11px; line-height: 1.5; }
.hl-chk { color: #38a169; font-weight: 700; flex-shrink: 0; }
.fc { margin-bottom: 14px; }
.fc-title { font-size: 9.5px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.4px;
    color: #4a5568; padding-bottom: 4px; border-bottom: 1px solid #e2e8f0; margin-bottom: 6px; }
.fc-text { font-size: 11px; line-height: 1.5; color: #2d3748; white-space: pre-wrap; }
.trend-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }
.trend-lbl { font-size: 9.5px; color: #718096; text-transform: uppercase; letter-spacing: 0.4px; margin-bottom: 6px; }
@media print { body { print-color-adjust: exact; -webkit-print-color-adjust: exact; } }";

    public static string Build(StructuredReportPayload report, StructuredReportBuildData data)
    {
        var body = new StringBuilder();
        body.Append(Cover(report, data));
        body.Append("<div class=\"page\">");
        foreach (var sec in report.Sections.Where(s => s.Enabled && s.Type != "cover"))
        {
            switch (sec.Type)
            {
                case "kpis":               body.Append(Kpis(data));                  break;
                case "trend":              body.Append(Trend(data));                 break;
                case "category-breakdown": body.Append(CategoryBreakdown(data));     break;
                case "findings-examples":  body.Append(FindingsExamples(sec, data)); break;
                case "ca-table":           body.Append(CaTable(data));               break;
                case "summary-text":       body.Append(SummaryText(sec, data));      break;
                case "highlights":         body.Append(Highlights(sec));             break;
            }
        }
        body.Append("</div>");

        return $@"<!DOCTYPE html>
<html lang=""en""><head><meta charset=""UTF-8""/>
<style>{BaseStyle}
.section-hdr {{ background: {data.Color}; }}
.narrative {{ border-left: 3px solid {data.Color}; }}
</style></head>
<body>{body}</body></html>";
    }

    private static string Cover(StructuredReportPayload report, StructuredReportBuildData data)
    {
        var label = report.TemplateType switch
        {
            "QuarterlySummary"   => "Quarterly Summary",
            "Newsletter"         => "Division Newsletter",
            "AuditSummary"       => "Audit Summary",
            "ExecutiveDashboard" => "Executive Dashboard",
            _                    => "Compliance Report",
        };
        return $@"<div style=""background:{data.Color};color:#fff;padding:36px 48px 32px;"">
  <div style=""font-size:9.5px;opacity:.65;letter-spacing:1.5px;text-transform:uppercase;margin-bottom:8px;"">Stronghold Companies</div>
  <div style=""font-size:10.5px;opacity:.8;text-transform:uppercase;letter-spacing:1px;margin-bottom:8px;"">{Enc(label)}</div>
  <div style=""font-size:28px;font-weight:700;margin-bottom:6px;"">{Enc(data.DivisionName)}</div>
  <div style=""font-size:13px;opacity:.85;"">{Enc(report.Period)}</div>
  <div style=""margin-top:14px;font-size:9.5px;opacity:.6;"">Prepared {DateTime.UtcNow:MMMM d, yyyy}</div>
</div>";
    }

    private static string Kpis(StructuredReportBuildData d)
    {
        var sc = d.AvgScore >= 90 ? "#276749" : d.AvgScore >= 75 ? "#c05621" : "#c53030";
        var sv = d.AvgScore.HasValue ? $"{d.AvgScore:F1}%" : "—";
        var caOverdue = d.OpenCas.Count(c => c.IsOverdue);
        var caColor   = caOverdue > 0 ? "#c53030" : d.OpenCas.Count > 0 ? "#c05621" : "#276749";
        return $@"<div class=""section-block"">
<div class=""section-hdr"">Key Performance Indicators</div>
<div class=""kpi-grid"">
  <div class=""kpi-card"" style=""border-top:3px solid {d.Color};"">
    <div class=""kpi-val"">{d.TotalAudits}</div><div class=""kpi-lbl"">Total Audits</div>
  </div>
  <div class=""kpi-card"" style=""border-top:3px solid {sc};"">
    <div class=""kpi-val"" style=""color:{sc};"">{sv}</div><div class=""kpi-lbl"">Avg Conformance</div>
  </div>
  <div class=""kpi-card"" style=""border-top:3px solid #c53030;"">
    <div class=""kpi-val"" style=""color:#c53030;"">{d.TotalNc}</div><div class=""kpi-lbl"">Non-Conformances</div>
  </div>
  <div class=""kpi-card"" style=""border-top:3px solid #c05621;"">
    <div class=""kpi-val"" style=""color:#c05621;"">{d.TotalWarnings}</div><div class=""kpi-lbl"">Warnings</div>
  </div>
  <div class=""kpi-card"" style=""border-top:3px solid #38a169;"">
    <div class=""kpi-val"" style=""color:#38a169;"">{d.CorrectedOnSitePct}%</div>
    <div class=""kpi-lbl"">Corrected On-Site</div><div class=""kpi-sub"">of NCs</div>
  </div>
  <div class=""kpi-card"" style=""border-top:3px solid {caColor};"">
    <div class=""kpi-val"" style=""color:{caColor};"">{d.OpenCas.Count}</div>
    <div class=""kpi-lbl"">Open CAs</div>
    {(caOverdue > 0 ? $"<div class=\"kpi-sub\" style=\"color:#c53030;\">{caOverdue} overdue</div>" : "")}
  </div>
</div></div>";
    }

    private static string Trend(StructuredReportBuildData d)
    {
        if (d.QuarterLabels.Count < 2) return "";
        var divData = d.QuarterLabels
            .Zip(d.DivNcRates, (q, r) => (Label: q, Value: r))
            .ToList<(string Label, double Value)>();
        var coData = d.QuarterLabels
            .Zip(d.CoNcRates, (q, r) => (Label: q, Value: r ?? 0d))
            .ToList<(string Label, double Value)>();
        return $@"<div class=""section-block"">
<div class=""section-hdr"">Conformance Trend — NC / Audit by Quarter</div>
<div class=""trend-grid"">
  <div><div class=""trend-lbl"">Division</div>{SvgChartBuilder.LineChart(divData, 290, 160)}</div>
  <div><div class=""trend-lbl"">Company-Wide</div>{SvgChartBuilder.LineChart(coData, 290, 160)}</div>
</div></div>";
    }

    private static string CategoryBreakdown(StructuredReportBuildData d)
    {
        if (!d.SectionBreakdown.Any()) return "";
        var barData = d.SectionBreakdown.Take(10)
            .Select(s => (s.SectionName.Length > 22 ? s.SectionName[..22] + "…" : s.SectionName, (double)s.NcCount))
            .ToList<(string Label, double Value)>();
        var sb = new StringBuilder();
        sb.Append($@"<div class=""section-block""><div class=""section-hdr"">Category Breakdown</div>
{SvgChartBuilder.BarChart(barData, 640, 195, colorHex: "#ef4444")}
<table style=""margin-top:10px;""><thead><tr>
  <th>Section</th><th style=""width:65px;text-align:right;"">NCs</th><th style=""width:85px;text-align:right;"">NC/Audit</th>
</tr></thead><tbody>");
        foreach (var s in d.SectionBreakdown)
        {
            var rate = d.TotalAudits > 0 ? (double)s.NcCount / d.TotalAudits : 0;
            var cls  = rate >= 0.5 ? "badge-red" : rate >= 0.2 ? "badge-amber" : "badge-green";
            sb.Append($@"<tr><td>{Enc(s.SectionName)}</td>
  <td style=""text-align:right;"">{s.NcCount}</td>
  <td style=""text-align:right;""><span class=""badge {cls}"">{rate:F2}</span></td></tr>");
        }
        sb.Append("</tbody></table></div>");
        return sb.ToString();
    }

    private static string FindingsExamples(StructuredSectionPayload sec, StructuredReportBuildData d)
    {
        if (string.IsNullOrWhiteSpace(sec.EditedNotes)) return "";
        Dictionary<string, string>? notes;
        try { notes = JsonSerializer.Deserialize<Dictionary<string, string>>(sec.EditedNotes); }
        catch { return ""; }
        if (notes is null || !notes.Any(n => !string.IsNullOrWhiteSpace(n.Value))) return "";
        var sb = new StringBuilder();
        sb.Append(@"<div class=""section-block""><div class=""section-hdr"">Findings Examples</div>");
        foreach (var (cat, text) in notes.Where(n => !string.IsNullOrWhiteSpace(n.Value)))
            sb.Append($@"<div class=""fc""><div class=""fc-title"">{Enc(cat)}</div><div class=""fc-text"">{Enc(text)}</div></div>");
        sb.Append("</div>");
        return sb.ToString();
    }

    private static string CaTable(StructuredReportBuildData d)
    {
        if (!d.OpenCas.Any())
            return @"<div class=""section-block""><div class=""section-hdr"">Open Corrective Actions</div><p style=""font-size:11px;color:#276749;padding:10px 0;"">&#10003; No open corrective actions.</p></div>";
        var sb = new StringBuilder();
        sb.Append($@"<div class=""section-block"">
<div class=""section-hdr"">Open Corrective Actions ({d.OpenCas.Count})</div>
<table><thead><tr>
  <th style=""width:38px;"">CA #</th><th>Description</th>
  <th style=""width:110px;"">Assigned To</th><th style=""width:84px;"">Due Date</th>
  <th style=""width:46px;"">Age</th><th style=""width:72px;"">Status</th>
</tr></thead><tbody>");
        foreach (var ca in d.OpenCas.OrderByDescending(c => c.IsOverdue).ThenByDescending(c => c.DaysOpen).Take(50))
        {
            var od    = ca.IsOverdue ? " overdue" : "";
            var badge = ca.IsOverdue ? "badge-red" : ca.Status == "InProgress" ? "badge-amber" : "badge-gray";
            var desc  = ca.Description.Length > 115 ? ca.Description[..115] + "…" : ca.Description;
            sb.Append($@"<tr>
  <td class=""{od}"">{ca.Id}</td>
  <td>{Enc(desc)}</td>
  <td>{Enc(ca.AssignedTo ?? "—")}</td>
  <td class=""{od}"">{Enc(ca.DueDate ?? "—")}</td>
  <td class=""{od}"">{ca.DaysOpen}d</td>
  <td><span class=""badge {badge}"">{(ca.IsOverdue ? "Overdue" : Enc(ca.Status))}</span></td>
</tr>");
        }
        sb.Append("</tbody></table></div>");
        return sb.ToString();
    }

    private static string SummaryText(StructuredSectionPayload sec, StructuredReportBuildData d)
    {
        if (string.IsNullOrWhiteSpace(sec.EditedText)) return "";
        return $@"<div class=""section-block"">
<div class=""section-hdr"">Summary</div>
<div class=""narrative"">{Enc(sec.EditedText)}</div>
</div>";
    }

    private static string Highlights(StructuredSectionPayload sec)
    {
        if (string.IsNullOrWhiteSpace(sec.EditedHighlights)) return "";
        var items = sec.EditedHighlights
            .Split('\n')
            .Select(l => l.TrimStart('•', '-', '*', ' ').Trim())
            .Where(l => l.Length > 0)
            .ToList();
        if (!items.Any()) return "";
        var sb = new StringBuilder();
        sb.Append(@"<div class=""section-block""><div class=""section-hdr"">Highlights</div><ul class=""hl-list"">");
        foreach (var item in items)
            sb.Append($@"<li><span class=""hl-chk"">&#10003;</span>{Enc(item)}</li>");
        sb.Append("</ul></div>");
        return sb.ToString();
    }

    private static string Enc(string? s) => System.Security.SecurityElement.Escape(s ?? "") ?? "";
}
