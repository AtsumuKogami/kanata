using Kanata.ProjectSystem.ComponentModel;

namespace Kanata.Build.Components;

internal sealed class ComponentSource
{
    public required string Id { get; init; }

    public required string Version { get; init; }

    public required string Kind { get; init; }

    public required string ManifestPath { get; init; }

    public required string ComponentRoot { get; init; }

    public required string ProjectPath { get; init; }

    public required string AssemblyName { get; init; }

    public required string TargetFramework { get; init; }

    public IReadOnlyList<string> Dependencies { get; init; } = [];

    public required KanataComponentManifest Manifest { get; init; }
}
