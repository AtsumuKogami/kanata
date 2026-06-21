using System.Text.Json.Serialization;

namespace Kanata.ProjectSystem.ProjectModel;

/// <summary>
/// Represents the root model of a Kanata project file.
/// </summary>
public sealed class KanataProject
{
    /// <summary>
    /// Gets the optional JSON schema URL used by editors.
    /// </summary>
    [JsonPropertyName("$schema")]
    public string? Schema { get; init; }

    /// <summary>
    /// Gets the project file format identifier.
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; init; } = string.Empty;

    /// <summary>
    /// Gets the project file schema version.
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; }

    /// <summary>
    /// Gets the stable project identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the human-readable project name.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional version of the game project.
    /// </summary>
    [JsonPropertyName("projectVersion")]
    public string? ProjectVersion { get; init; }

    /// <summary>
    /// Gets the Kanata SDK version requested by the project.
    /// </summary>
    [JsonPropertyName("kanataVersion")]
    public string KanataVersion { get; init; } = string.Empty;

    /// <summary>
    /// Gets the main workspace paths used by the project.
    /// </summary>
    [JsonPropertyName("paths")]
    public KanataPaths Paths { get; init; } = new();

    /// <summary>
    /// Gets the C# source projects used by the game.
    /// </summary>
    [JsonPropertyName("source")]
    public KanataSourceProjects Source { get; init; } = new();

    /// <summary>
    /// Gets the requested high-level engine features.
    /// </summary>
    [JsonPropertyName("features")]
    public List<string> Features { get; init; } = new();

    /// <summary>
    /// Gets the configured build targets.
    /// </summary>
    [JsonPropertyName("targets")]
    public Dictionary<string, KanataTarget> Targets { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the startup settings used when running the game.
    /// </summary>
    [JsonPropertyName("start")]
    public KanataStartSettings Start { get; init; } = new();
}
