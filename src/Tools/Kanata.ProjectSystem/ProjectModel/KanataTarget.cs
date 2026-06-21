using System.Text.Json.Serialization;

namespace Kanata.ProjectSystem.ProjectModel;

/// <summary>
/// Describes a concrete build target for a Kanata project.
/// </summary>
public sealed class KanataTarget
{
    /// <summary>
    /// Gets the logical platform name.
    /// </summary>
    [JsonPropertyName("platform")]
    public string? Platform { get; init; }

    /// <summary>
    /// Gets the backend provider identifier.
    /// </summary>
    [JsonPropertyName("backend")]
    public string? Backend { get; init; }

    /// <summary>
    /// Gets the host C# project path used for this target.
    /// </summary>
    [JsonPropertyName("hostProject")]
    public string? HostProject { get; init; }

    /// <summary>
    /// Gets the session mode used by this target.
    /// </summary>
    [JsonPropertyName("session")]
    public string? Session { get; init; }
}
