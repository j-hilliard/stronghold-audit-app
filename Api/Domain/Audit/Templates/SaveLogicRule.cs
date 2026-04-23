using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Templates;

// ── Get rules for a version ────────────────────────────────────────────────────

[AllowedAuthorizationRole(AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator, AuthorizationRole.AuditAdmin)]
public class GetLogicRules : IRequest<List<LogicRuleDto>>
{
    public int TemplateVersionId { get; set; }
}

public class GetLogicRulesHandler : IRequestHandler<GetLogicRules, List<LogicRuleDto>>
{
    private readonly AppDbContext _db;
    public GetLogicRulesHandler(AppDbContext db) => _db = db;

    public async Task<List<LogicRuleDto>> Handle(GetLogicRules request, CancellationToken ct)
    {
        var rules = await _db.QuestionLogicRules
            .Where(r => r.TemplateVersionId == request.TemplateVersionId && r.IsActive)
            .ToListAsync(ct);

        return rules.Select(ToDto).ToList();
    }

    internal static LogicRuleDto ToDto(QuestionLogicRule r) => new()
    {
        Id = r.Id,
        TriggerVersionQuestionId = r.TriggerVersionQuestionId,
        TriggerResponse = r.TriggerResponse,
        Action = r.Action,
        TargetSectionId = r.TargetSectionId,
    };
}

// ── Upsert ─────────────────────────────────────────────────────────────────────

[AllowedAuthorizationRole(AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator, AuthorizationRole.AuditAdmin)]
public class UpsertLogicRule : IRequest<LogicRuleDto>
{
    public SaveLogicRuleRequest Rule { get; set; } = null!;
}

public class UpsertLogicRuleHandler : IRequestHandler<UpsertLogicRule, LogicRuleDto>
{
    private readonly AppDbContext _db;
    public UpsertLogicRuleHandler(AppDbContext db) => _db = db;

    public async Task<LogicRuleDto> Handle(UpsertLogicRule request, CancellationToken ct)
    {
        var req = request.Rule;
        QuestionLogicRule entity;

        if (req.Id.HasValue && req.Id.Value > 0)
        {
            entity = await _db.QuestionLogicRules.FirstOrDefaultAsync(r => r.Id == req.Id.Value, ct)
                ?? throw new KeyNotFoundException($"LogicRule {req.Id.Value} not found.");
        }
        else
        {
            entity = new QuestionLogicRule();
            _db.QuestionLogicRules.Add(entity);
        }

        entity.TemplateVersionId         = req.TemplateVersionId;
        entity.TriggerVersionQuestionId  = req.TriggerVersionQuestionId;
        entity.TriggerResponse           = req.TriggerResponse;
        entity.Action                    = req.Action;
        entity.TargetSectionId           = req.TargetSectionId;
        entity.IsActive                  = true;

        await _db.SaveChangesAsync(ct);
        return GetLogicRulesHandler.ToDto(entity);
    }
}

// ── Delete ─────────────────────────────────────────────────────────────────────

[AllowedAuthorizationRole(AuthorizationRole.TemplateAdmin, AuthorizationRole.Administrator, AuthorizationRole.AuditAdmin)]
public class DeleteLogicRule : IRequest<Unit>
{
    public int Id { get; set; }
}

public class DeleteLogicRuleHandler : IRequestHandler<DeleteLogicRule, Unit>
{
    private readonly AppDbContext _db;
    public DeleteLogicRuleHandler(AppDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteLogicRule request, CancellationToken ct)
    {
        var entity = await _db.QuestionLogicRules.FirstOrDefaultAsync(r => r.Id == request.Id, ct)
            ?? throw new KeyNotFoundException($"LogicRule {request.Id} not found.");
        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
