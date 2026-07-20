using System.Text.Json;

namespace Kanata.Packaging;

/// <summary>
/// Inspects packages installed in the local Kanata package store and checks artifact usability.
/// </summary>
public static class KpkgPackageInspector
{
    /// <summary>
    /// Inspects installed packages from the configured local package store.
    /// </summary>
    /// <param name="options">The inspector options.</param>
    /// <returns>The inspection result.</returns>
    public static KpkgPackageInspectionResult Inspect(KpkgPackageInspectorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var store = KpkgPackageStore.Create(options.StoreRoot);
        var registry = KpkgInstalledRegistry.Read(store);
        var packages = registry.Packages
            .Where(package => MatchesTarget(package, options.TargetId))
            .Select(package => InspectPackage(store, registry, package))
            .ToArray();

        return new KpkgPackageInspectionResult
        {
            StoreRoot = store.RootPath,
            Packages = packages
        };
    }

    private static bool MatchesTarget(KpkgInstalledPackageRecord package, string? targetId)
    {
        if (string.IsNullOrWhiteSpace(targetId))
        {
            return true;
        }

        return string.Equals(package.PackageId, targetId, StringComparison.Ordinal)
            || package.Installables.Any(installable => string.Equals(installable.Id, targetId, StringComparison.Ordinal));
    }

    private static KpkgInstalledPackageInspection InspectPackage(
        KpkgPackageStore store,
        KpkgInstalledRegistryDocument registry,
        KpkgInstalledPackageRecord record)
    {
        var problems = new List<string>();
        var installRoot = record.InstalledPath;

        if (string.IsNullOrWhiteSpace(installRoot))
        {
            problems.Add("Installed path is empty in the registry.");
            installRoot = store.GetInstallRoot(record.PackageId, record.Version, record.PackageSha256);
        }

        if (!Directory.Exists(installRoot))
        {
            problems.Add($"Installed path does not exist: {installRoot}.");
        }

        var installManifestPath = Path.Combine(installRoot, "package.install.kmanifest");
        if (!File.Exists(installManifestPath))
        {
            problems.Add($"Install manifest is missing: {installManifestPath}.");
        }

        var descriptorsDirectory = Path.Combine(installRoot, "descriptors");
        if (!Directory.Exists(descriptorsDirectory))
        {
            problems.Add($"Descriptors directory is missing: {descriptorsDirectory}.");
        }

        var filesDirectory = Path.Combine(installRoot, "files");
        if (!Directory.Exists(filesDirectory))
        {
            problems.Add($"Files directory is missing: {filesDirectory}.");
        }

        var installables = record.Installables
            .Select(installable => InspectInstallable(registry, installRoot, filesDirectory, installable))
            .ToArray();

        return new KpkgInstalledPackageInspection
        {
            PackageId = record.PackageId,
            Version = record.Version,
            PackageSha256 = record.PackageSha256,
            InstalledPath = installRoot,
            Installables = installables,
            Problems = problems
        };
    }

