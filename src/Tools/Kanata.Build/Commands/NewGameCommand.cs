using Kanata.Build.Infrastructure;

namespace Kanata.Build.Commands;

/// <summary>
/// Implements the <c>new game</c> command.
/// </summary>
public static class NewGameCommand
{
    /// <summary>
    /// Creates a new Kanata game project.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Process exit code.</returns>
    public static Task<int> RunAsync(IReadOnlyList<string> args)
    {
        var options = CommandLineOptions.Parse(args);

        if (options.Positionals.Count < 2 || !string.Equals(options.Positionals[0], "game", StringComparison.OrdinalIgnoreCase))
        {
            PrintUsage();
            return Task.FromResult(1);
        }

        var displayName = options.Positionals[1];
        var projectName = TextNaming.ToPascalIdentifier(displayName);
        var projectId = options.GetValue("id") ?? TextNaming.ToProjectId(displayName);
        var output = options.GetValue("output") ?? projectName;
        var force = options.HasFlag("force");

        var projectRoot = Path.GetFullPath(output);

        if (Directory.Exists(projectRoot) && Directory.EnumerateFileSystemEntries(projectRoot).Any() && !force)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"error: output directory is not empty: {projectRoot}");
            Console.ResetColor();
            return Task.FromResult(1);
        }

        CreateProject(projectRoot, projectName, displayName, projectId);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Created Kanata game project: {displayName}");
        Console.ResetColor();
        Console.WriteLine($"Project file: {Path.Combine(projectRoot, projectName + ".kanata")}");
        Console.WriteLine();
        Console.WriteLine("Next:");
        Console.WriteLine($"  kanata validate {Path.Combine(projectRoot, projectName + ".kanata")}");
        Console.WriteLine($"  kanata build desktop Debug {Path.Combine(projectRoot, projectName + ".kanata")}");

        return Task.FromResult(0);
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  kanata new game <name> [--output <path>] [--id <id>] [--force]");
    }

    private static void CreateProject(string projectRoot, string projectName, string displayName, string projectId)
    {
        Directory.CreateDirectory(projectRoot);

        CreateDirectories(projectRoot);
        WriteProjectFile(projectRoot, projectName, displayName, projectId);
        WriteContentFiles(projectRoot);
        WriteSourceProjects(projectRoot, projectName, projectId);
        WriteDesktopHost(projectRoot, projectName, displayName);
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

    private static void WriteProjectFile(string projectRoot, string projectName, string displayName, string projectId)
    {
        WriteFile(projectRoot, $"{projectName}.kanata", $$"""
{
  "$schema": "https://schemas.kanata.dev/project/v1/kanata.project.schema.json",

  "format": "kanata.project",
  "schemaVersion": 1,

  "id": "{{projectId}}",
  "name": "{{displayName}}",
  "projectVersion": "0.1.0",
  "kanataVersion": "0.1.0",

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

    private static void WriteSourceProjects(string projectRoot, string projectName, string projectId)
    {
        WriteFile(projectRoot, $"Source/Shared/{projectName}.Shared.csproj", ProjectFile(projectName, "Shared", null));
        WriteFile(projectRoot, $"Source/Logic/{projectName}.Logic.csproj", ProjectFile(projectName, "Logic", [Path.Combine("..", "Shared", $"{projectName}.Shared.csproj")]));
        WriteFile(projectRoot, $"Source/View/{projectName}.View.csproj", ProjectFile(projectName, "View", [Path.Combine("..", "Shared", $"{projectName}.Shared.csproj"), Path.Combine("..", "Logic", $"{projectName}.Logic.csproj")]));

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

    private static void WriteDesktopHost(string projectRoot, string projectName, string displayName)
    {
        WriteFile(projectRoot, $"Platforms/Desktop/{projectName}.Desktop.csproj", $$"""
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
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

    private static string ProjectFile(string projectName, string layerName, IEnumerable<string>? references)
    {
        var referenceItems = references?.Select(reference => $"    <ProjectReference Include=\"{reference}\" />") ?? [];
        var referenceBlock = referenceItems.Any()
            ? $"\n  <ItemGroup>\n{string.Join(Environment.NewLine, referenceItems)}\n  </ItemGroup>\n"
            : string.Empty;

        return $$"""
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
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
