using Stronghold.AppDashboard.Data.Models.Audit;

namespace Stronghold.AppDashboard.Api.Services;

/// <summary>Canonical email HTML builder shared by GetDistributionPreview and SendDistributionEmail.</summary>
public interface IAuditEmailBodyBuilder
{
    /// <summary>Returns (Subject, BodyHtml) for an audit distribution email or preview.</summary>
    (string Subject, string BodyHtml) Build(AuditEmailBuildOptions opts);
}

public class AuditEmailBuildOptions
{
    public required Audit Audit { get; init; }
    /// <summary>Overrides Audit.ReviewSummary when set (maps to the Message field on send).</summary>
    public string? FindingsSummaryOverride { get; init; }
    public string? SubjectOverride { get; init; }
    public bool IncludeCorrectiveActions { get; init; } = true;
    public bool IncludeOpenCasOnly { get; init; }
    public IReadOnlyList<int> SelectedAttachmentIds { get; init; } = Array.Empty<int>();
    public required string AppBaseUrl { get; init; }
    /// <summary>When set, footer reads "Sent by {name}". Null produces preview footer text.</summary>
    public string? SentByName { get; init; }
}

public class AuditEmailBodyBuilder : IAuditEmailBodyBuilder
{
    public (string Subject, string BodyHtml) Build(AuditEmailBuildOptions opts)
    {
        var audit     = opts.Audit;
        var header    = audit.Header;
        var divCode   = audit.Division?.Code ?? "—";
        var auditDate = header?.AuditDate?.ToString("MM/dd/yyyy")
                        ?? audit.SubmittedAt?.ToString("MM/dd/yyyy")
                        ?? "—";
        var location  = header?.Location  ?? "—";
        var auditor   = header?.Auditor   ?? "—";
        var pm        = header?.PM        ?? "—";
        var client    = header?.Client    ?? "—";
        var jobNumber = header?.JobNumber ?? "—";

        // ── Conformance score ─────────────────────────────────────────────────
        var responses  = audit.Responses.Where(r => r.Status != null).ToList();
        var conforming = responses.Count(r => r.Status == "Conforming");
        var denom      = responses.Count(r => r.Status is "Conforming" or "NonConforming" or "Warning");
        var scoreText  = denom > 0 ? $"{Math.Round((double)conforming / denom * 100, 1)}%" : "N/A";

        var reviewLink = $"{opts.AppBaseUrl}/audits/{audit.Id}/review";

        var subject = !string.IsNullOrWhiteSpace(opts.SubjectOverride)
            ? opts.SubjectOverride
            : $"[{divCode}] Compliance Audit Distribution — {auditDate}";

        // ── Findings summary block ────────────────────────────────────────────
        var effectiveSummary = !string.IsNullOrWhiteSpace(opts.FindingsSummaryOverride)
            ? opts.FindingsSummaryOverride
            : audit.ReviewSummary;

        var summaryBlock = !string.IsNullOrWhiteSpace(effectiveSummary)
            ? $"""
              <div style="margin:20px 0;padding:14px 16px;background:#f8fafc;border-left:4px solid #1e3a5f;border-radius:0 4px 4px 0;">
                <p style="margin:0 0 6px 0;font-size:12px;font-weight:bold;color:#1e3a5f;text-transform:uppercase;letter-spacing:0.5px;">Findings Summary</p>
                <p style="margin:0;color:#1e293b;font-size:14px;line-height:1.7;white-space:pre-wrap;">{Encode(effectiveSummary!)}</p>
              </div>
              """
            : string.Empty;

        // ── Non-conforming findings block ─────────────────────────────────────
        var findings = audit.Findings.Where(f => !f.IsDeleted).OrderBy(f => f.Id).ToList();

        string findingsBlock;
        if (findings.Count == 0)
        {
            findingsBlock = "<p style=\"color:#16a34a;font-size:14px;\">No non-conforming findings.</p>";
        }
        else
        {
            var rows = string.Join("", findings.Select((f, i) =>
            {
                string caRows;
                if (!opts.IncludeCorrectiveActions)
                {
                    caRows = string.Empty;
                }
                else
                {
                    var cas = f.CorrectiveActions
                        .Where(ca => !ca.IsDeleted &&
                               (!opts.IncludeOpenCasOnly || (ca.Status != "Closed" && ca.Status != "Voided")))
                        .OrderBy(ca => ca.CreatedAt)
                        .ToList();

                    caRows = cas.Count == 0
                        ? "<tr><td colspan=\"3\" style=\"padding:4px 8px;color:#94a3b8;font-size:12px;\">No corrective actions assigned</td></tr>"
                        : string.Join("", cas.Select(ca =>
                            $"<tr>" +
                            $"<td style=\"padding:4px 8px 4px 24px;font-size:12px;color:#475569;\">{Encode(ca.Description ?? "—")}</td>" +
                            $"<td style=\"padding:4px 8px;font-size:12px;color:#475569;\">{Encode(ca.AssignedTo ?? "Unassigned")}</td>" +
                            $"<td style=\"padding:4px 8px;font-size:12px;color:#475569;\">{ca.DueDate?.ToString("MM/dd/yyyy") ?? "—"}</td>" +
                            $"</tr>"));
                }

                return $"""
                    <tr style="background:{(i % 2 == 0 ? "#fff" : "#f8fafc")}">
                      <td colspan="3" style="padding:8px 8px 4px;font-size:13px;font-weight:600;color:#dc2626;">
                        {i + 1}. {Encode(f.QuestionTextSnapshot ?? "—")}
                        {(f.CorrectedOnSite ? "<span style=\"margin-left:8px;font-size:11px;color:#16a34a;font-weight:normal;\">[Corrected On-Site]</span>" : "")}
                      </td>
                    </tr>
                    {caRows}
                    """;
            }));

            var caHeaders = opts.IncludeCorrectiveActions
                ? """
                  <th style="padding:8px;color:#fff;text-align:left;font-size:12px;">Assigned To</th>
                  <th style="padding:8px;color:#fff;text-align:left;font-size:12px;">Due Date</th>
                  """
                : string.Empty;

            findingsBlock = $"""
                <table style="width:100%;border-collapse:collapse;font-size:13px;">
                  <thead>
                    <tr style="background:#1e3a5f;">
                      <th style="padding:8px;color:#fff;text-align:left;font-size:12px;">Finding</th>
                      {caHeaders}
                    </tr>
                  </thead>
                  <tbody>{rows}</tbody>
                </table>
                """;
        }

        // ── Attachments block ─────────────────────────────────────────────────
        var selectedAttachments = audit.Attachments
            .Where(a => opts.SelectedAttachmentIds.Contains(a.Id) && !a.IsDeleted)
            .ToList();

        var attachmentBlock = selectedAttachments.Count > 0
            ? $"""
              <div style="margin-top:16px;">
                <p style="margin:0 0 6px;font-size:12px;font-weight:bold;color:#334155;text-transform:uppercase;letter-spacing:0.5px;">Attachments Included ({selectedAttachments.Count})</p>
                <ul style="margin:0;padding-left:20px;">{string.Join("", selectedAttachments.Select(a => $"<li style=\"font-size:13px;color:#475569;\">{Encode(a.FileName)}</li>"))}</ul>
              </div>
              """
            : string.Empty;

        // ── Footer ────────────────────────────────────────────────────────────
        var footerText = opts.SentByName != null
            ? $"Sent by {Encode(opts.SentByName)} via Stronghold Compliance Audit system."
            : "Preview generated by Stronghold Compliance Audit system.";

        // ── Full HTML body ────────────────────────────────────────────────────
        var bodyHtml = $"""
            <div style="font-family:Arial,sans-serif;max-width:640px;margin:0 auto;">
              <div style="background:#1e3a5f;padding:20px;border-radius:4px 4px 0 0;">
                <h2 style="color:#fff;margin:0;">{divCode} — Compliance Audit Distribution</h2>
                <p style="color:#93c5fd;margin:6px 0 0;font-size:13px;">{auditDate}</p>
              </div>
              <div style="border:1px solid #ddd;border-top:none;padding:20px;border-radius:0 0 4px 4px;">
                <table style="width:100%;border-collapse:collapse;margin-bottom:16px;">
                  <tr><td style="padding:5px 0;color:#64748b;width:140px;font-size:13px;">Division</td><td style="padding:5px 0;font-weight:bold;font-size:13px;">{divCode}</td></tr>
                  <tr><td style="padding:5px 0;color:#64748b;font-size:13px;">Location</td><td style="padding:5px 0;font-size:13px;">{Encode(location)}</td></tr>
                  <tr><td style="padding:5px 0;color:#64748b;font-size:13px;">Auditor</td><td style="padding:5px 0;font-size:13px;">{Encode(auditor)}</td></tr>
                  <tr><td style="padding:5px 0;color:#64748b;font-size:13px;">Project Manager</td><td style="padding:5px 0;font-size:13px;">{Encode(pm)}</td></tr>
                  <tr><td style="padding:5px 0;color:#64748b;font-size:13px;">Client</td><td style="padding:5px 0;font-size:13px;">{Encode(client)}</td></tr>
                  <tr><td style="padding:5px 0;color:#64748b;font-size:13px;">Job Number</td><td style="padding:5px 0;font-size:13px;">{Encode(jobNumber)}</td></tr>
                  <tr><td style="padding:5px 0;color:#64748b;font-size:13px;">Audit Date</td><td style="padding:5px 0;font-size:13px;">{auditDate}</td></tr>
                  <tr><td style="padding:5px 0;color:#64748b;font-size:13px;">Conformance Score</td><td style="padding:5px 0;font-weight:bold;font-size:13px;color:{(findings.Count == 0 ? "#16a34a" : "#dc2626")}">{scoreText}</td></tr>
                </table>

                {summaryBlock}

                <h3 style="font-size:14px;color:#1e293b;margin:20px 0 10px;border-bottom:1px solid #e2e8f0;padding-bottom:6px;">
                  Non-Conforming Findings ({findings.Count})
                </h3>
                {findingsBlock}
                {attachmentBlock}

                <div style="margin-top:24px;">
                  <a href="{reviewLink}" style="background:#1e3a5f;color:#fff;padding:10px 20px;border-radius:4px;text-decoration:none;display:inline-block;font-size:13px;">
                    View Full Audit Report →
                  </a>
                </div>
                <p style="color:#94a3b8;font-size:11px;margin-top:20px;">{footerText}</p>
              </div>
            </div>
            """;

        return (subject, bodyHtml);
    }

    private static string Encode(string? s) =>
        s is null ? string.Empty : System.Net.WebUtility.HtmlEncode(s);
}
