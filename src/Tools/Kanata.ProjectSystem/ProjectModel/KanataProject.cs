using System.Text.Json.Serialization;

namespace Kanata.ProjectSystem.ProjectModel;

/// <summary>
/// Represents a Kanata project file loaded from disk.
/// </summary>
public sealed class KanataProject
{
    /// <summary>
    /// Gets the optional JSON schema reference used by editors.
    /// </summary>
    [JsonPropertyName("$schema")]
    public string? Schema { get; init; }

    /// <summary>
    /// Gets the project file format identifier.
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; init; }

    /// <summary>
    /// Gets the project file schema version.
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; }

    /// <summary>
    /// Gets the stable project identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the human-readable project name.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>
    /// Gets the game or application version.
    /// </summary>
    [JsonPropertyName("projectVersion")]
    public string? ProjectVersion { get; init; }

    /// <summary>
    /// Gets the Kanata SDK version expected by the project.
    /// </summary>
    [JsonPropertyName("kanataVersion")]
    public string? KanataVersion { get; init; }

    /// <summary>
    /// Gets project folder paths.
    /// </summary>
    [JsonPropertyName("paths")]
    public KanataPaths? Paths { get; init; }

    /// <summary>
    /// Gets source project references used by Kanata builds.
    /// </summary>
    [JsonPropertyName("source")]
    public KanataSourceProjects? Source { get; init; }

    /// <summary>
    /// Gets high-level features requested by the project.
    /// </summary>
    [JsonPropertyName("features")]
    public List<string> Features { get; init; } = [];

    /// <summary>
    /// Gets build targets declared by the project.
    /// </summary>
    [JsonPropertyName("targets")]
    public Dictionary<string, KanataTarget> Targets { get; init; } = [];

    /// <summary>
    /// Gets startup settings for the project.
    /// </summary>
    [JsonPropertyName("start")]
    public KanataStartSettings? Start { get; init; }
}
