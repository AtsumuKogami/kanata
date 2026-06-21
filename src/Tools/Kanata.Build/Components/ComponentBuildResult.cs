namespace Kanata.Build.Components;

internal sealed class ComponentBuildResult
{
    public required ComponentReference Reference { get; init; }

    public required bool WasBuilt { get; init; }

    public required bool WasSkipped { get; init; }
}
