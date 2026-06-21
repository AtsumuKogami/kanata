using Kanata.Build.Generation;
using Kanata.Build.Infrastructure;

namespace Kanata.Build.Commands;

/// <summary>
/// Implements the <c>play</c> command.
/// </summary>
public static class PlayCommand
{
    /// <summary>
    /// Validates, generates, and runs a Kanata target.
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

        Console.WriteLine($"Generated props: {propsPath}");
        Console.WriteLine($"Running target '{context.TargetName}' ({context.Configuration})...");

        return await ProcessRunner.RunAsync(
            "dotnet",
            ["run", "--project", context.HostProjectPath, "-c", context.Configuration, $"--property:KanataGeneratedProps={propsPath}"],
            context.ProjectRoot).ConfigureAwait(false);
    }
}
