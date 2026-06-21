using Kanata.Build.Components;
using Kanata.Build.Generation;
using Kanata.Build.Infrastructure;

namespace Kanata.Build.Commands;

/// <summary>
/// Implements the <c>build</c> command.
/// </summary>
public static class BuildCommand
{
    /// <summary>
    /// Validates, generates, and builds a Kanata target.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Process exit code.</returns>
    public static async Task<int> RunAsync(IReadOnlyList<string> args)
    {
        var options = CommandLineOptions.Parse(args);
        var context = await TargetBuildContextLoader.LoadAsync(args).ConfigureAwait(false);

        if (context is null)
        {
            return 1;
        }

        var forceEngineBuild = options.HasFlag("force-engine") || options.HasFlag("force-components");
        var componentCoordinator = new ComponentBuildCoordinator();
        var componentResults = await componentCoordinator
            .EnsureTargetComponentsAsync(context, forceEngineBuild)
            .ConfigureAwait(false);
        var componentReferences = componentResults.Select(result => result.Reference).ToArray();

        var propsWriter = new GeneratedPropsWriter();
        var propsPath = await propsWriter
            .WriteAsync(context.Project, context.ProjectFilePath, context.TargetName, context.Configuration, componentReferences)
            .ConfigureAwait(false);

        Console.WriteLine($"Generated props: {propsPath}");
        Console.WriteLine($"Building target '{context.TargetName}' ({context.Configuration})...");

        return await ProcessRunner.RunAsync(
            "dotnet",
            ["build", context.HostProjectPath, "-c", context.Configuration, $"-p:KanataGeneratedProps={propsPath}"],
            context.ProjectRoot).ConfigureAwait(false);
    }
}
