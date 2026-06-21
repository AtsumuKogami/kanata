namespace Kanata.Build.Components;

internal sealed class ComponentReference
{
    public required string Id { get; init; }

    public required string Version { get; init; }

    public required string Kind { get; init; }

    public required string Source { get; init; }

    public required string TargetFramework { get; init; }

    public required string AssemblyName { get; init; }

    public required string AssemblyPath { get; init; }

    public string? ManifestPath { get; init; }

    public IReadOnlyList<string> Dependencies { get; init; } = [];
}
