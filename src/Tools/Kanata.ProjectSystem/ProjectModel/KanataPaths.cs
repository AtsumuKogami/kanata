using System.Text.Json.Serialization;

namespace Kanata.ProjectSystem.ProjectModel;

/// <summary>
/// Describes standard folders in a Kanata project workspace.
/// </summary>
public sealed class KanataPaths
{
    /// <summary>
    /// Gets the content folder path.
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; init; }

    /// <summary>
    /// Gets the source folder path.
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; init; }

    /// <summary>
    /// Gets the generated files folder path.
    /// </summary>
    [JsonPropertyName("generated")]
    public string? Generated { get; init; }

    /// <summary>
    /// Gets the project settings folder path.
    /// </summary>
    [JsonPropertyName("settings")]
    public string? Settings { get; init; }
}
