using System.Text.Json.Serialization;

namespace Kanata.Packaging;

/// <summary>
/// Represents one file stored in package payload blocks.
/// </summary>
public sealed class KpkgFileEntry
{
    /// <summary>
    /// Gets the canonical package path.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Gets the payload block id containing the file data.
    /// </summary>
    [JsonPropertyName("payloadBlockId")]
    public uint PayloadBlockId { get; init; }

    /// <summary>
    /// Gets the byte offset inside the payload block.
    /// </summary>
    [JsonPropertyName("payloadOffset")]
    public ulong PayloadOffset { get; init; }

    /// <summary>
    /// Gets the stored byte length inside the payload block.
    /// </summary>
    [JsonPropertyName("storedLength")]
    public ulong StoredLength { get; init; }

    /// <summary>
    /// Gets the uncompressed byte length.
    /// </summary>
    [JsonPropertyName("length")]
    public ulong Length { get; init; }

    /// <summary>
    /// Gets the file compression algorithm name.
    /// </summary>
    [JsonPropertyName("compression")]
    public string Compression { get; init; } = "none";

    /// <summary>
    /// Gets the SHA-256 hash of the uncompressed file bytes.
    /// </summary>
    [JsonPropertyName("sha256")]
    public string Sha256 { get; init; } = string.Empty;
}
