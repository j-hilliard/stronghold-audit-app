using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;
using AuditEntity = Stronghold.AppDashboard.Data.Models.Audit.Audit;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.Auditor, AuthorizationRole.AuditAdmin)]
public class CreateAudit : IRequest<int>
{
    public int DivisionId { get; set; }
    public string CreatedBy { get; set; } = null!;

    /// <summary>
    /// Optional section groups to enable for this audit (e.g. ["RADIOGRAPHY", "ROPE_ACCESS"]).
    /// Empty = no optional sections enabled. Immutable after creation.
    /// </summary>
    public List<string> EnabledOptionalGroupKeys { get; set; } = new();

    /// <summary>FK to DivisionJobPrefix. Null = use division default prefix.</summary>
    public int? JobPrefixId { get; set; }

    /// <summary>3-char site code appended to the tracking number (e.g. "IPT").</summary>
    public string? SiteCode { get; set; }
}

public class CreateAuditHandler : IRequestHandler<CreateAudit, int>
{
    private readonly AppDbContext _context;

    public CreateAuditHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateAudit request, CancellationToken cancellationToken)
    {
        var division = await _context.Divisions
            .FirstOrDefaultAsync(d => d.Id == request.DivisionId, cancellationToken)
            ?? throw new ArgumentException($"Division {request.DivisionId} not found.");

        // Lock to the current active template version at creation time — never changes
        var version = await _context.AuditTemplateVersions
            .Include(v => v.Template)
            .FirstOrDefaultAsync(v =>
                v.Template.DivisionId == request.DivisionId &&
                v.Status == "Active",
                cancellationToken)
            ?? throw new ArgumentException($"No active template found for division {division.Code}.");

        var now = DateTime.UtcNow;
        var audit = new AuditEntity
        {
            DivisionId = request.DivisionId,
            TemplateVersionId = version.Id,
            AuditType = division.AuditType,
            Status = "Draft",
            CreatedAt = now,
            CreatedBy = request.CreatedBy
        };

        _context.Audits.Add(audit);
        await _context.SaveChangesAsync(cancellationToken);

        // ── Assign tracking number ────────────────────────────────────────────
        await AssignTrackingNumberAsync(audit, request, cancellationToken);

        // Persist enabled optional section groups (immutable snapshot)
        foreach (var key in request.EnabledOptionalGroupKeys.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            _context.AuditEnabledSections.Add(new AuditEnabledSection
            {
                AuditId = audit.Id,
                OptionalGroupKey = key,
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return audit.Id;
    }

    private async Task AssignTrackingNumberAsync(AuditEntity audit, CreateAudit request, CancellationToken ct)
    {
        // Resolve prefix: use requested JobPrefixId, fall back to division default
        DivisionJobPrefix? prefix = null;
        if (request.JobPrefixId.HasValue)
        {
            prefix = await _context.DivisionJobPrefixes
                .FirstOrDefaultAsync(p => p.Id == request.JobPrefixId.Value && p.DivisionId == audit.DivisionId, ct);
        }
        prefix ??= await _context.DivisionJobPrefixes
            .Where(p => p.DivisionId == audit.DivisionId && p.IsDefault)
            .OrderBy(p => p.SortOrder)
            .FirstOrDefaultAsync(ct);

        if (prefix == null) return; // division has no prefixes configured — skip tracking number

        // Atomically increment sequence for this division + year using a row-level lock
        var year = DateTime.UtcNow.Year;
        var seq = await _context.AuditNumberSequences
            .FromSqlRaw(
                "SELECT * FROM [audit].[AuditNumberSequence] WITH (UPDLOCK, ROWLOCK) WHERE DivisionId = {0} AND Year = {1}",
                audit.DivisionId, year)
            .FirstOrDefaultAsync(ct);

        if (seq == null)
        {
            seq = new AuditNumberSequence { DivisionId = audit.DivisionId, Year = year, LastSequence = 0 };
            _context.AuditNumberSequences.Add(seq);
            await _context.SaveChangesAsync(ct); // persist so row-lock applies on retry
        }

        seq.LastSequence++;

        var letter = prefix.Prefix;
        var yy = year % 100;
        var siteCode = string.IsNullOrWhiteSpace(request.SiteCode)
            ? ""
            : $"-{request.SiteCode.Trim().ToUpperInvariant()}";

        audit.TrackingNumber = $"{letter}{yy:D2}-{seq.LastSequence:D3}{siteCode}";
        audit.JobPrefixId = prefix.Id;

        // SaveChanges is called by the caller after this returns
    }
}
