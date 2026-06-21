using Kanata.Build.Infrastructure;
using Kanata.Build.Reporting;
using Kanata.ProjectSystem.ProjectLoading;
using Kanata.ProjectSystem.Validation;

namespace Kanata.Build.Commands;

/// <summary>
/// Implements project creation commands.
/// </summary>
public static class NewGameCommand
{
    /// <summary>
    /// Creates a new Kanata game project using the <c>new</c> syntax.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Process exit code.</returns>
    public static async Task<int> RunAsync(IReadOnlyList<string> args)
    {
        return await RunCreateInternalAsync(args).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new Kanata game project using the short <c>create</c> syntax.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Process exit code.</returns>
    public static async Task<int> RunCreateAsync(IReadOnlyList<string> args)
    {
        return await RunCreateInternalAsync(args).ConfigureAwait(false);
    }

    private static async Task<int> RunCreateInternalAsync(IReadOnlyList<string> args)
    {
        var options = CommandLineOptions.Parse(args);
        var nameIndex = options.Positionals.Count > 0
            && string.Equals(options.Positionals[0], "game", StringComparison.OrdinalIgnoreCase)
                ? 1
                : 0;

        if (options.Positionals.Count <= nameIndex)
        {
            PrintCreateUsage();
            return 1;
        }

        var displayName = options.Positionals[nameIndex];
        var projectName = TextNaming.ToPascalIdentifier(displayName);
        var projectId = options.GetValue("id") ?? TextNaming.ToProjectId(displayName);
        var output = options.GetValue("output") ?? projectName;
        var force = options.HasFlag("force");
        var kanataVersion = KanataVersionProvider.CurrentVersion;
        var targetFramework = KanataSdkInfo.TargetFramework;

        var projectRoot = Path.GetFullPath(output);

        if (Directory.Exists(projectRoot) && Directory.EnumerateFileSystemEntries(projectRoot).Any() && !force)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"error: output directory is not empty: {projectRoot}");
            Console.ResetColor();
            return 1;
        }

        CreateProject(projectRoot, projectName, displayName, projectId, kanataVersion, targetFramework);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Created Kanata game project: {displayName}");
        Console.ResetColor();

        var projectFilePath = Path.Combine(projectRoot, projectName + ".kanata");
        Console.WriteLine($"Project file: {projectFilePath}");
        Console.WriteLine($"Kanata version: {kanataVersion}");
        Console.WriteLine($"Target framework: {targetFramework}");
        Console.WriteLine();

        var validationExitCode = await ValidateCreatedProjectAsync(projectFilePath).ConfigureAwait(false);

        if (validationExitCode != 0)
        {
            return validationExitCode;
        }

        Console.WriteLine("Next:");
        Console.WriteLine($"  cd {projectRoot}");
        Console.WriteLine("  kanata build");
        Console.WriteLine("  kanata play");

