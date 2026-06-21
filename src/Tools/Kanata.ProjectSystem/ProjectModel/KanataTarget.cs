using System.Text.Json.Serialization;

namespace Kanata.ProjectSystem.ProjectModel;

/// <summary>
/// Describes a concrete build target of a Kanata project.
/// </summary>
public sealed class KanataTarget
{
    /// <summary>
    /// Gets the logical platform name of the target.
    /// </summary>
    [JsonPropertyName("platform")]
    public string Platform { get; init; } = string.Empty;

    /// <summary>
    /// Gets the backend component used by the target.
    /// </summary>
    [JsonPropertyName("backend")]
    public string Backend { get; init; } = string.Empty;

    /// <summary>
    /// Gets the host C# project used to build or run the target.
    /// </summary>
    [JsonPropertyName("hostProject")]
    public string HostProject { get; init; } = string.Empty;

    /// <summary>
    /// Gets the session mode used by the target.
    /// </summary>
    [JsonPropertyName("session")]
    public string Session { get; init; } = string.Empty;
}
