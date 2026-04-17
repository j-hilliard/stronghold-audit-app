using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

// ── DTO ────────────────────────────────────────────────────────────────────────

public class DivisionScoreTargetDto
{
    public int DivisionId { get; set; }
    public string DivisionCode { get; set; } = null!;
    public string DivisionName { get; set; } = null!;
    public decimal? ScoreTarget { get; set; }
    /// <summary>How often this division is expected to audit, in days. Null = no schedule set.</summary>
    public int? AuditFrequencyDays { get; set; }
    /// <summary>When true, closing a CA in this division requires at least one closure photo.</summary>
    public bool RequireClosurePhoto { get; set; }
}

// ── Get all divisions with current targets ─────────────────────────────────────

[AllowedAuthorizationRole(AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator)]
public class GetDivisionScoreTargets : IRequest<List<DivisionScoreTargetDto>> { }

public class GetDivisionScoreTargetsHandler : IRequestHandler<GetDivisionScoreTargets, List<DivisionScoreTargetDto>>
{
    private readonly AppDbContext _db;
    public GetDivisionScoreTargetsHandler(AppDbContext db) => _db = db;

    public async Task<List<DivisionScoreTargetDto>> Handle(GetDivisionScoreTargets request, CancellationToken ct)
    {
        return await _db.Divisions
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .Select(d => new DivisionScoreTargetDto
            {
                DivisionId         = d.Id,
                DivisionCode       = d.Code,
                DivisionName       = d.Name,
                ScoreTarget        = d.ScoreTarget,
                AuditFrequencyDays = d.AuditFrequencyDays,
                RequireClosurePhoto = d.RequireClosurePhoto,
            })
            .ToListAsync(ct);
    }
}

// ── Set target ─────────────────────────────────────────────────────────────────

[AllowedAuthorizationRole(AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator)]
public class SetDivisionScoreTarget : IRequest<DivisionScoreTargetDto>
{
    public int DivisionId { get; set; }
    /// <summary>0–100, or null to clear the target.</summary>
    public decimal? ScoreTarget { get; set; }
}

public class SetDivisionScoreTargetHandler : IRequestHandler<SetDivisionScoreTarget, DivisionScoreTargetDto>
{
    private readonly AppDbContext _db;
    public SetDivisionScoreTargetHandler(AppDbContext db) => _db = db;

    public async Task<DivisionScoreTargetDto> Handle(SetDivisionScoreTarget request, CancellationToken ct)
    {
        if (request.ScoreTarget.HasValue && (request.ScoreTarget.Value < 0 || request.ScoreTarget.Value > 100))
            throw new ArgumentException("ScoreTarget must be between 0 and 100.");

        var division = await _db.Divisions.FirstOrDefaultAsync(d => d.Id == request.DivisionId, ct)
            ?? throw new KeyNotFoundException($"Division {request.DivisionId} not found.");

        division.ScoreTarget = request.ScoreTarget;
        await _db.SaveChangesAsync(ct);

        return new DivisionScoreTargetDto
        {
            DivisionId = division.Id,
            DivisionCode = division.Code,
            DivisionName = division.Name,
            ScoreTarget = division.ScoreTarget,
        };
    }
}

// ── Set audit frequency ───────────────────────────────────────────────────────

[AllowedAuthorizationRole(AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator)]
public class SetDivisionAuditFrequency : IRequest<DivisionScoreTargetDto>
{
    public int DivisionId { get; set; }
    /// <summary>Days between audits, or null to clear the schedule.</summary>
    public int? AuditFrequencyDays { get; set; }
}

public class SetDivisionAuditFrequencyHandler : IRequestHandler<SetDivisionAuditFrequency, DivisionScoreTargetDto>
{
    private readonly AppDbContext _db;
    public SetDivisionAuditFrequencyHandler(AppDbContext db) => _db = db;

