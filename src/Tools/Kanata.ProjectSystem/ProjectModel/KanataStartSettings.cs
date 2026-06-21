using System.Text.Json.Serialization;

namespace Kanata.ProjectSystem.ProjectModel;

/// <summary>
/// Describes startup content for a Kanata project.
/// </summary>
public sealed class KanataStartSettings
{
    /// <summary>
    /// Gets the startup scene path.
    /// </summary>
    [JsonPropertyName("scene")]
    public string? Scene { get; init; }
}
