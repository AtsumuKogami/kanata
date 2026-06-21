using Kanata.Build.Components;

namespace Kanata.Build.Restore;

internal sealed class ComponentRestoreResult
{
    public required string LockFilePath { get; init; }

    public required IReadOnlyList<ComponentReference> ComponentReferences { get; init; }

    public required int BuiltCount { get; init; }

    public required int CachedCount { get; init; }
}
