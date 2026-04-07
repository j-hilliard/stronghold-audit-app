using System.Text.Json;

namespace Stronghold.AppDashboard.Api.Domain.Audit.ReportDrafts;

/// <summary>
/// Validates the BlocksJson payload before it is persisted.
/// The backend treats blocks as an opaque string — this only rejects
/// corrupt or oversized payloads early, before a DB write.
/// </summary>
internal static class BlocksJsonValidator
{
    private const int MaxBytes = 1_048_576; // 1 MB

    public static void Validate(string? blocksJson)
    {
        if (string.IsNullOrWhiteSpace(blocksJson))
            throw new ArgumentException("BlocksJson must not be empty.");

        if (blocksJson.Length > MaxBytes)
            throw new ArgumentException("BlocksJson exceeds the maximum allowed size (1 MB).");

        try
        {
            using var _ = JsonDocument.Parse(blocksJson);
        }
        catch (JsonException)
        {
            throw new ArgumentException("BlocksJson is not valid JSON.");
        }
    }
}
