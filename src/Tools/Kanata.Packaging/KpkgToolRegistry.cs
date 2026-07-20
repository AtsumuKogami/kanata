using System.Text.Json;

namespace Kanata.Packaging;

/// <summary>
/// Computes the installed tool registry view from the local Kanata package store.
/// </summary>
public static class KpkgToolRegistry
{
    /// <summary>
    /// Reads installed tool packages and their command/UI surfaces from the local package store.
    /// </summary>
    /// <param name="options">Tool registry read options.</param>
    /// <returns>The computed installed tool registry document.</returns>
    public static KpkgToolRegistryDocument Read(KpkgToolRegistryOptions? options = null)
    {
        options ??= new KpkgToolRegistryOptions();

        var store = KpkgPackageStore.Create(options.StoreRoot);
        var registry = KpkgInstalledRegistry.Read(store);
        var tools = registry.Packages
            .SelectMany(package => package.Installables
                .Where(installable => string.Equals(installable.Kind, "tool", StringComparison.OrdinalIgnoreCase))
                .Select(installable => ReadTool(store, package, installable)))
            .Where(tool => MatchesTarget(tool, options.TargetId))
            .OrderBy(tool => tool.Id, StringComparer.Ordinal)
            .ThenBy(tool => tool.Version, StringComparer.Ordinal)
            .ToArray();

        return new KpkgToolRegistryDocument
        {
            StoreRoot = store.RootPath,
            Tools = tools,
            Problems = FindCommandConflicts(tools)
        };
    }

    private static bool MatchesTarget(KpkgInstalledToolRecord tool, string? targetId)
    {
        return string.IsNullOrWhiteSpace(targetId)
            || string.Equals(tool.Id, targetId, StringComparison.Ordinal)
            || string.Equals(tool.PackageId, targetId, StringComparison.Ordinal);
    }

    private static KpkgInstalledToolRecord ReadTool(
        KpkgPackageStore store,
        KpkgInstalledPackageRecord package,
        KpkgInstalledInstallableRecord installable)
    {
        var problems = new List<string>();
        var installRoot = string.IsNullOrWhiteSpace(package.InstalledPath)
            ? store.GetInstallRoot(package.PackageId, package.Version, package.PackageSha256)
            : package.InstalledPath;

        var descriptorPath = Path.Combine(
            installRoot,
            "descriptors",
            $"{KpkgPackageStore.ToStoreSegment(installable.Id, "tool id")}.ktool");

        if (!File.Exists(descriptorPath))
        {
            problems.Add($"Tool descriptor is missing: {descriptorPath}.");
            return CreateToolRecord(package, installable, installRoot, descriptorPath, [], [], [], problems);
        }

        try
        {
            using var descriptor = JsonDocument.Parse(File.ReadAllBytes(descriptorPath));
            var root = descriptor.RootElement;
            CheckDescriptorIdentity(root, installable, problems);

            var filesDirectory = Path.Combine(installRoot, "files");
            var provides = ReadStringArray(root, "provides").ToArray();
            var commands = ReadCommands(root, filesDirectory, problems).ToArray();
            var surfaces = ReadSurfaces(root, filesDirectory, problems).ToArray();

            if (commands.Length == 0)
            {
                problems.Add("Tool descriptor does not declare required CLI commands.");
            }

            foreach (var command in commands.Where(command => command.Required && !command.EntryPointExists))
            {
                problems.Add($"Required command entry point is missing: {command.Name} -> {command.EntryPointPackagePath}.");
            }

            foreach (var surface in surfaces.Where(surface => !surface.Optional && !surface.EntryPointExists))
            {
                problems.Add($"Required surface entry point is missing: {surface.Id} -> {surface.EntryPointPackagePath}.");
            }

            return CreateToolRecord(package, installable, installRoot, descriptorPath, provides, commands, surfaces, problems);
        }
        catch (JsonException exception)
        {
            problems.Add($"Tool descriptor JSON is invalid: {exception.Message}");
            return CreateToolRecord(package, installable, installRoot, descriptorPath, [], [], [], problems);
        }
    }

    private static KpkgInstalledToolRecord CreateToolRecord(
        KpkgInstalledPackageRecord package,
        KpkgInstalledInstallableRecord installable,
        string installRoot,
        string descriptorPath,
        IReadOnlyList<string> provides,
        IReadOnlyList<KpkgToolCommandRecord> commands,
        IReadOnlyList<KpkgToolSurfaceRecord> surfaces,
        IReadOnlyList<string> problems)
    {
        return new KpkgInstalledToolRecord
        {
            Id = installable.Id,
            Version = installable.Version,
            PackageId = package.PackageId,
            PackageVersion = package.Version,
            PackageSha256 = package.PackageSha256,
            InstalledPath = installRoot,
            DescriptorPath = descriptorPath,
            Provides = provides,
            Commands = commands,
            Surfaces = surfaces,
            Problems = problems
        };
    }

    private static void CheckDescriptorIdentity(JsonElement root, KpkgInstalledInstallableRecord installable, List<string> problems)
    {
        CheckStringProperty(root, "id", installable.Id, problems);
        CheckStringProperty(root, "version", installable.Version, problems);
        CheckStringProperty(root, "kind", installable.Kind, problems);
    }

    private static void CheckStringProperty(JsonElement root, string propertyName, string expectedValue, List<string> problems)
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

