using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

public class AttachmentDownloadResult
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = "application/octet-stream";
}

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.ReadOnlyViewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class DownloadAttachment : IRequest<AttachmentDownloadResult>
{
    public int AuditId { get; set; }
    public int AttachmentId { get; set; }
}

public class DownloadAttachmentHandler : IRequestHandler<DownloadAttachment, AttachmentDownloadResult>
{
    private readonly AppDbContext _context;

    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".pdf",  "application/pdf" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".doc",  "application/msword" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".xls",  "application/vnd.ms-excel" },
        { ".png",  "image/png" },
        { ".jpg",  "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".heic", "image/heic" },
        { ".gif",  "image/gif" },
        { ".mp4",  "video/mp4" },
        { ".mov",  "video/quicktime" },
    };

    public DownloadAttachmentHandler(AppDbContext context) => _context = context;

    public async Task<AttachmentDownloadResult> Handle(DownloadAttachment request, CancellationToken cancellationToken)
    {
        var attachment = await _context.AuditAttachments
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId && a.AuditId == request.AuditId, cancellationToken)
            ?? throw new KeyNotFoundException($"Attachment {request.AttachmentId} not found on audit {request.AuditId}.");

        var path = attachment.BlobPath;
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            throw new FileNotFoundException("Attachment file not found on server.");

        var ext = Path.GetExtension(attachment.FileName);
        var contentType = ContentTypes.TryGetValue(ext, out var ct) ? ct : "application/octet-stream";

        return new AttachmentDownloadResult
        {
            FileStream = File.OpenRead(path),
            FileName = attachment.FileName,
            ContentType = contentType,
        };
    }
}
