using System.Text.Json.Serialization;

namespace Kanata.Packaging;

/// <summary>
/// Represents the payload file table block stored inside a Kanata package.
/// </summary>
public sealed class KpkgFileTable
{
    /// <summary>
    /// Gets the file table format identifier.
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; init; } = string.Empty;

    /// <summary>
    /// Gets the file table schema version.
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; }

    /// <summary>
    /// Gets the payload file entries.
    /// </summary>
    [JsonPropertyName("files")]
    public IReadOnlyList<KpkgFileEntry> Files { get; init; } = [];
}
