using Kanata.Build.Generation;

namespace Kanata.Build.Commands;

/// <summary>
/// Implements the <c>generate</c> command.
/// </summary>
public static class GenerateCommand
{
    /// <summary>
    /// Validates a project and generates build files for a target.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Process exit code.</returns>
    public static async Task<int> RunAsync(IReadOnlyList<string> args)
    {
        var context = await TargetBuildContextLoader.LoadAsync(args).ConfigureAwait(false);

        if (context is null)
        {
            return 1;
        }

        var propsWriter = new GeneratedPropsWriter();
        var propsPath = await propsWriter
            .WriteAsync(context.Project, context.ProjectFilePath, context.TargetName, context.Configuration)
            .ConfigureAwait(false);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Generated target build files.");
        Console.ResetColor();
        Console.WriteLine($"Props: {propsPath}");

        return 0;
    }
}
