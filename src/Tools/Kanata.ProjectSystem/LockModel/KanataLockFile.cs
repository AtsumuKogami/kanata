using System.Text.Json.Serialization;

namespace Kanata.ProjectSystem.LockModel;

/// <summary>
/// Represents the resolved component graph for a Kanata project target.
/// </summary>
public sealed class KanataLockFile
{
    /// <summary>
    /// Gets the optional JSON schema URL.
    /// </summary>
    [JsonPropertyName("$schema")]
    public string? Schema { get; init; }

    /// <summary>
    /// Gets the lock file format identifier.
    /// </summary>
    [JsonPropertyName("format")]
    public required string Format { get; init; }

    /// <summary>
    /// Gets the lock file schema version.
    /// </summary>
    [JsonPropertyName("schemaVersion")]
    public required int SchemaVersion { get; init; }

    /// <summary>
    /// Gets the project identifier for which the lock was generated.
    /// </summary>
    [JsonPropertyName("projectId")]
    public required string ProjectId { get; init; }

    /// <summary>
    /// Gets the Kanata toolchain version that generated the lock file.
    /// </summary>
    [JsonPropertyName("kanataVersion")]
    public required string KanataVersion { get; init; }

    /// <summary>
    /// Gets the resolved target name.
    /// </summary>
    [JsonPropertyName("target")]
    public required string Target { get; init; }

    /// <summary>
    /// Gets the resolved build configuration.
    /// </summary>
    [JsonPropertyName("configuration")]
    public required string Configuration { get; init; }

    /// <summary>
    /// Gets the UTC time when the lock file was generated.
    /// </summary>
    [JsonPropertyName("generatedAtUtc")]
    public required DateTimeOffset GeneratedAtUtc { get; init; }

    /// <summary>
    /// Gets resolved components in dependency order.
    /// </summary>
    [JsonPropertyName("components")]
    public List<KanataLockedComponent> Components { get; init; } = [];
}
