using System.Text.Json.Serialization;

namespace Kanata.ProjectSystem.ComponentModel;

/// <summary>
/// Represents a Kanata component manifest loaded from a <c>.kcomponent</c> file.
/// </summary>
public sealed class KanataComponentManifest
{
    /// <summary>
    /// Gets the component manifest format identifier.
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; init; }

    /// <summary>
    /// Gets the component manifest schema version.
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; init; }

    /// <summary>
    /// Gets the stable component identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the component version or a version token resolved by Kanata.Build.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; init; }

    /// <summary>
    /// Gets the component kind, such as runtime or backend.
    /// </summary>
    [JsonPropertyName("kind")]
    public string? Kind { get; init; }

    /// <summary>
    /// Gets the relative path to the source project used to build this component.
    /// </summary>
    [JsonPropertyName("project")]
    public string? Project { get; init; }

    /// <summary>
    /// Gets the main assembly name produced by the component project.
    /// </summary>
    [JsonPropertyName("assemblyName")]
    public string? AssemblyName { get; init; }

    /// <summary>
    /// Gets the target framework used for component artifact discovery.
    /// </summary>
    [JsonPropertyName("targetFramework")]
    public string? TargetFramework { get; init; }

    /// <summary>
    /// Gets component identifiers that must be built before this component.
    /// </summary>
    [JsonPropertyName("dependencies")]
    public List<string> Dependencies { get; init; } = [];

    /// <summary>
    /// Gets a short human-readable component description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
