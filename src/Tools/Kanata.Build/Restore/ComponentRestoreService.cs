using Kanata.Build.Commands;
using Kanata.Build.Components;

namespace Kanata.Build.Restore;

internal sealed class ComponentRestoreService
{
    public async Task<ComponentRestoreResult> RestoreAsync(
        TargetBuildContext context,
        bool forceComponents,
        CancellationToken cancellationToken = default)
    {
        var coordinator = new ComponentBuildCoordinator();
        var buildResults = await coordinator
            .EnsureTargetComponentsAsync(context, forceComponents, cancellationToken)
            .ConfigureAwait(false);

        var componentReferences = buildResults
            .Select(result => result.Reference)
            .ToArray();

        var lockWriter = new ComponentLockWriter();
        var lockFilePath = await lockWriter
            .WriteAsync(context, componentReferences, cancellationToken)
            .ConfigureAwait(false);

        return new ComponentRestoreResult
        {
            LockFilePath = lockFilePath,
            ComponentReferences = componentReferences,
            BuiltCount = buildResults.Count(result => result.WasBuilt),
            CachedCount = buildResults.Count(result => result.WasSkipped),
        };
    }
}
