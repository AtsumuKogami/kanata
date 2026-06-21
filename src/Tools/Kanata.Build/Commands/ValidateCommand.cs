using Kanata.ProjectSystem.ProjectLoading;
using Kanata.ProjectSystem.Validation;

namespace Kanata.Build.Commands;

internal sealed class ValidateCommand
{
    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        var requestedPath = args.Length > 0 ? args[0] : null;
        var finder = new KanataProjectFileFinder();
        var reader = new KanataProjectReader();
        var validator = new KanataProjectValidator();

        try
        {
            var projectFilePath = finder.Find(requestedPath);
            var project = await reader.ReadAsync(projectFilePath, cancellationToken);
            var result = validator.Validate(project, projectFilePath);

            Console.WriteLine($"Project: {projectFilePath}");
            PrintIssues(result);

            if (result.HasErrors)
            {
                Console.WriteLine("Validation failed.");
                return 1;
            }

            Console.WriteLine("Validation succeeded.");
            return 0;
        }
        catch (Exception exception) when (exception is IOException or InvalidOperationException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            Console.Error.WriteLine($"Validation failed: {exception.Message}");
            return 1;
        }
    }

    private static void PrintIssues(ValidationResult result)
    {
        if (result.Issues.Count == 0)
        {
            return;
        }

        foreach (var issue in result.Issues)
        {
            var location = string.IsNullOrWhiteSpace(issue.Location) ? string.Empty : $" [{issue.Location}]";
            Console.WriteLine($"{issue.Severity} {issue.Code}{location}: {issue.Message}");
        }
    }
}
