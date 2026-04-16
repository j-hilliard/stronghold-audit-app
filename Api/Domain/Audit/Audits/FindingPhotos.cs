using MediatR;
using Microsoft.EntityFrameworkCore;
using Stronghold.AppDashboard.Api.Authorization;
using Stronghold.AppDashboard.Data;
using Stronghold.AppDashboard.Data.Models.Audit;
using Stronghold.AppDashboard.Shared.Enumerations;

namespace Stronghold.AppDashboard.Api.Domain.Audit.Audits;

// ── DTO ────────────────────────────────────────────────────────────────────────

public class FindingPhotoDto
{
    public int Id { get; set; }
    public int AuditId { get; set; }
    public int QuestionId { get; set; }
    public string FileName { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; }
    public string UploadedBy { get; set; } = null!;
    public string? Caption { get; set; }
    /// <summary>Convenience URL for the download endpoint — populated server-side.</summary>
    public string DownloadUrl { get; set; } = null!;
}

// ── Get ────────────────────────────────────────────────────────────────────────

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.ReadOnlyViewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class GetFindingPhotos : IRequest<List<FindingPhotoDto>>
{
    public int AuditId { get; set; }
    public int QuestionId { get; set; }
    public string BaseUrl { get; set; } = null!;
}

public class GetFindingPhotosHandler : IRequestHandler<GetFindingPhotos, List<FindingPhotoDto>>
{
    private readonly AppDbContext _db;
    public GetFindingPhotosHandler(AppDbContext db) => _db = db;

    public async Task<List<FindingPhotoDto>> Handle(GetFindingPhotos request, CancellationToken ct)
    {
        return await _db.FindingPhotos
            .Where(p => p.AuditId == request.AuditId && p.QuestionId == request.QuestionId)
            .OrderBy(p => p.UploadedAt)
            .Select(p => new FindingPhotoDto
            {
                Id = p.Id,
                AuditId = p.AuditId,
                QuestionId = p.QuestionId,
                FileName = p.FileName,
                FileSizeBytes = p.FileSizeBytes,
                UploadedAt = p.UploadedAt,
                UploadedBy = p.UploadedBy,
                Caption = p.Caption,
                DownloadUrl = $"{request.BaseUrl}/v1/audits/{p.AuditId}/questions/{p.QuestionId}/photos/{p.Id}/download",
            })
            .ToListAsync(ct);
    }
}

// ── Upload ─────────────────────────────────────────────────────────────────────

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class UploadFindingPhoto : IRequest<FindingPhotoDto>
{
    public int AuditId { get; set; }
    public int QuestionId { get; set; }
    public string FileName { get; set; } = null!;
    public byte[] FileData { get; set; } = null!;
    public string? Caption { get; set; }
    public string UploadedBy { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
}

public class UploadFindingPhotoHandler : IRequestHandler<UploadFindingPhoto, FindingPhotoDto>
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".heic", ".gif", ".webp", ".bmp"
    };
    private const long MaxBytes = 25 * 1024 * 1024; // 25 MB

    public UploadFindingPhotoHandler(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<FindingPhotoDto> Handle(UploadFindingPhoto request, CancellationToken ct)
    {
        var ext = Path.GetExtension(request.FileName);
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"File type '{ext}' is not allowed. Accepted: jpg, jpeg, png, heic, gif, webp, bmp.");

        if (request.FileData.Length > MaxBytes)
            throw new InvalidOperationException("Photo exceeds the 25 MB limit.");

        // Validate audit + question exist
        var auditExists = await _db.Audits.AnyAsync(a => a.Id == request.AuditId, ct);
        if (!auditExists) throw new KeyNotFoundException($"Audit {request.AuditId} not found.");

        var questionExists = await _db.AuditQuestions.AnyAsync(q => q.Id == request.QuestionId, ct);
        if (!questionExists) throw new KeyNotFoundException($"Question {request.QuestionId} not found.");

        var basePath = _config.GetValue<string>("Attachments:BasePath")
            ?? Path.Combine(Path.GetTempPath(), "stronghold-attachments");

        var photoDir = Path.Combine(basePath, "photos", request.AuditId.ToString(), request.QuestionId.ToString());
        Directory.CreateDirectory(photoDir);

        var storedName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(photoDir, storedName);
        await File.WriteAllBytesAsync(filePath, request.FileData, ct);

        var now = DateTime.UtcNow;
        var photo = new FindingPhoto
        {
            AuditId = request.AuditId,
            QuestionId = request.QuestionId,
            FileName = request.FileName,
            FilePath = filePath,
            FileSizeBytes = request.FileData.Length,
            UploadedAt = now,
            UploadedBy = request.UploadedBy,
            Caption = request.Caption,
            CreatedAt = now,
            CreatedBy = request.UploadedBy,
        };

        _db.FindingPhotos.Add(photo);
        await _db.SaveChangesAsync(ct);

        return new FindingPhotoDto
        {
            Id = photo.Id,
            AuditId = photo.AuditId,
            QuestionId = photo.QuestionId,
            FileName = photo.FileName,
            FileSizeBytes = photo.FileSizeBytes,
            UploadedAt = photo.UploadedAt,
            UploadedBy = photo.UploadedBy,
            Caption = photo.Caption,
            DownloadUrl = $"{request.BaseUrl}/v1/audits/{photo.AuditId}/questions/{photo.QuestionId}/photos/{photo.Id}/download",
        };
    }
}

