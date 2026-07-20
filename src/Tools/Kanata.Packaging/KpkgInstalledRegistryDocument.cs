using System.Text.Json.Serialization;

namespace Kanata.Packaging;

/// <summary>
/// Represents the local installed package registry document.
/// </summary>
public sealed class KpkgInstalledRegistryDocument
{
    /// <summary>
    /// Gets the registry format identifier.
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; init; } = "kanata.package.installedRegistry";

    /// <summary>
    /// Gets the registry schema version.
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; } = 1;

    /// <summary>
    /// Gets the installed package records.
    /// </summary>
    [JsonPropertyName("packages")]
    public IReadOnlyList<KpkgInstalledPackageRecord> Packages { get; init; } = [];
}
