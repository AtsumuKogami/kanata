using System.Text.Json.Serialization;

namespace Kanata.Packaging;

/// <summary>
/// Represents a base metadata snapshot for one installable in a package manifest.
/// </summary>
public sealed class KpkgInstallableIndexEntry
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

    /// <summary>
    /// Gets the optional installable description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// Gets capability ids provided by the installable.
    /// </summary>
    [JsonPropertyName("provides")]
    public IReadOnlyList<string> Provides { get; init; } = [];

    /// <summary>
    /// Gets compatibility metadata used before installation.
    /// </summary>
    [JsonPropertyName("compatibility")]
    public KpkgCompatibility? Compatibility { get; init; }

    /// <summary>
    /// Gets explicit game graph participation metadata.
    /// </summary>
    [JsonPropertyName("gameParticipation")]
    public KpkgGameParticipation? GameParticipation { get; init; }

    /// <summary>
    /// Gets the descriptor block id containing the full typed descriptor.
    /// </summary>
    [JsonPropertyName("descriptorBlockId")]
    public uint DescriptorBlockId { get; init; }
}
