using Kanata.ProjectSystem.Validation;

namespace Kanata.Build.Reporting;

internal static class ValidationReporter
{
    public static void Print(string projectFilePath, ValidationResult result)
    {
        Console.WriteLine($"Project: {projectFilePath}");

        if (result.Issues.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Validation passed.");
            Console.ResetColor();
            return;
        }

        foreach (var issue in result.Issues)
        {
            Console.ForegroundColor = issue.Severity switch
            {
                ValidationSeverity.Error => ConsoleColor.Red,
                ValidationSeverity.Warning => ConsoleColor.Yellow,
                _ => ConsoleColor.Gray,
            };

            var path = string.IsNullOrWhiteSpace(issue.Path) ? string.Empty : $" [{issue.Path}]";
            Console.WriteLine($"{issue.Severity.ToString().ToLowerInvariant()} {issue.Code}: {issue.Message}{path}");
            Console.ResetColor();
        }
    }
}
