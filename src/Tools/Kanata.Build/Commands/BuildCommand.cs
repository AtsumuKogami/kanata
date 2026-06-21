using Kanata.Build.Generation;
using Kanata.Build.Infrastructure;
using Kanata.Build.Reporting;
using Kanata.ProjectSystem.ProjectLoading;
using Kanata.ProjectSystem.ProjectModel;
using Kanata.ProjectSystem.Validation;

namespace Kanata.Build.Commands;

/// <summary>
/// Implements the <c>build</c> command.
/// </summary>
public static class BuildCommand
{
    /// <summary>
    /// Executes a Kanata target build.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Process exit code.</returns>
    public static async Task<int> RunAsync(IReadOnlyList<string> args)
    {
        var options = CommandLineOptions.Parse(args);
        var targetName = options.Positionals.Count > 0 ? options.Positionals[0] : "desktop";
        var configuration = options.Positionals.Count > 1 ? options.Positionals[1] : "Debug";
        var projectInput = options.GetValue("project") ?? (options.Positionals.Count > 2 ? options.Positionals[2] : null);

        var finder = new KanataProjectFileFinder();
        var reader = new KanataProjectReader();
        var validator = new KanataProjectValidator();

        var projectFilePath = finder.FindProjectFile(projectInput);
        var projectRoot = Path.GetDirectoryName(projectFilePath) ?? Directory.GetCurrentDirectory();
        var project = await reader.ReadAsync(projectFilePath).ConfigureAwait(false);
        var validation = validator.Validate(project, projectFilePath);

        ValidationReporter.Print(projectFilePath, validation);

        if (validation.HasErrors)
        {
            return 1;
        }

        if (!TryGetTarget(project, targetName, out var resolvedTargetName, out var target))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"error: target '{targetName}' is not declared in project.");
            Console.ResetColor();
            return 1;
        }

        var propsWriter = new GeneratedPropsWriter();
        var propsPath = await propsWriter.WriteAsync(project, projectFilePath, resolvedTargetName, configuration).ConfigureAwait(false);
        var hostProjectPath = Path.GetFullPath(Path.Combine(projectRoot, target.HostProject!));

        Console.WriteLine($"Generated props: {propsPath}");
        Console.WriteLine($"Building target '{resolvedTargetName}' ({configuration})...");

        return await ProcessRunner.RunAsync(
            "dotnet",
            ["build", hostProjectPath, "-c", configuration, $"-p:KanataGeneratedProps={propsPath}"],
            projectRoot).ConfigureAwait(false);
    }

    private static bool TryGetTarget(
        KanataProject project,
        string targetName,
        out string resolvedTargetName,
        out KanataTarget target)
    {
        foreach (var pair in project.Targets)
        {
            if (string.Equals(pair.Key, targetName, StringComparison.OrdinalIgnoreCase))
            {
                resolvedTargetName = pair.Key;
                target = pair.Value;
                return true;
            }
        }

        resolvedTargetName = string.Empty;
        target = new KanataTarget();
        return false;
    }
}
