using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Api.Models.Audit;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.ReadOnlyViewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.Auditor, AuthorizationRole.AuditAdmin, AuthorizationRole.Executive)]
public class GetAudit : IRequest<AuditDetailDto?>
{
    public int AuditId { get; set; }
    public string RequestedBy { get; set; } = null!;
}

public class GetAuditHandler : IRequestHandler<GetAudit, AuditDetailDto?>
{
    private readonly AppDbContext _context;

    public GetAuditHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AuditDetailDto?> Handle(GetAudit request, CancellationToken cancellationToken)
    {
        var audit = await _context.Audits
            .Include(a => a.Division)
            .Include(a => a.Header)
            .Include(a => a.Responses)
            .Include(a => a.EnabledSections)
            .FirstOrDefaultAsync(a => a.Id == request.AuditId && !a.IsDeleted, cancellationToken);

        if (audit == null) return null;

        return new AuditDetailDto
        {
            Id = audit.Id,
            DivisionId = audit.DivisionId,
            DivisionCode = audit.Division.Code,
            DivisionName = audit.Division.Name,
            TemplateVersionId = audit.TemplateVersionId,
            AuditType = audit.AuditType,
            Status = audit.Status,
            CreatedAt = audit.CreatedAt,
            CreatedBy = audit.CreatedBy,
            SubmittedAt = audit.SubmittedAt,
            Header = audit.Header == null ? null : new AuditHeaderDto
            {
                Id = audit.Header.Id,
                JobNumber = audit.Header.JobNumber,
                Client = audit.Header.Client,
                PM = audit.Header.PM,
                Unit = audit.Header.Unit,
                Time = audit.Header.Time,
                Shift = audit.Header.Shift,
                WorkDescription = audit.Header.WorkDescription,
                Company1 = audit.Header.Company1,
                Company2 = audit.Header.Company2,
                Company3 = audit.Header.Company3,
                ResponsibleParty = audit.Header.ResponsibleParty,
                Location = audit.Header.Location,
                AuditDate = audit.Header.AuditDate?.ToString("yyyy-MM-dd"),
                Auditor = audit.Header.Auditor,
                SiteCode = audit.Header.SiteCode
            },
            Responses = audit.Responses.Select(r => new AuditResponseDto
            {
                Id = r.Id,
                QuestionId = r.QuestionId,
                QuestionTextSnapshot = r.QuestionTextSnapshot,
                Status = r.Status,
                Comment = r.Comment,
                CorrectedOnSite = r.CorrectedOnSite
            }).ToList(),
            EnabledOptionalGroupKeys = audit.EnabledSections.Select(s => s.OptionalGroupKey).ToList(),
            TrackingNumber = audit.TrackingNumber
        };
    }
}
