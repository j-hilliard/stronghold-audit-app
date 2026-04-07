using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Api.Services;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

[AllowedAuthorizationRole(AuthorizationRole.Administrator)]
public class UpdateEmailRouting : IRequest<Unit>
{
    public UpdateEmailRoutingRequest Payload { get; set; } = null!;
    public string UpdatedBy { get; set; } = null!;
}

public class UpdateEmailRoutingHandler : IRequestHandler<UpdateEmailRouting, Unit>
{
    private readonly AppDbContext _context;
    private readonly IProcessLogService _log;

    public UpdateEmailRoutingHandler(AppDbContext context, IProcessLogService log)
    {
        _context = context;
        _log = log;
    }

    public async Task<Unit> Handle(UpdateEmailRouting request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        foreach (var dto in request.Payload.Rules)
        {
            if (dto.Id.HasValue)
            {
                // Update existing rule
                var existing = await _context.EmailRoutingRules.FindAsync(new object[] { dto.Id.Value }, cancellationToken);
                if (existing != null)
                {
                    existing.EmailAddress = dto.EmailAddress;
                    existing.IsActive = dto.IsActive;
                    existing.UpdatedAt = now;
                    existing.UpdatedBy = request.UpdatedBy;
                }
            }
            else
            {
                // Insert new rule
                _context.EmailRoutingRules.Add(new EmailRoutingRule
                {
                    DivisionId = dto.DivisionId,
                    EmailAddress = dto.EmailAddress,
                    IsActive = dto.IsActive,
                    CreatedAt = now,
                    CreatedBy = request.UpdatedBy
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _log.LogAsync("UpdateEmailRouting", "EmailRoutingRule", "Info",
            $"Email routing updated by {request.UpdatedBy}. {request.Payload.Rules.Count} rule(s) processed.",
            relatedObject: null);

        return Unit.Value;
    }
}