    private static KpkgInstalledInstallableInspection InspectInstallable(
        KpkgInstalledRegistryDocument registry,
        string installRoot,
        string filesDirectory,
        KpkgInstalledInstallableRecord installable)
    {
        var problems = new List<string>();
        var descriptorPath = Path.Combine(
            installRoot,
            "descriptors",
            $"{KpkgPackageStore.ToStoreSegment(installable.Id, "installable id")}{GetDescriptorExtension(installable.Kind)}");

        if (!File.Exists(descriptorPath))
        {
            problems.Add($"Descriptor file is missing: {descriptorPath}.");
            return new KpkgInstalledInstallableInspection
            {
                Id = installable.Id,
                Version = installable.Version,
                Kind = installable.Kind,
                DescriptorPath = descriptorPath,
                Problems = problems
            };
        }

        try
        {
            using var descriptor = JsonDocument.Parse(File.ReadAllBytes(descriptorPath));
            var root = descriptor.RootElement;

            CheckDescriptorIdentity(root, installable, problems);

            var artifacts = ReadPathArray(root, "artifacts", filesDirectory, problems).ToArray();
            var sources = ReadPathArray(root, "sources", filesDirectory, problems).ToArray();
            var commands = ReadCommands(root, filesDirectory, problems).ToArray();
            var dependencies = ReadDependencies(root, registry).ToArray();
            var sourceReferenceCount = CountArray(root, "sourceRefs");

            if (artifacts.Length == 0)
            {
                problems.Add("No artifacts are declared. V1 installed packages are usable only through artifacts.");
            }

            foreach (var artifact in artifacts.Where(artifact => !artifact.Exists))
            {
                problems.Add($"Artifact is missing: {artifact.PackagePath}.");
            }

            foreach (var command in commands.Where(command => !command.EntryPointExists))
            {
                problems.Add($"Command entry point is missing: {command.Name} -> {command.EntryPointPackagePath}.");
            }

            foreach (var dependency in dependencies.Where(dependency => !dependency.IsInstalled))
            {
                problems.Add($"Dependency is not installed: {dependency.Id}.");
            }

            return new KpkgInstalledInstallableInspection
            {
                Id = installable.Id,
                Version = installable.Version,
                Kind = installable.Kind,
                DescriptorPath = descriptorPath,
                Artifacts = artifacts,
                Sources = sources,
                Commands = commands,
                Dependencies = dependencies,
                SourceReferenceCount = sourceReferenceCount,
                Problems = problems
            };
        }
        catch (JsonException exception)
        {
            problems.Add($"Descriptor JSON is invalid: {exception.Message}");
            return new KpkgInstalledInstallableInspection
            {
                Id = installable.Id,
                Version = installable.Version,
                Kind = installable.Kind,
                DescriptorPath = descriptorPath,
                Problems = problems
            };
        }
    }

    private static void CheckDescriptorIdentity(
        JsonElement root,
        KpkgInstalledInstallableRecord installable,
        List<string> problems)
    {
        CheckStringProperty(root, "id", installable.Id, problems);
        CheckStringProperty(root, "version", installable.Version, problems);
        CheckStringProperty(root, "kind", installable.Kind, problems);
    }

    private static void CheckStringProperty(
        JsonElement root,
        string propertyName,
        string expectedValue,
        List<string> problems)
    {
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            problems.Add($"Descriptor property '{propertyName}' is missing or is not a string.");
            return;
        }

