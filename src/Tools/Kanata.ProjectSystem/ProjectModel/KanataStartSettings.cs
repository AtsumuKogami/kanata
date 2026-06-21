using System.Text.Json.Serialization;

namespace Kanata.ProjectSystem.ProjectModel;

/// <summary>
/// Describes startup content used when launching a Kanata project.
/// </summary>
public sealed class KanataStartSettings
{
    /// <summary>
    /// Gets the scene loaded when the game starts.
    /// </summary>
    [JsonPropertyName("scene")]
    public string Scene { get; init; } = string.Empty;
}