// ── Download ───────────────────────────────────────────────────────────────────

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.AuditReviewer,
    AuthorizationRole.CorrectiveActionOwner, AuthorizationRole.ReadOnlyViewer,
    AuthorizationRole.ExecutiveViewer, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class DownloadFindingPhoto : IRequest<DownloadFindingPhotoResult>
{
    public int AuditId { get; set; }
    public int QuestionId { get; set; }
    public int PhotoId { get; set; }
}

public class DownloadFindingPhotoResult
{
    public Stream FileStream { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public string FileName { get; set; } = null!;
}

public class DownloadFindingPhotoHandler : IRequestHandler<DownloadFindingPhoto, DownloadFindingPhotoResult>
{
    private readonly AppDbContext _db;
    public DownloadFindingPhotoHandler(AppDbContext db) => _db = db;

    public async Task<DownloadFindingPhotoResult> Handle(DownloadFindingPhoto request, CancellationToken ct)
    {
        var photo = await _db.FindingPhotos
            .FirstOrDefaultAsync(p => p.Id == request.PhotoId
                                   && p.AuditId == request.AuditId
                                   && p.QuestionId == request.QuestionId, ct)
            ?? throw new KeyNotFoundException($"Photo {request.PhotoId} not found.");

        if (!File.Exists(photo.FilePath))
            throw new FileNotFoundException($"Photo file not found on server.", photo.FilePath);

        var ext = Path.GetExtension(photo.FileName).TrimStart('.').ToLowerInvariant();
        var contentType = ext switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "gif" => "image/gif",
            "webp" => "image/webp",
            "heic" => "image/heic",
            "bmp" => "image/bmp",
            _ => "application/octet-stream",
        };

        return new DownloadFindingPhotoResult
        {
            FileStream = File.OpenRead(photo.FilePath),
            ContentType = contentType,
            FileName = photo.FileName,
        };
    }
}

// ── Delete ─────────────────────────────────────────────────────────────────────

[AllowedAuthorizationRole(
    AuthorizationRole.AuditManager, AuthorizationRole.TemplateAdmin,
    AuthorizationRole.Administrator)]
public class DeleteFindingPhoto : IRequest<Unit>
{
    public int AuditId { get; set; }
    public int QuestionId { get; set; }
    public int PhotoId { get; set; }
}

public class DeleteFindingPhotoHandler : IRequestHandler<DeleteFindingPhoto, Unit>
{
    private readonly AppDbContext _db;
    public DeleteFindingPhotoHandler(AppDbContext db) => _db = db;

    public async Task<Unit> Handle(DeleteFindingPhoto request, CancellationToken ct)
    {
        var photo = await _db.FindingPhotos
            .FirstOrDefaultAsync(p => p.Id == request.PhotoId
                                   && p.AuditId == request.AuditId
                                   && p.QuestionId == request.QuestionId, ct)
            ?? throw new KeyNotFoundException($"Photo {request.PhotoId} not found.");

        // Best-effort file deletion — don't fail if file already missing
        if (File.Exists(photo.FilePath))
        {
            try { File.Delete(photo.FilePath); } catch { /* log in production */ }
        }

        _db.FindingPhotos.Remove(photo);
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
