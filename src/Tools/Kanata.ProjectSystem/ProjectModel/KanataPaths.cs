using System.Text.Json.Serialization;

namespace Kanata.ProjectSystem.ProjectModel;

/// <summary>
/// Describes the main folders of a Kanata workspace.
/// </summary>
public sealed class KanataPaths
{
    /// <summary>
    /// Gets the folder containing game content files.
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Gets the folder containing game source projects.
    /// </summary>
    [JsonPropertyName("source")]
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Gets the folder containing generated build files and caches.
    /// </summary>
    [JsonPropertyName("generated")]
    public string Generated { get; init; } = string.Empty;

    /// <summary>
    /// Gets the folder containing project-level settings.
    /// </summary>
    [JsonPropertyName("settings")]
    public string Settings { get; init; } = string.Empty;
}
