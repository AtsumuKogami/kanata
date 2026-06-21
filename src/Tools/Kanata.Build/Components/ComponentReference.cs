namespace Kanata.Build.Components;

internal sealed class ComponentReference
{
    public required string Id { get; init; }

    public required string Version { get; init; }

    public required string AssemblyName { get; init; }

    public required string AssemblyPath { get; init; }
}