    public async Task<DivisionScoreTargetDto> Handle(SetDivisionAuditFrequency request, CancellationToken ct)
    {
        if (request.AuditFrequencyDays.HasValue && request.AuditFrequencyDays.Value <= 0)
            throw new ArgumentException("AuditFrequencyDays must be a positive integer.");

        var division = await _db.Divisions.FirstOrDefaultAsync(d => d.Id == request.DivisionId, ct)
            ?? throw new KeyNotFoundException($"Division {request.DivisionId} not found.");

        division.AuditFrequencyDays = request.AuditFrequencyDays;
        await _db.SaveChangesAsync(ct);

        return new DivisionScoreTargetDto
        {
            DivisionId = division.Id,
            DivisionCode = division.Code,
            DivisionName = division.Name,
            ScoreTarget = division.ScoreTarget,
            AuditFrequencyDays = division.AuditFrequencyDays,
        };
    }
}

// ── Set closure photo requirement ─────────────────────────────────────────────

[AllowedAuthorizationRole(AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator)]
public class SetDivisionClosurePhotoRequirement : IRequest<DivisionScoreTargetDto>
{
    public int  DivisionId          { get; set; }
    public bool RequireClosurePhoto { get; set; }
}

public class SetDivisionClosurePhotoRequirementHandler : IRequestHandler<SetDivisionClosurePhotoRequirement, DivisionScoreTargetDto>
{
    private readonly AppDbContext _db;
    public SetDivisionClosurePhotoRequirementHandler(AppDbContext db) => _db = db;

    public async Task<DivisionScoreTargetDto> Handle(SetDivisionClosurePhotoRequirement request, CancellationToken ct)
    {
        var division = await _db.Divisions.FirstOrDefaultAsync(d => d.Id == request.DivisionId, ct)
            ?? throw new KeyNotFoundException($"Division {request.DivisionId} not found.");

        division.RequireClosurePhoto = request.RequireClosurePhoto;
        await _db.SaveChangesAsync(ct);

        return new DivisionScoreTargetDto
        {
            DivisionId          = division.Id,
            DivisionCode        = division.Code,
            DivisionName        = division.Name,
            ScoreTarget         = division.ScoreTarget,
            AuditFrequencyDays  = division.AuditFrequencyDays,
            RequireClosurePhoto = division.RequireClosurePhoto,
        };
    }
}

// ── Benchmark query (company-wide avg for a division's recent audits) ──────────

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.ReadOnlyViewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class GetDivisionBenchmark : IRequest<DivisionBenchmarkDto>
{
    public int DivisionId { get; set; }
    /// <summary>How many recent submitted audits to average. Defaults to 10.</summary>
    public int WindowSize { get; set; } = 10;
}

public class DivisionBenchmarkDto
{
    public int DivisionId { get; set; }
    /// <summary>Average score across the last WindowSize submitted audits for this division. Null if no audits.</summary>
    public double? CompanyAvgScore { get; set; }
    public decimal? ScoreTarget { get; set; }
    public int AuditCount { get; set; }
}

public class GetDivisionBenchmarkHandler : IRequestHandler<GetDivisionBenchmark, DivisionBenchmarkDto>
{
    private readonly AppDbContext _db;
    public GetDivisionBenchmarkHandler(AppDbContext db) => _db = db;

    public async Task<DivisionBenchmarkDto> Handle(GetDivisionBenchmark request, CancellationToken ct)
    {
        var division = await _db.Divisions
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == request.DivisionId, ct);

        if (division == null)
            return new DivisionBenchmarkDto { DivisionId = request.DivisionId };

        var recentAudits = await _db.Audits
            .AsNoTracking()
            .Where(a => a.DivisionId == request.DivisionId
                     && (a.Status == "Submitted" || a.Status == "Closed"))
            .OrderByDescending(a => a.SubmittedAt)
            .Take(request.WindowSize)
            .Include(a => a.Responses)
            .ToListAsync(ct);

        var scores = recentAudits
            .Select(a => Audits.GetAuditReportHandler.ComputeTwoLevelScore(a.Responses))
            .Where(s => s.HasValue)
            .Select(s => s!.Value)
            .ToList();

        return new DivisionBenchmarkDto
        {
            DivisionId = request.DivisionId,
            CompanyAvgScore = scores.Count > 0 ? Math.Round(scores.Average(), 1) : null,
            ScoreTarget = division.ScoreTarget,
            AuditCount = recentAudits.Count,
        };
    }
}
