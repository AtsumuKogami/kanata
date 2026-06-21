using System.Text.Json.Serialization;

namespace Kanata.Build.Components;

internal sealed class ComponentBuildInfo
{
    [JsonPropertyName("componentId")]
    public required string ComponentId { get; init; }

    [JsonPropertyName("version")]
    public required string Version { get; init; }

    [JsonPropertyName("configuration")]
    public required string Configuration { get; init; }

    [JsonPropertyName("fingerprint")]
    public required string Fingerprint { get; init; }

    [JsonPropertyName("assemblyPath")]
    public required string AssemblyPath { get; init; }

    [JsonPropertyName("builtAtUtc")]
    public required DateTimeOffset BuiltAtUtc { get; init; }
}
