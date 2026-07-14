using System.Text.Json.Serialization;

namespace Kanata.Packaging;

/// <summary>
/// Represents the package manifest block stored inside a Kanata package.
/// </summary>
public sealed class KpkgPackageManifest
{
    /// <summary>
    /// Gets the manifest format identifier.
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; init; } = string.Empty;

    /// <summary>
    /// Gets the manifest schema version.
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; }

    /// <summary>
    /// Gets the stable package id.
    /// </summary>
    [JsonPropertyName("packageId")]
    public string PackageId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the package version.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional display name.
    /// </summary>
    [JsonPropertyName("displayName")]
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets the optional package description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Gets the installables index entries contained in the package.
    /// </summary>
    [JsonPropertyName("installables")]
    public IReadOnlyList<KpkgInstallableIndexEntry> Installables { get; init; } = [];
}
