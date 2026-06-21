using System.Text.Json;
using Kanata.Build.Commands;
using Kanata.Build.Components;
using Kanata.Build.Infrastructure;
using Kanata.ProjectSystem.LockModel;

namespace Kanata.Build.Restore;

internal sealed class ComponentLockWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public async Task<string> WriteAsync(
        TargetBuildContext context,
        IReadOnlyList<ComponentReference> componentReferences,
        CancellationToken cancellationToken = default)
    {
        var lockFile = new KanataLockFile
        {
            Schema = "https://schemas.kanata.dev/lock/v1/kanata.lock.schema.json",
            Format = KanataLockFileFormat.Format,
            SchemaVersion = KanataLockFileFormat.SupportedSchemaVersion,
            ProjectId = context.Project.Id ?? string.Empty,
            KanataVersion = KanataVersionProvider.CurrentVersion,
            Target = context.TargetName,
            Configuration = context.Configuration,
            GeneratedAtUtc = DateTimeOffset.UtcNow,
            Components = componentReferences.Select(ToLockedComponent).ToList(),
        };

        var lockPath = Path.Combine(context.ProjectRoot, "Kanata.lock.json");
        var json = JsonSerializer.Serialize(lockFile, JsonOptions);
        await File.WriteAllTextAsync(lockPath, json, cancellationToken).ConfigureAwait(false);
        return lockPath;
    }

    private static KanataLockedComponent ToLockedComponent(ComponentReference reference)
    {
        return new KanataLockedComponent
        {
            Id = reference.Id,
            Version = reference.Version,
            Kind = reference.Kind,
            Source = reference.Source,
            TargetFramework = reference.TargetFramework,
            AssemblyName = reference.AssemblyName,
            AssemblyPath = reference.AssemblyPath,
            ManifestPath = reference.ManifestPath,
            Dependencies = reference.Dependencies.ToList(),
        };
    }
}
