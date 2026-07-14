using System.Text.Json.Serialization;

namespace Kanata.Packaging;

/// <summary>
/// Represents compatibility metadata used during package inspection and installation planning.
/// </summary>
public sealed class KpkgCompatibility
{
    /// <summary>
    /// Gets the supported Kanata Tool version range.
    /// </summary>
    [JsonPropertyName("kanataToolVersion")]
    public string? KanataToolVersion { get; init; }

    /// <summary>
    /// Gets supported platform identifiers.
    /// </summary>
    [JsonPropertyName("platforms")]
    public IReadOnlyList<string> Platforms { get; init; } = [];

    /// <summary>
    /// Gets supported architecture identifiers.
    /// </summary>
    [JsonPropertyName("architectures")]
    public IReadOnlyList<string> Architectures { get; init; } = [];
}
