using ClosedXML.Excel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Export;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.AuditAdmin, AuthorizationRole.Executive)]
public class ExportNcrReport : IRequest<byte[]>
{
    public int? DivisionId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class ExportNcrReportHandler : IRequestHandler<ExportNcrReport, byte[]>
{
    private readonly AppDbContext _db;
    private readonly IAuditUserContext _userContext;

    public ExportNcrReportHandler(AppDbContext db, IAuditUserContext userContext)
    {
        _db = db;
        _userContext = userContext;
    }

    public async Task<byte[]> Handle(ExportNcrReport request, CancellationToken ct)
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

        // ── Sheet 1: Non-Conformances ──────────────────────────────────────────
        var ws1 = wb.AddWorksheet("Non-Conformances");
        WriteHeader(ws1, 1, new[] {
            "Audit #", "Division", "Audit Date", "Section", "Finding", "Corrected On-Site", "Comment"
        });
        var allFindings = audits
            .SelectMany(a => a.Findings
                .Where(f => !f.IsDeleted)
                .Select(f => (audit: a, finding: f)))
            .OrderByDescending(x => x.audit.SubmittedAt ?? x.audit.CreatedAt)
            .ToList();

        int r1 = 2;
        foreach (var (a, f) in allFindings)
        {
            // Get section from the matching response
            var sectionName = a.Responses
                .FirstOrDefault(rx => rx.QuestionId == f.QuestionId)?.SectionNameSnapshot ?? "";
            ws1.Cell(r1, 1).Value = a.Id;
            ws1.Cell(r1, 2).Value = a.Division.Code;
            ws1.Cell(r1, 3).Value = a.Header?.AuditDate?.ToString("yyyy-MM-dd") ?? "";
            ws1.Cell(r1, 4).Value = sectionName;
            ws1.Cell(r1, 5).Value = f.QuestionTextSnapshot;
            ws1.Cell(r1, 6).Value = f.CorrectedOnSite ? "Yes" : "No";
            ws1.Cell(r1, 7).Value = f.Description ?? "";
            r1++;
        }
        AutoFit(ws1);

        // ── Sheet 2: Open CAs ──────────────────────────────────────────────────
        var ws2 = wb.AddWorksheet("Open CAs");
        WriteHeader(ws2, 1, new[] {
            "CA #", "Audit #", "Division", "Audit Date", "Finding", "Assigned To", "Due Date", "Status", "Days Open", "Overdue"
        });
        var nowUtc = DateTime.UtcNow;
        var openCas = audits
            .SelectMany(a => a.Findings
                .SelectMany(f => f.CorrectiveActions
                    .Where(ca => ca.Status != "Closed")
                    .Select(ca => (audit: a, finding: f, ca))))
            .OrderBy(x => x.ca.DueDate)
            .ToList();

        int r2 = 2;
        foreach (var (a, f, ca) in openCas)
        {
            var isOverdue = ca.DueDate.HasValue && ca.DueDate.Value < today;
            ws2.Cell(r2, 1).Value = ca.Id;
            ws2.Cell(r2, 2).Value = a.Id;
            ws2.Cell(r2, 3).Value = a.Division.Code;
            ws2.Cell(r2, 4).Value = a.Header?.AuditDate?.ToString("yyyy-MM-dd") ?? "";
            ws2.Cell(r2, 5).Value = f.QuestionTextSnapshot;
            ws2.Cell(r2, 6).Value = ca.AssignedTo ?? "";
            ws2.Cell(r2, 7).Value = ca.DueDate?.ToString("yyyy-MM-dd") ?? "";
            ws2.Cell(r2, 8).Value = ca.Status;
            ws2.Cell(r2, 9).Value = (int)(nowUtc - ca.CreatedAt).TotalDays;
            ws2.Cell(r2, 10).Value = isOverdue ? "Yes" : "";
            if (isOverdue) ws2.Row(r2).Style.Fill.BackgroundColor = XLColor.FromHtml("#FEE2E2");
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
