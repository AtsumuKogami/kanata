using System.Text.Json.Serialization;

namespace Kanata.ProjectSystem.ProjectModel;

/// <summary>
/// Describes the source projects that make up game code.
/// </summary>
public sealed class KanataSourceProjects
{
    /// <summary>
    /// Gets the project containing shared contracts and protocol types.
    /// </summary>
    [JsonPropertyName("shared")]
    public string Shared { get; init; } = string.Empty;

    /// <summary>
    /// Gets the project containing simulation and gameplay logic.
    /// </summary>
    [JsonPropertyName("logic")]
    public string Logic { get; init; } = string.Empty;

    /// <summary>
    /// Gets the project containing client-side presentation code.
    /// </summary>
    [JsonPropertyName("view")]
    public string View { get; init; } = string.Empty;
}
