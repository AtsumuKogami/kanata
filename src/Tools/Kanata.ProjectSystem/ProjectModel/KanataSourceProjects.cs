using System.Text.Json.Serialization;

namespace Kanata.ProjectSystem.ProjectModel;

/// <summary>
/// Describes C# source projects that belong to a Kanata game project.
/// </summary>
public sealed class KanataSourceProjects
{
    /// <summary>
    /// Gets the shared source project path.
    /// </summary>
    [JsonPropertyName("shared")]
    public string? Shared { get; init; }

    /// <summary>
    /// Gets the simulation or game logic source project path.
    /// </summary>
    [JsonPropertyName("logic")]
    public string? Logic { get; init; }

    /// <summary>
    /// Gets the client view source project path.
    /// </summary>
    [JsonPropertyName("view")]
    public string? View { get; init; }
}
