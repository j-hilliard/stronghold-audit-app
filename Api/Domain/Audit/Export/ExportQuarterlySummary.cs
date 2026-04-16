using ClosedXML.Excel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Domain.Audit.Audits;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;
using System.Globalization;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Export;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class ExportQuarterlySummary : IRequest<byte[]>
{
    public int? DivisionId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class ExportQuarterlySummaryHandler : IRequestHandler<ExportQuarterlySummary, byte[]>
{
    private readonly AppDbContext _db;
    private readonly IAuditUserContext _userContext;

    public ExportQuarterlySummaryHandler(AppDbContext db, IAuditUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<byte[]> Handle(ExportQuarterlySummary request, CancellationToken ct)
    {
        var query = _db.Audits
            .Include(a => a.Division)
            .Include(a => a.Header)
            .Include(a => a.Responses)
            .Include(a => a.Findings).ThenInclude(f => f.CorrectiveActions)
            .Where(a => a.Status == "Submitted" || a.Status == "Closed")
            .AsQueryable();

        if (!_userContext.IsGlobal && _userContext.AllowedDivisionIds is { Count: > 0 } allowed)
            query = query.Where(a => allowed.Contains(a.DivisionId));
        if (request.DivisionId.HasValue)
            query = query.Where(a => a.DivisionId == request.DivisionId.Value);
        if (request.DateFrom.HasValue)
            query = query.Where(a => (a.SubmittedAt ?? a.CreatedAt) >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(a => (a.SubmittedAt ?? a.CreatedAt) <= request.DateTo.Value);

        var audits = await query.OrderByDescending(a => a.SubmittedAt ?? a.CreatedAt).ToListAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        using var wb = new XLWorkbook();
        wb.Style.Font.FontName = "Calibri";
        wb.Style.Font.FontSize = 11;

        // ── Sheet 1: Summary Tally ─────────────────────────────────────────────
        var ws1 = wb.AddWorksheet("Summary Tally");
        WriteHeader(ws1, 1, new[] { "Division", "Audits", "Avg Score %", "Total NCs", "Total Warnings", "Corrected On-Site" });
        var divGroups = audits.GroupBy(a => a.Division.Code).OrderBy(g => g.Key);
        int r1 = 2;
        foreach (var g in divGroups)
        {
            var scores = g.Select(a => GetAuditReportHandler.ComputeTwoLevelScore(a.Responses))
                          .Where(x => x.HasValue).Select(x => x!.Value).ToList();
            ws1.Cell(r1, 1).Value = g.Key;
            ws1.Cell(r1, 2).Value = g.Count();
            ws1.Cell(r1, 3).Value = scores.Any() ? Math.Round(scores.Average(), 1) : (double?)null;
            ws1.Cell(r1, 4).Value = g.Sum(a => a.Responses.Count(rx => rx.Status == "NonConforming"));
            ws1.Cell(r1, 5).Value = g.Sum(a => a.Responses.Count(rx => rx.Status == "Warning"));
            ws1.Cell(r1, 6).Value = g.Sum(a => a.Responses.Count(rx => rx.Status == "NonConforming" && rx.CorrectedOnSite));
            r1++;
        }
        AutoFit(ws1);

        // ── Sheet 2: Corrective Actions ────────────────────────────────────────
        var ws2 = wb.AddWorksheet("Corrective Actions");
        WriteHeader(ws2, 1, new[] { "CA #", "Audit #", "Division", "Finding", "Assigned To", "Due Date", "Status", "Overdue" });
        var allCas = audits
            .SelectMany(a => a.Findings.SelectMany(f => f.CorrectiveActions.Select(ca => (a, f, ca))))
            .OrderBy(x => x.ca.DueDate).ToList();
        int r2 = 2;
        foreach (var (a, f, ca) in allCas)
        {
            ws2.Cell(r2, 1).Value = ca.Id;
            ws2.Cell(r2, 2).Value = a.Id;
            ws2.Cell(r2, 3).Value = a.Division.Code;
            ws2.Cell(r2, 4).Value = f.QuestionTextSnapshot;
            ws2.Cell(r2, 5).Value = ca.AssignedTo ?? "";
            ws2.Cell(r2, 6).Value = ca.DueDate?.ToString("yyyy-MM-dd") ?? "";
            ws2.Cell(r2, 7).Value = ca.Status;
            var overdue = ca.DueDate.HasValue && ca.DueDate.Value < today && ca.Status != "Closed";
            ws2.Cell(r2, 8).Value = overdue ? "Yes" : "";
            if (overdue) ws2.Row(r2).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
            r2++;
        }
        AutoFit(ws2);

        // ── Sheet 3: By Employee ───────────────────────────────────────────────
        var ws3 = wb.AddWorksheet("By Employee");
        WriteHeader(ws3, 1, new[] { "Auditor", "Audit Count", "Avg Score %", "Total NCs", "Total Warnings", "Last Audit Date" });
        var byEmp = audits
            .Where(a => !string.IsNullOrWhiteSpace(a.Header?.Auditor))
            .GroupBy(a => a.Header!.Auditor!)
            .OrderBy(g => g.Key);
        int r3 = 2;
        foreach (var g in byEmp)
        {
            var scores = g.Select(a => GetAuditReportHandler.ComputeTwoLevelScore(a.Responses))
                          .Where(x => x.HasValue).Select(x => x!.Value).ToList();
            var lastDate = g.Max(a => a.Header?.AuditDate?.ToString("yyyy-MM-dd"));
            ws3.Cell(r3, 1).Value = g.Key;
            ws3.Cell(r3, 2).Value = g.Count();
            ws3.Cell(r3, 3).Value = scores.Any() ? Math.Round(scores.Average(), 1) : (double?)null;
            ws3.Cell(r3, 4).Value = g.Sum(a => a.Responses.Count(rx => rx.Status == "NonConforming"));
            ws3.Cell(r3, 5).Value = g.Sum(a => a.Responses.Count(rx => rx.Status == "Warning"));
            ws3.Cell(r3, 6).Value = lastDate ?? "";
            r3++;
        }
        AutoFit(ws3);

        // ── Sheet 4: Audit Log ─────────────────────────────────────────────────
        var ws4 = wb.AddWorksheet("Audit Log");
        WriteHeader(ws4, 1, new[] { "Audit #", "Division", "Status", "Audit Date", "Auditor", "Job #", "Location", "Score %", "NCs", "Warnings" });
        int r4 = 2;
        foreach (var a in audits)
        {
            var score = GetAuditReportHandler.ComputeTwoLevelScore(a.Responses);
            ws4.Cell(r4, 1).Value = a.Id;
            ws4.Cell(r4, 2).Value = a.Division.Code;
            ws4.Cell(r4, 3).Value = a.Status;
            ws4.Cell(r4, 4).Value = a.Header?.AuditDate?.ToString("yyyy-MM-dd") ?? "";
            ws4.Cell(r4, 5).Value = a.Header?.Auditor ?? "";
            ws4.Cell(r4, 6).Value = a.Header?.JobNumber ?? "";
            ws4.Cell(r4, 7).Value = a.Header?.Location ?? "";
            ws4.Cell(r4, 8).Value = score.HasValue ? Math.Round(score.Value, 1) : (double?)null;
            ws4.Cell(r4, 9).Value = a.Responses.Count(rx => rx.Status == "NonConforming");
            ws4.Cell(r4, 10).Value = a.Responses.Count(rx => rx.Status == "Warning");
            r4++;
        }
        AutoFit(ws4);

        return SaveWorkbook(wb);
    }

    private static void WriteHeader(IXLWorksheet ws, int row, string[] headers)
    {
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(row, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1E3A5F");
            cell.Style.Font.FontColor = XLColor.White;
        }
    }

    private static void AutoFit(IXLWorksheet ws) => ws.Columns().AdjustToContents();

    private static byte[] SaveWorkbook(XLWorkbook wb)
    {
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
