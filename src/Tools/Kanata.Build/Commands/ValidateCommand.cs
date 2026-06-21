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
    /// Executes project validation.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Process exit code.</returns>
    public static async Task<int> RunAsync(IReadOnlyList<string> args)
    {
        var projectInput = args.Count > 0 ? args[0] : null;
        var finder = new KanataProjectFileFinder();
        var reader = new KanataProjectReader();
        var validator = new KanataProjectValidator();

        var projectFilePath = finder.FindProjectFile(projectInput);
        var project = await reader.ReadAsync(projectFilePath).ConfigureAwait(false);
        var result = validator.Validate(project, projectFilePath);

        ValidationReporter.Print(projectFilePath, result);

        return result.HasErrors ? 1 : 0;
    }
}
