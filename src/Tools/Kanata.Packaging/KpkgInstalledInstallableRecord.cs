using System.Text.Json.Serialization;

namespace Kanata.Packaging;

/// <summary>
/// Represents one installed installable entry inside an installed package record.
/// </summary>
public sealed class KpkgInstalledInstallableRecord
{
    /// <summary>
    /// Gets the installable id.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installable version.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installable kind.
    /// </summary>
    [JsonPropertyName("kind")]
    public string Kind { get; init; } = string.Empty;
}
