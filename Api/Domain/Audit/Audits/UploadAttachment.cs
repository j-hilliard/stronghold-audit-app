using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator,
    AuthorizationRole.Auditor, AuthorizationRole.AuditAdmin)]
public class UploadAttachment : IRequest<AuditAttachmentDto>
{
    public int AuditId { get; set; }
    public string FileName { get; set; } = null!;
    public byte[] FileData { get; set; } = null!;
    public string UploadedBy { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
}

public class UploadAttachmentHandler : IRequestHandler<UploadAttachment, AuditAttachmentDto>
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".png", ".jpg", ".jpeg", ".heic", ".gif", ".mp4", ".mov"
    };

    private const long MaxFileSizeBytes = 25 * 1024 * 1024; // 25 MB

    public UploadAttachmentHandler(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<AuditAttachmentDto> Handle(UploadAttachment request, CancellationToken cancellationToken)
    {
        // Validate audit exists
        var auditExists = await _context.Audits.AnyAsync(a => a.Id == request.AuditId, cancellationToken);
        if (!auditExists)
            throw new ArgumentException($"Audit {request.AuditId} not found.");

        var ext = Path.GetExtension(request.FileName);
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"File type '{ext}' is not allowed.");

        if (request.FileData.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("File exceeds the 25 MB limit.");

        var basePath = _config.GetValue<string>("Attachments:BasePath")
            ?? Path.Combine(Path.GetTempPath(), "stronghold-attachments");

        var auditDir = Path.Combine(basePath, request.AuditId.ToString());
        Directory.CreateDirectory(auditDir);

        // Use a unique filename to avoid collisions
        var storedName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(auditDir, storedName);

        await File.WriteAllBytesAsync(fullPath, request.FileData, cancellationToken);

        var now = DateTime.UtcNow;
        var attachment = new AuditAttachment
        {
            AuditId = request.AuditId,
            FileName = request.FileName,       // original user-facing name
            BlobPath = fullPath,               // actual stored path (Phase 1 = local FS)
            FileSizeBytes = request.FileData.Length,
            UploadedAt = now,
            UploadedBy = request.UploadedBy,
            CreatedAt = now,
            CreatedBy = request.UploadedBy,
        };

        _context.AuditAttachments.Add(attachment);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuditAttachmentDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            UploadedBy = attachment.UploadedBy,
            UploadedAt = attachment.UploadedAt,
            FileSizeBytes = attachment.FileSizeBytes,
            DownloadUrl = $"{request.BaseUrl}/v1/audits/{request.AuditId}/attachments/{attachment.Id}/download",
        };
    }
}
