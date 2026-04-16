using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

public class AuditAttachmentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = null!;
    public string UploadedBy { get; set; } = null!;
    public DateTime UploadedAt { get; set; }
    public long FileSizeBytes { get; set; }
    public string DownloadUrl { get; set; } = null!;
}

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.ReadOnlyViewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class GetAttachments : IRequest<List<AuditAttachmentDto>>
{
    public int AuditId { get; set; }
    public string BaseUrl { get; set; } = null!;
}

public class GetAttachmentsHandler : IRequestHandler<GetAttachments, List<AuditAttachmentDto>>
{
    private readonly AppDbContext _context;

    public GetAttachmentsHandler(AppDbContext context) => _context = context;

    public async Task<List<AuditAttachmentDto>> Handle(GetAttachments request, CancellationToken cancellationToken)
    {
        return await _context.AuditAttachments
            .Where(a => a.AuditId == request.AuditId)
            .OrderByDescending(a => a.UploadedAt)
            .Select(a => new AuditAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                UploadedBy = a.UploadedBy,
                UploadedAt = a.UploadedAt,
                FileSizeBytes = a.FileSizeBytes,
                DownloadUrl = $"{request.BaseUrl}/v1/audits/{a.AuditId}/attachments/{a.Id}/download",
            })
            .ToListAsync(cancellationToken);
    }
}