    private static IEnumerable<KpkgToolCommandRecord> ReadCommands(JsonElement root, string filesDirectory, List<string> problems)
    {
        if (!root.TryGetProperty("commands", out var commands))
        {
            yield break;
        }

        if (commands.ValueKind != JsonValueKind.Array)
        {
            problems.Add("Descriptor property 'commands' must be an array.");
            yield break;
        }

        foreach (var command in commands.EnumerateArray())
        {
            if (command.ValueKind != JsonValueKind.Object)
            {
                problems.Add("Descriptor property 'commands' contains a non-object item.");
                continue;
            }

            var name = GetOptionalString(command, "name");
            if (string.IsNullOrWhiteSpace(name))
            {
                problems.Add("Tool command does not declare name.");
            }

            if (!command.TryGetProperty("entryPoint", out var entryPoint) || entryPoint.ValueKind != JsonValueKind.Object)
            {
                problems.Add($"Command '{name}' does not declare an entryPoint object.");
                continue;
            }

            var entryPointKind = GetOptionalString(entryPoint, "kind");
            var entryPointPath = GetOptionalString(entryPoint, "path");
            var localPath = ResolveOptionalEntryPoint(filesDirectory, entryPointPath, problems, $"Command '{name}'");

            yield return new KpkgToolCommandRecord
            {
                Name = name,
                Description = GetOptionalString(command, "description"),
                Aliases = ReadStringArray(command, "aliases").ToArray(),
                EntryPointKind = entryPointKind,
                EntryPointPackagePath = entryPointPath,
                EntryPointLocalPath = localPath,
                EntryPointExists = PathExists(localPath),
                Arguments = ReadStringArray(command, "arguments").ToArray(),
                LaunchMode = GetOptionalString(command, "launchMode"),
                Required = GetOptionalBool(command, "required", defaultValue: true)
            };
        }
    }

    private static IEnumerable<KpkgToolSurfaceRecord> ReadSurfaces(JsonElement root, string filesDirectory, List<string> problems)
    {
        if (!root.TryGetProperty("surfaces", out var surfaces))
        {
            yield break;
        }

        if (surfaces.ValueKind != JsonValueKind.Array)
        {
            problems.Add("Descriptor property 'surfaces' must be an array.");
            yield break;
        }

        foreach (var surface in surfaces.EnumerateArray())
        {
            if (surface.ValueKind != JsonValueKind.Object)
            {
                problems.Add("Descriptor property 'surfaces' contains a non-object item.");
                continue;
            }

            var id = GetOptionalString(surface, "id");
            if (string.IsNullOrWhiteSpace(id))
            {
                problems.Add("Tool surface does not declare id.");
            }

            if (!surface.TryGetProperty("entryPoint", out var entryPoint) || entryPoint.ValueKind != JsonValueKind.Object)
            {
                problems.Add($"Surface '{id}' does not declare an entryPoint object.");
                continue;
            }

            var entryPointKind = GetOptionalString(entryPoint, "kind");
            var entryPointPath = GetOptionalString(entryPoint, "path");
            var localPath = ResolveOptionalEntryPoint(filesDirectory, entryPointPath, problems, $"Surface '{id}'");

            yield return new KpkgToolSurfaceRecord
            {
                Id = id,
                Kind = GetOptionalString(surface, "kind"),
                Title = GetOptionalString(surface, "title"),
                Description = GetOptionalString(surface, "description"),
                Optional = GetOptionalBool(surface, "optional", defaultValue: true),
                EntryPointKind = entryPointKind,
                EntryPointPackagePath = entryPointPath,
                EntryPointLocalPath = localPath,
                EntryPointExists = PathExists(localPath),
                Platforms = ReadStringArray(surface, "platforms").ToArray()
            };
        }
    }

    private static IReadOnlyList<string> FindCommandConflicts(IReadOnlyList<KpkgInstalledToolRecord> tools)
    {
        var names = tools
            .SelectMany(tool => tool.Commands.Select(command => new { Tool = tool, Name = command.Name }))
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Name))
            .GroupBy(entry => entry.Name, StringComparer.Ordinal)
            .Where(group => group.Select(entry => entry.Tool.Id).Distinct(StringComparer.Ordinal).Count() > 1)
            .Select(group => $"Tool command conflict '{group.Key}': {string.Join(", ", group.Select(entry => entry.Tool.Id).Distinct(StringComparer.Ordinal))}.")
            .ToArray();

        var aliases = tools
            .SelectMany(tool => tool.Commands.SelectMany(command => command.Aliases.Select(alias => new { Tool = tool, Name = alias })))
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Name))
            .GroupBy(entry => entry.Name, StringComparer.Ordinal)
            .Where(group => group.Select(entry => entry.Tool.Id).Distinct(StringComparer.Ordinal).Count() > 1)
            .Select(group => $"Tool alias conflict '{group.Key}': {string.Join(", ", group.Select(entry => entry.Tool.Id).Distinct(StringComparer.Ordinal))}.")
            .ToArray();

        return names.Concat(aliases).ToArray();
    }

    private static IEnumerable<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var array) || array.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var item in array.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.String)
            {
                var value = item.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    yield return value;
                }
            }
        }
    }

    private static string GetOptionalString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
    }

    private static bool GetOptionalBool(JsonElement element, string propertyName, bool defaultValue)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
            ? property.GetBoolean()
            : defaultValue;
    }

    private static string ResolveOptionalEntryPoint(
        string filesDirectory,
        string packagePath,
        List<string> problems,
        string ownerDescription)
    {
        if (string.IsNullOrWhiteSpace(packagePath))
        {
            problems.Add($"{ownerDescription} does not declare entryPoint.path.");
            return string.Empty;
        }

        try
        {
            return ResolveInstalledPackagePath(filesDirectory, packagePath);
        }
        catch (KpkgFormatException exception)
        {
            problems.Add(exception.Message);
            return string.Empty;
        }
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
}
