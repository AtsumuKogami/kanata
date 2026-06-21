namespace Kanata.Build.Components;

internal sealed class ResolvedComponent
{
    public required ComponentSource Source { get; init; }

    public required bool IsRequestedDirectly { get; init; }
}