        var actualValue = property.GetString();
        if (!string.Equals(actualValue, expectedValue, StringComparison.Ordinal))
        {
            problems.Add($"Descriptor property '{propertyName}' is '{actualValue}', expected '{expectedValue}'.");
        }
    }

    private static IEnumerable<KpkgInstalledPathInspection> ReadPathArray(
        JsonElement root,
        string propertyName,
        string filesDirectory,
        List<string> problems)
    {
        if (!root.TryGetProperty(propertyName, out var array))
        {
            yield break;
        }

        if (array.ValueKind != JsonValueKind.Array)
        {
            problems.Add($"Descriptor property '{propertyName}' must be an array.");
            yield break;
        }

        foreach (var item in array.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                problems.Add($"Descriptor property '{propertyName}' contains a non-object item.");
                continue;
            }

            var role = GetOptionalString(item, "role");
            var packagePath = GetOptionalString(item, "path");
            if (string.IsNullOrWhiteSpace(packagePath))
            {
                problems.Add($"Descriptor property '{propertyName}' contains an item without path.");
                continue;
            }

            string localPath;
            try
            {
                localPath = ResolveInstalledPackagePath(filesDirectory, packagePath);
            }
            catch (KpkgFormatException exception)
            {
                problems.Add(exception.Message);
                localPath = string.Empty;
            }

            yield return new KpkgInstalledPathInspection
            {
                Role = role,
                PackagePath = packagePath,
                LocalPath = localPath,
                Exists = PathExists(localPath)
            };
        }
    }

    private static IEnumerable<KpkgInstalledCommandInspection> ReadCommands(
        JsonElement root,
        string filesDirectory,
        List<string> problems)
    {
        if (!root.TryGetProperty("commands", out var array))
        {
            yield break;
        }

        if (array.ValueKind != JsonValueKind.Array)
        {
            problems.Add("Descriptor property 'commands' must be an array.");
            yield break;
        }

        foreach (var command in array.EnumerateArray())
        {
            if (command.ValueKind != JsonValueKind.Object)
            {
                problems.Add("Descriptor property 'commands' contains a non-object item.");
                continue;
            }

            var name = GetOptionalString(command, "name");
            if (!command.TryGetProperty("entryPoint", out var entryPoint) || entryPoint.ValueKind != JsonValueKind.Object)
            {
                problems.Add($"Command '{name}' does not declare an entryPoint object.");
                continue;
            }

            var entryPointKind = GetOptionalString(entryPoint, "kind");
            var entryPointPath = GetOptionalString(entryPoint, "path");
            if (string.IsNullOrWhiteSpace(entryPointPath))
            {
                problems.Add($"Command '{name}' does not declare entryPoint.path.");
                continue;
            }

            string localPath;
            try
            {
                localPath = ResolveInstalledPackagePath(filesDirectory, entryPointPath);
            }
            catch (KpkgFormatException exception)
            {
                problems.Add(exception.Message);
                localPath = string.Empty;
            }

            yield return new KpkgInstalledCommandInspection
            {
                Name = name,
                EntryPointKind = entryPointKind,
                EntryPointPackagePath = entryPointPath,
                EntryPointLocalPath = localPath,
                EntryPointExists = PathExists(localPath)
            };
        }
    }

    private static IEnumerable<KpkgInstalledDependencyInspection> ReadDependencies(
        JsonElement root,
        KpkgInstalledRegistryDocument registry)
    {
        if (!root.TryGetProperty("dependencies", out var array) || array.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var dependency in array.EnumerateArray())
        {
            var dependencyId = dependency.ValueKind switch
            {
                JsonValueKind.String => dependency.GetString(),
                JsonValueKind.Object when dependency.TryGetProperty("id", out var idProperty)
                    && idProperty.ValueKind == JsonValueKind.String => idProperty.GetString(),
                _ => null
            };

            if (string.IsNullOrWhiteSpace(dependencyId))
            {
                continue;
            }

            yield return new KpkgInstalledDependencyInspection
            {
                Id = dependencyId,
                IsInstalled = IsDependencyInstalled(registry, dependencyId)
            };
        }
    }

    private static bool IsDependencyInstalled(KpkgInstalledRegistryDocument registry, string dependencyId)
    {
        return registry.Packages.Any(package =>
            string.Equals(package.PackageId, dependencyId, StringComparison.Ordinal)
            || package.Installables.Any(installable => string.Equals(installable.Id, dependencyId, StringComparison.Ordinal)));
    }

    private static int CountArray(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
        {
            return 0;
        }

        return property.GetArrayLength();
    }

    private static string GetOptionalString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static string ResolveInstalledPackagePath(string filesDirectory, string packagePath)
    {
        KpkgPathValidator.Validate(packagePath);

        var relativePath = packagePath.Replace('/', Path.DirectorySeparatorChar);
        var localPath = Path.GetFullPath(Path.Combine(filesDirectory, relativePath));
        var rootPath = Path.GetFullPath(filesDirectory);

        if (!localPath.StartsWith(rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && !string.Equals(localPath, rootPath, StringComparison.Ordinal))
        {
            throw new KpkgFormatException($"Package path escapes the installed files directory: {packagePath}.");
        }

        return localPath;
    }

    private static bool PathExists(string path)
    {
        return !string.IsNullOrWhiteSpace(path)
            && (File.Exists(path) || Directory.Exists(path));
    }


    private static string GetDescriptorExtension(string kind)
    {
        return kind.ToLowerInvariant() switch
        {
            "tool" => ".ktool",
            "backend" => ".kbackend",
            "runtime" => ".kruntime",
            "editor" => ".keditor",
            "plugin" => ".kplugin",
            _ => ".kcomponent"
        };
    }
}
