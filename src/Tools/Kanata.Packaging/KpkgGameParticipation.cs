using System.Text.Json.Serialization;

namespace Kanata.Packaging;

/// <summary>
/// Represents explicit game build/runtime graph participation flags.
/// </summary>
public sealed class KpkgGameParticipation
{
    /// <summary>
    /// Gets a value indicating whether the installable participates in the game build graph.
    /// </summary>
    [JsonPropertyName("build")]
    public bool Build { get; init; }

    /// <summary>
    /// Gets a value indicating whether the installable participates in the game runtime graph.
    /// </summary>
    [JsonPropertyName("runtime")]
    public bool Runtime { get; init; }
}
