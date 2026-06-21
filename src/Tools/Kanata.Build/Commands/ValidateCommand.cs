using Kanata.Build.Infrastructure;
using Kanata.Build.Reporting;
using Kanata.ProjectSystem.ProjectLoading;
using Kanata.ProjectSystem.Validation;

namespace Kanata.Build.Commands;

/// <summary>
/// Implements the <c>validate</c> command.
/// </summary>
public static class ValidateCommand
{
    /// <summary>
    /// Executes project validation for the current project directory.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Process exit code.</returns>
    public static async Task<int> RunAsync(IReadOnlyList<string> args)
    {
        var options = CommandLineOptions.Parse(args);

        if (options.Positionals.Count > 0 || options.HasFlag("project"))
        {
            PrintUsage();
            return 1;
        }

        var finder = new KanataProjectFileFinder();
        var reader = new KanataProjectReader();
        var validator = new KanataProjectValidator();

        var projectFilePath = finder.FindProjectFileInCurrentDirectory();
        var project = await reader.ReadAsync(projectFilePath).ConfigureAwait(false);
        var result = validator.Validate(project, projectFilePath);

        ValidationReporter.Print(projectFilePath, result);

        return result.HasErrors ? 1 : 0;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  kanata validate");
        Console.WriteLine();
        Console.WriteLine("Run this command from the project root directory that contains a .kanata file.");
    }
}
