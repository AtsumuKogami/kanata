using Kanata.Build.Infrastructure;
using Kanata.Build.Reporting;
using Kanata.ProjectSystem.ProjectLoading;
using Kanata.ProjectSystem.ProjectModel;
using Kanata.ProjectSystem.Validation;

namespace Kanata.Build.Commands;

internal static class TargetBuildContextLoader
{
    public static async Task<TargetBuildContext?> LoadAsync(IReadOnlyList<string> args)
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
            return null;
        }

        if (!TryGetTarget(project, targetName, out var resolvedTargetName, out var target))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"error: target '{targetName}' is not declared in project.");
            Console.ResetColor();
            return null;
        }

        var hostProjectPath = Path.GetFullPath(Path.Combine(projectRoot, target.HostProject!));

        return new TargetBuildContext
        {
            ProjectFilePath = projectFilePath,
            ProjectRoot = projectRoot,
            Project = project,
            TargetName = resolvedTargetName,
            Target = target,
            Configuration = configuration,
            HostProjectPath = hostProjectPath,
        };
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
