namespace Kanata.Build.Components;

internal sealed class ComponentResolver
{
    private readonly IReadOnlyDictionary<string, ComponentSource> _components;

    public ComponentResolver(IReadOnlyDictionary<string, ComponentSource> components)
    {
        _components = components;
    }

    public IReadOnlyList<ResolvedComponent> Resolve(IEnumerable<string> requestedComponentIds)
    {
        var requested = requestedComponentIds
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var result = new List<ResolvedComponent>();
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var direct = requested.ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var componentId in requested)
        {
            Visit(componentId, direct, visiting, visited, result);
        }

        return result;
    }

    private void Visit(
        string componentId,
        HashSet<string> direct,
        HashSet<string> visiting,
        HashSet<string> visited,
        List<ResolvedComponent> result)
    {
        if (visited.Contains(componentId))
        {
            return;
        }

        if (!visiting.Add(componentId))
        {
            throw new InvalidOperationException($"Component dependency cycle detected at {componentId}.");
        }

        if (!_components.TryGetValue(componentId, out var source))
        {
            throw new InvalidOperationException($"Component '{componentId}' is not available in the current Kanata source repository.");
        }

        foreach (var dependency in source.Dependencies)
        {
            Visit(dependency, direct, visiting, visited, result);
        }

        visiting.Remove(componentId);
        visited.Add(componentId);
        result.Add(new ResolvedComponent
        {
            Source = source,
            IsRequestedDirectly = direct.Contains(componentId),
        });
    }
}
