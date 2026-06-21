using Kanata.Build.Generation;
using Kanata.Build.Infrastructure;
using Kanata.Build.Restore;

namespace Kanata.Build.Commands;

/// <summary>
/// Implements the <c>build</c> command.
/// </summary>
public static class BuildCommand
{
    /// <summary>
    /// Restores, generates, and builds a Kanata target.
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

        var forceComponents = options.HasFlag("force-engine") || options.HasFlag("force-components");
        var restoreService = new ComponentRestoreService();
        var restore = await restoreService
            .RestoreAsync(context, forceComponents)
            .ConfigureAwait(false);

        var propsWriter = new GeneratedPropsWriter();
        var propsPath = await propsWriter
            .WriteAsync(context.Project, context.ProjectFilePath, context.TargetName, context.Configuration, restore.ComponentReferences)
            .ConfigureAwait(false);

        Console.WriteLine($"Lock file: {restore.LockFilePath}");
        Console.WriteLine($"Generated props: {propsPath}");
        Console.WriteLine($"Building target '{context.TargetName}' ({context.Configuration})...");

        return await ProcessRunner.RunAsync(
            "dotnet",
            ["build", context.HostProjectPath, "-c", context.Configuration, $"-p:KanataGeneratedProps={propsPath}"],
            context.ProjectRoot).ConfigureAwait(false);
    }
}
