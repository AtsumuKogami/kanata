using System.Text.Json.Serialization;

namespace Kanata.ProjectSystem.LockModel;

/// <summary>
/// Represents one component selected for a Kanata target build.
/// </summary>
public sealed class KanataLockedComponent
{
    /// <summary>
    /// Gets the component identifier.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Gets the resolved component version.
    /// </summary>
    [JsonPropertyName("version")]
    public required string Version { get; init; }

    /// <summary>
    /// Gets the component kind.
    /// </summary>
    [JsonPropertyName("kind")]
    public required string Kind { get; init; }

    /// <summary>
    /// Gets the component source type.
    /// </summary>
    [JsonPropertyName("source")]
    public required string Source { get; init; }

    /// <summary>
    /// Gets the component target framework.
    /// </summary>
    [JsonPropertyName("targetFramework")]
    public required string TargetFramework { get; init; }

    /// <summary>
    /// Gets the component assembly name.
    /// </summary>
    [JsonPropertyName("assemblyName")]
    public required string AssemblyName { get; init; }

    /// <summary>
    /// Gets the resolved component assembly path.
    /// </summary>
    [JsonPropertyName("assemblyPath")]
    public required string AssemblyPath { get; init; }

    /// <summary>
    /// Gets the source manifest path used for this component.
    /// </summary>
    [JsonPropertyName("manifestPath")]
    public string? ManifestPath { get; init; }

    /// <summary>
    /// Gets component dependencies by identifier.
    /// </summary>
    [JsonPropertyName("dependencies")]
    public List<string> Dependencies { get; init; } = [];
}
