using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Admin;

public class SaveReviewGroupRequest
{
    public List<ReviewGroupMemberDto> Members { get; set; } = new();
}

[AllowedAuthorizationRole(AuthorizationRole.Administrator, AuthorizationRole.TemplateAdmin, AuthorizationRole.AuditAdmin)]
public class SaveReviewGroup : IRequest<Unit>
{
    public SaveReviewGroupRequest Payload { get; set; } = null!;
    public string SavedBy { get; set; } = null!;
}

public class SaveReviewGroupHandler : IRequestHandler<SaveReviewGroup, Unit>
{
    private readonly AppDbContext _context;

    public SaveReviewGroupHandler(AppDbContext context) => _context = context;

    public async Task<Unit> Handle(SaveReviewGroup request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // Get existing (non-deleted) members
        var existing = await _context.ReviewGroupMembers.ToListAsync(cancellationToken);

        // Soft-delete any member not in the incoming list
        var incomingIds = request.Payload.Members.Where(m => m.Id > 0).Select(m => m.Id).ToHashSet();
        foreach (var member in existing.Where(m => !incomingIds.Contains(m.Id)))
        {
            member.IsDeleted = true;
            member.DeletedAt = now;
            member.DeletedBy = request.SavedBy;
        }

        foreach (var dto in request.Payload.Members)
        {
            if (dto.Id > 0)
            {
                // Update existing
                var member = existing.FirstOrDefault(m => m.Id == dto.Id);
                if (member != null)
                {
                    member.Name = dto.Name;
                    member.Email = dto.Email;
                    member.IsActive = dto.IsActive;
                    member.UpdatedAt = now;
                    member.UpdatedBy = request.SavedBy;
                }
            }
            else
            {
                // Insert new
                _context.ReviewGroupMembers.Add(new ReviewGroupMember
                {
                    Name = dto.Name,
                    Email = dto.Email,
                    IsActive = dto.IsActive,
                    CreatedAt = now,
                    CreatedBy = request.SavedBy,
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
