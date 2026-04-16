using ClosedXML.Excel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Export;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.ExecutiveViewer,
    AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator)]
public class ExportCorrectiveActions : IRequest<byte[]>
{
    public int? DivisionId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class ExportCorrectiveActionsHandler : IRequestHandler<ExportCorrectiveActions, byte[]>
{
    private readonly AppDbContext _db;
    private readonly IAuditUserContext _userContext;

    public ExportCorrectiveActionsHandler(AppDbContext db, IAuditUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<byte[]> Handle(ExportCorrectiveActions request, CancellationToken ct)
    {
        var caQuery = _db.CorrectiveActions
            .Include(ca => ca.Finding).ThenInclude(f => f.Audit).ThenInclude(a => a.Division)
            .Include(ca => ca.Finding).ThenInclude(f => f.Audit).ThenInclude(a => a.Header)
            .AsQueryable();

        // Division scope
        if (!_userContext.IsGlobal && _userContext.AllowedDivisionIds is { Count: > 0 } allowed)
            caQuery = caQuery.Where(ca => allowed.Contains(ca.Finding.Audit.DivisionId));
        if (request.DivisionId.HasValue)
            caQuery = caQuery.Where(ca => ca.Finding.Audit.DivisionId == request.DivisionId.Value);
        if (request.DateFrom.HasValue)
            caQuery = caQuery.Where(ca => ca.CreatedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            caQuery = caQuery.Where(ca => ca.CreatedAt <= request.DateTo.Value);

        var allCas = await caQuery.OrderBy(ca => ca.DueDate).ToListAsync(ct);

        var today   = DateOnly.FromDateTime(DateTime.UtcNow);
        var nowUtc  = DateTime.UtcNow;

        using var wb = new XLWorkbook();
        wb.Style.Font.FontName = "Calibri";
        wb.Style.Font.FontSize = 11;

        var openCas   = allCas.Where(ca => ca.Status != "Closed").ToList();
        var closedCas = allCas.Where(ca => ca.Status == "Closed").ToList();

        // ── Sheet 1: Open CAs ──────────────────────────────────────────────────
        var ws1 = wb.AddWorksheet("Open CAs");
        WriteHeader(ws1, 1, new[] {
            "CA #", "Audit #", "Division", "Audit Date", "Finding", "Assigned To",
            "Due Date", "Status", "Days Open", "Overdue"
        });
        int r1 = 2;
        foreach (var ca in openCas)
        {
            var a = ca.Finding.Audit;
            var isOverdue = ca.DueDate.HasValue && ca.DueDate.Value < today;
            ws1.Cell(r1, 1).Value  = ca.Id;
            ws1.Cell(r1, 2).Value  = a?.Id ?? 0;
            ws1.Cell(r1, 3).Value  = a?.Division?.Code ?? "";
            ws1.Cell(r1, 4).Value  = a?.Header?.AuditDate?.ToString("yyyy-MM-dd") ?? "";
            ws1.Cell(r1, 5).Value  = ca.Finding.QuestionTextSnapshot;
            ws1.Cell(r1, 6).Value  = ca.AssignedTo ?? "";
            ws1.Cell(r1, 7).Value  = ca.DueDate?.ToString("yyyy-MM-dd") ?? "";
            ws1.Cell(r1, 8).Value  = ca.Status;
            ws1.Cell(r1, 9).Value  = (int)(nowUtc - ca.CreatedAt).TotalDays;
            ws1.Cell(r1, 10).Value = isOverdue ? "Yes" : "";
            if (isOverdue) ws1.Row(r1).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
            r1++;
        }
        AutoFit(ws1);

        // ── Sheet 2: Closed CAs ────────────────────────────────────────────────
        var ws2 = wb.AddWorksheet("Closed CAs");
        WriteHeader(ws2, 1, new[] {
            "CA #", "Audit #", "Division", "Audit Date", "Finding", "Assigned To",
            "Due Date", "Completed Date", "Days to Close"
        });
        int r2 = 2;
        foreach (var ca in closedCas)
        {
            var a = ca.Finding.Audit;
            int? daysToClose = ca.CompletedDate.HasValue
                ? (int)(ca.CompletedDate.Value.ToDateTime(TimeOnly.MinValue) - ca.CreatedAt).TotalDays
                : null;
            ws2.Cell(r2, 1).Value = ca.Id;
            ws2.Cell(r2, 2).Value = a?.Id ?? 0;
            ws2.Cell(r2, 3).Value = a?.Division?.Code ?? "";
            ws2.Cell(r2, 4).Value = a?.Header?.AuditDate?.ToString("yyyy-MM-dd") ?? "";
            ws2.Cell(r2, 5).Value = ca.Finding.QuestionTextSnapshot;
            ws2.Cell(r2, 6).Value = ca.AssignedTo ?? "";
            ws2.Cell(r2, 7).Value = ca.DueDate?.ToString("yyyy-MM-dd") ?? "";
            ws2.Cell(r2, 8).Value = ca.CompletedDate?.ToString("yyyy-MM-dd") ?? "";
            if (daysToClose.HasValue) ws2.Cell(r2, 9).Value = daysToClose.Value;
            r2++;
        }
        AutoFit(ws2);

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
