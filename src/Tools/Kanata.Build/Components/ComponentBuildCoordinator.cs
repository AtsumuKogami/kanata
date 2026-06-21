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
        var requestedComponentIds = TargetComponentSelector.Select(context.Project, context.Target);
        var resolver = new ComponentResolver(components);
        var resolvedComponents = resolver.Resolve(requestedComponentIds);
        var builder = new ComponentBuilder(repositoryRoot);

        return await builder
            .EnsureBuiltAsync(resolvedComponents, context.Configuration, force, cancellationToken)
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
        var resolver = new ComponentResolver(components);
        var resolvedComponents = resolver.Resolve(components.Keys);
        var builder = new ComponentBuilder(repositoryRoot);

        return await builder
            .EnsureBuiltAsync(resolvedComponents, configuration, force, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ComponentReference>> GetTargetReferencesAsync(
        TargetBuildContext context,
        CancellationToken cancellationToken = default)
    {
        var repositoryRoot = EngineRepositoryLocator.FindRepositoryRoot();
        var registry = new BundledComponentRegistry(repositoryRoot);
        var components = await registry.LoadAsync(cancellationToken).ConfigureAwait(false);
        var requestedComponentIds = TargetComponentSelector.Select(context.Project, context.Target);
        var resolver = new ComponentResolver(components);
        var resolvedComponents = resolver.Resolve(requestedComponentIds);
        var builder = new ComponentBuilder(repositoryRoot);

        return builder.GetCachedReferences(resolvedComponents, context.Configuration);
    }
}
