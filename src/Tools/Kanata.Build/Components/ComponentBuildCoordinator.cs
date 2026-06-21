using Kanata.Build.Commands;

namespace Kanata.Build.Components;

internal sealed class ComponentBuildCoordinator
{
    public async Task<IReadOnlyList<ComponentBuildResult>> EnsureTargetComponentsAsync(
        TargetBuildContext context,
        bool force,
        CancellationToken cancellationToken = default)
    {
        var repositoryRoot = EngineRepositoryLocator.FindRepositoryRoot();
        var registry = new BundledComponentRegistry(repositoryRoot);
        var components = await registry.LoadAsync(cancellationToken).ConfigureAwait(false);
        var builder = new ComponentBuilder(repositoryRoot, components);
        var componentIds = TargetComponentSelector.Select(context.Project, context.Target);

        return await builder
            .EnsureBuiltAsync(componentIds, context.Configuration, force, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ComponentBuildResult>> EnsureAllBundledComponentsAsync(
        string configuration,
        bool force,
        CancellationToken cancellationToken = default)
    {
        var repositoryRoot = EngineRepositoryLocator.FindRepositoryRoot();
        var registry = new BundledComponentRegistry(repositoryRoot);
        var components = await registry.LoadAsync(cancellationToken).ConfigureAwait(false);
        var builder = new ComponentBuilder(repositoryRoot, components);

        return await builder
            .EnsureBuiltAsync(components.Keys, configuration, force, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ComponentReference>> GetTargetReferencesAsync(
        TargetBuildContext context,
        CancellationToken cancellationToken = default)
    {
        var repositoryRoot = EngineRepositoryLocator.FindRepositoryRoot();
        var registry = new BundledComponentRegistry(repositoryRoot);
        var components = await registry.LoadAsync(cancellationToken).ConfigureAwait(false);
        var builder = new ComponentBuilder(repositoryRoot, components);
        var componentIds = TargetComponentSelector.Select(context.Project, context.Target);

        return builder.GetCachedReferences(componentIds, context.Configuration);
    }
}