        return 0;
    }

    private static void PrintCreateUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  kanata create <name> [--output <path>] [--id <id>] [--force]");
        Console.WriteLine("  kanata create game <name> [--output <path>] [--id <id>] [--force]");
        Console.WriteLine("  kanata new <name> [--output <path>] [--id <id>] [--force]");
        Console.WriteLine("  kanata new game <name> [--output <path>] [--id <id>] [--force]");
    }

    private static async Task<int> ValidateCreatedProjectAsync(string projectFilePath)
    {
        Console.WriteLine("Validating generated project...");

        var reader = new KanataProjectReader();
        var validator = new KanataProjectValidator();
        var project = await reader.ReadAsync(projectFilePath).ConfigureAwait(false);
        var result = validator.Validate(project, projectFilePath);

        ValidationReporter.Print(projectFilePath, result);

        return result.HasErrors ? 1 : 0;
    }

    private static void CreateProject(
        string projectRoot,
        string projectName,
        string displayName,
        string projectId,
        string kanataVersion,
        string targetFramework)
    {
        Directory.CreateDirectory(projectRoot);

        CreateDirectories(projectRoot);
        WriteProjectFile(projectRoot, projectName, displayName, projectId, kanataVersion);
        WriteContentFiles(projectRoot);
        WriteSourceProjects(projectRoot, projectName, projectId, targetFramework);
        WriteDesktopHost(projectRoot, projectName, displayName, targetFramework);
    }

    private static void CreateDirectories(string projectRoot)
    {
        foreach (var directory in new[]
        {
            "Content/Assets",
            "Content/Objects",
            "Content/Scenes",
            "Content/UI",
            "Content/Data",
            "Source/Shared",
            "Source/Logic",
            "Source/View",
            "ProjectSettings",
            "Generated",
            "Platforms/Desktop",
        })
        {
            Directory.CreateDirectory(Path.Combine(projectRoot, directory));
        }

        WriteFile(projectRoot, "Content/Assets/.gitkeep", string.Empty);
        WriteFile(projectRoot, "Content/Objects/.gitkeep", string.Empty);
        WriteFile(projectRoot, "Content/UI/.gitkeep", string.Empty);
        WriteFile(projectRoot, "Content/Data/.gitkeep", string.Empty);
        WriteFile(projectRoot, "ProjectSettings/.gitkeep", string.Empty);
        WriteFile(projectRoot, "Generated/.gitkeep", string.Empty);
    }

    private static void WriteProjectFile(
        string projectRoot,
        string projectName,
        string displayName,
        string projectId,
        string kanataVersion)
    {
        WriteFile(projectRoot, $"{projectName}.kanata", $$"""
{
  "$schema": "https://schemas.kanata.dev/project/v1/kanata.project.schema.json",

  "format": "kanata.project",
  "schemaVersion": 1,

  "id": "{{projectId}}",
  "name": "{{displayName}}",
  "projectVersion": "0.1.0",
  "kanataVersion": "{{kanataVersion}}",

  "paths": {
    "content": "Content",
    "source": "Source",
    "generated": "Generated",
    "settings": "ProjectSettings"
  },

  "source": {
    "shared": "Source/Shared/{{projectName}}.Shared.csproj",
    "logic": "Source/Logic/{{projectName}}.Logic.csproj",
    "view": "Source/View/{{projectName}}.View.csproj"
  },

  "features": [
    "ui",
    "input",
    "assets",
    "local-session"
  ],

  "targets": {
    "desktop": {
      "platform": "desktop",
      "backend": "kanata.backend.monogame",
      "hostProject": "Platforms/Desktop/{{projectName}}.Desktop.csproj",
      "session": "local"
    }
  },

  "start": {
    "scene": "Content/Scenes/MainMenu.kscene"
  }
}
""");
    }

    private static void WriteContentFiles(string projectRoot)
    {
        WriteFile(projectRoot, "Content/Scenes/MainMenu.kscene", """
{
  "format": "kanata.scene",
  "schemaVersion": 1,
  "id": "main-menu",
  "name": "Main Menu"
}
""");
    }

    private static void WriteSourceProjects(string projectRoot, string projectName, string projectId, string targetFramework)
    {
        WriteFile(projectRoot, $"Source/Shared/{projectName}.Shared.csproj", ProjectFile(projectName, "Shared", null, targetFramework));
        WriteFile(projectRoot, $"Source/Logic/{projectName}.Logic.csproj", ProjectFile(projectName, "Logic", [Path.Combine("..", "Shared", $"{projectName}.Shared.csproj")], targetFramework));
        WriteFile(projectRoot, $"Source/View/{projectName}.View.csproj", ProjectFile(projectName, "View", [Path.Combine("..", "Shared", $"{projectName}.Shared.csproj"), Path.Combine("..", "Logic", $"{projectName}.Logic.csproj")], targetFramework));

        WriteFile(projectRoot, $"Source/Shared/{projectName}Info.cs", $$"""
namespace {{projectName}}.Shared;

/// <summary>
/// Contains basic information about the generated Kanata project.
/// </summary>
public static class {{projectName}}Info
{
    /// <summary>
    /// Gets the stable Kanata project identifier.
    /// </summary>
    public const string Id = "{{projectId}}";
}
""");

        WriteFile(projectRoot, $"Source/Logic/{projectName}Logic.cs", $$"""
namespace {{projectName}}.Logic;

/// <summary>
/// Marks the initial game logic assembly.
/// </summary>
public static class {{projectName}}Logic
{
}
""");

        WriteFile(projectRoot, $"Source/View/{projectName}View.cs", $$"""
namespace {{projectName}}.View;

/// <summary>
/// Marks the initial game view assembly.
/// </summary>
public static class {{projectName}}View
{
}
""");
    }

    private static void WriteDesktopHost(string projectRoot, string projectName, string displayName, string targetFramework)
    {
        WriteFile(projectRoot, $"Platforms/Desktop/{projectName}.Desktop.csproj", $$"""
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>{{targetFramework}}</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <Import Project="$(KanataGeneratedProps)" Condition="'$(KanataGeneratedProps)' != '' and Exists('$(KanataGeneratedProps)')" />
</Project>
""");

        WriteFile(projectRoot, "Platforms/Desktop/Program.cs", $$"""
using {{projectName}}.Shared;

Console.WriteLine("Kanata desktop host");
Console.WriteLine("Project: {{displayName}}");
Console.WriteLine("Project id: " + {{projectName}}Info.Id);
""");
    }

    private static string ProjectFile(string projectName, string layerName, IEnumerable<string>? references, string targetFramework)
    {
        var referenceItems = references?.Select(reference => $"    <ProjectReference Include=\"{reference}\" />") ?? [];
        var referenceBlock = referenceItems.Any()
            ? $"\n  <ItemGroup>\n{string.Join(Environment.NewLine, referenceItems)}\n  </ItemGroup>\n"
            : string.Empty;

        return $$"""
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>{{targetFramework}}</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AssemblyName>{{projectName}}.{{layerName}}</AssemblyName>
    <RootNamespace>{{projectName}}.{{layerName}}</RootNamespace>
  </PropertyGroup>{{referenceBlock}}
</Project>
""";
    }

    private static void WriteFile(string projectRoot, string relativePath, string content)
    {
        var fullPath = Path.Combine(projectRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content.Replace("\n", Environment.NewLine));
    }
}
