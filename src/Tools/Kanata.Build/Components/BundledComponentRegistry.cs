using Kanata.Build.Infrastructure;
using Kanata.ProjectSystem.ComponentLoading;
using Kanata.ProjectSystem.ComponentModel;

namespace Kanata.Build.Components;

internal sealed class BundledComponentRegistry
{
    private readonly KanataComponentManifestReader _reader = new();

    public string RepositoryRoot { get; }

    public BundledComponentRegistry(string repositoryRoot)
    {
        RepositoryRoot = repositoryRoot;
    }

    public async Task<IReadOnlyDictionary<string, ComponentSource>> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        var manifestPaths = Directory.EnumerateFiles(RepositoryRoot, "*.kcomponent", SearchOption.AllDirectories)
            .Where(path => !path.Replace('\\', '/').Contains("/.kanata/", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var result = new Dictionary<string, ComponentSource>(StringComparer.OrdinalIgnoreCase);

        foreach (var manifestPath in manifestPaths)
        {
            var manifest = await _reader.ReadAsync(manifestPath, cancellationToken).ConfigureAwait(false);
            var source = CreateSource(manifestPath, manifest);
            result[source.Id] = source;
        }

        return result;
    }

    private ComponentSource CreateSource(string manifestPath, KanataComponentManifest manifest)
    {
        if (!string.Equals(manifest.Format, KanataComponentManifestFormat.Format, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Invalid component manifest format in {manifestPath}.");
        }

        if (manifest.SchemaVersion != KanataComponentManifestFormat.SupportedSchemaVersion)
        {
            throw new InvalidOperationException(
                $"Unsupported component manifest schema version {manifest.SchemaVersion} in {manifestPath}.");
        }

        var componentRoot = Path.GetDirectoryName(Path.GetFullPath(manifestPath)) ?? RepositoryRoot;
        var version = ResolveVersion(manifest.Version);
        var projectPath = Path.GetFullPath(Path.Combine(componentRoot, Require(manifest.Project, manifestPath, "project")));
        var assemblyName = manifest.AssemblyName ?? Path.GetFileNameWithoutExtension(projectPath);

        return new ComponentSource
        {
            Id = Require(manifest.Id, manifestPath, "id"),
            Version = version,
            Kind = manifest.Kind ?? "runtime",
            ManifestPath = Path.GetFullPath(manifestPath),
            ComponentRoot = componentRoot,
            ProjectPath = projectPath,
            AssemblyName = assemblyName,
            TargetFramework = manifest.TargetFramework ?? KanataSdkInfo.TargetFramework,
            Dependencies = manifest.Dependencies,
            Manifest = manifest,
        };
    }

    private static string ResolveVersion(string? version)
    {
        if (string.IsNullOrWhiteSpace(version) || string.Equals(version, "$kanata", StringComparison.OrdinalIgnoreCase))
        {
            return KanataVersionProvider.CurrentVersion;
        }

        return version;
    }

    private static string Require(string? value, string manifestPath, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Component manifest {manifestPath} does not define '{propertyName}'.");
        }

        return value;
    }
}
