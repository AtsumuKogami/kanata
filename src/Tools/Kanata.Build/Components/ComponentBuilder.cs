using System.Text.Json;
using Kanata.Build.Infrastructure;

namespace Kanata.Build.Components;

internal sealed class ComponentBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    private readonly string _repositoryRoot;
    private readonly IReadOnlyDictionary<string, ComponentSource> _components;

    public ComponentBuilder(string repositoryRoot, IReadOnlyDictionary<string, ComponentSource> components)
    {
        _repositoryRoot = repositoryRoot;
        _components = components;
    }

    public async Task<IReadOnlyList<ComponentBuildResult>> EnsureBuiltAsync(
        IEnumerable<string> componentIds,
        string configuration,
        bool force,
        CancellationToken cancellationToken = default)
    {
        var orderedSources = ResolveBuildOrder(componentIds);
        var results = new List<ComponentBuildResult>();

        foreach (var source in orderedSources)
        {
            var result = await EnsureBuiltAsync(source, configuration, force, cancellationToken).ConfigureAwait(false);
            results.Add(result);
        }

        return results;
    }

    public IReadOnlyList<ComponentReference> GetCachedReferences(
        IEnumerable<string> componentIds,
        string configuration)
    {
        return ResolveBuildOrder(componentIds)
            .Select(source => CreateReference(source, configuration))
            .ToArray();
    }

    private async Task<ComponentBuildResult> EnsureBuiltAsync(
        ComponentSource source,
        string configuration,
        bool force,
        CancellationToken cancellationToken)
    {
        var paths = GetArtifactPaths(source, configuration);
        var fingerprint = ComponentFingerprint.Compute(source, configuration, _repositoryRoot);
        var reference = CreateReference(source, configuration);

        if (!force && IsUpToDate(paths, fingerprint))
        {
            Console.WriteLine($"component {source.Id} {source.Version} [{configuration}] is up to date");
            return new ComponentBuildResult
            {
                Reference = reference,
                WasBuilt = false,
                WasSkipped = true,
            };
        }

        Console.WriteLine($"building component {source.Id} {source.Version} [{configuration}]...");
        Directory.CreateDirectory(paths.LibRoot);

        var exitCode = await ProcessRunner.RunAsync(
            "dotnet",
            ["build", source.ProjectPath, "-c", configuration, "-f", source.TargetFramework, "-o", paths.LibRoot],
            _repositoryRoot,
            cancellationToken).ConfigureAwait(false);

        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Failed to build component {source.Id}.");
        }

        if (!File.Exists(paths.AssemblyPath))
        {
            throw new FileNotFoundException(
                $"Component build completed but expected assembly was not found: {paths.AssemblyPath}",
                paths.AssemblyPath);
        }

        Directory.CreateDirectory(paths.Root);
        File.Copy(source.ManifestPath, paths.ManifestCopyPath, overwrite: true);

        var buildInfo = new ComponentBuildInfo
        {
            ComponentId = source.Id,
            Version = source.Version,
            Configuration = configuration,
            Fingerprint = fingerprint,
            AssemblyPath = paths.AssemblyPath,
            BuiltAtUtc = DateTimeOffset.UtcNow,
        };

        var json = JsonSerializer.Serialize(buildInfo, JsonOptions);
        await File.WriteAllTextAsync(paths.BuildInfoPath, json, cancellationToken).ConfigureAwait(false);

        return new ComponentBuildResult
        {
            Reference = reference,
            WasBuilt = true,
            WasSkipped = false,
        };
    }

    private bool IsUpToDate(ComponentArtifactPaths paths, string fingerprint)
    {
        if (!File.Exists(paths.AssemblyPath) || !File.Exists(paths.BuildInfoPath))
        {
            return false;
        }

        try
        {
            var json = File.ReadAllText(paths.BuildInfoPath);
            var buildInfo = JsonSerializer.Deserialize<ComponentBuildInfo>(json);
            return string.Equals(buildInfo?.Fingerprint, fingerprint, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private IReadOnlyList<ComponentSource> ResolveBuildOrder(IEnumerable<string> componentIds)
    {
        var result = new List<ComponentSource>();
        var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var componentId in componentIds)
        {
            Visit(componentId, visiting, visited, result);
        }

        return result;
    }

    private void Visit(
        string componentId,
        HashSet<string> visiting,
        HashSet<string> visited,
        List<ComponentSource> result)
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
            Visit(dependency, visiting, visited, result);
        }

        visiting.Remove(componentId);
        visited.Add(componentId);
        result.Add(source);
    }

    private ComponentReference CreateReference(ComponentSource source, string configuration)
    {
        var paths = GetArtifactPaths(source, configuration);
        return new ComponentReference
        {
            Id = source.Id,
            Version = source.Version,
            AssemblyName = source.AssemblyName,
            AssemblyPath = paths.AssemblyPath,
        };
    }

    private ComponentArtifactPaths GetArtifactPaths(ComponentSource source, string configuration)
    {
        var root = Path.Combine(_repositoryRoot, ".kanata", "cache", "components", source.Id, source.Version, configuration);
        var libRoot = Path.Combine(root, "lib", source.TargetFramework);

        return new ComponentArtifactPaths
        {
            Root = root,
            LibRoot = libRoot,
            AssemblyPath = Path.Combine(libRoot, source.AssemblyName + ".dll"),
            BuildInfoPath = Path.Combine(root, "build-info.json"),
            ManifestCopyPath = Path.Combine(root, "kanata.component.json"),
        };
    }
}
