using Kanata.Build.Infrastructure;
using Kanata.Build.Restore;

namespace Kanata.Build.Commands;

/// <summary>
/// Implements the <c>restore</c> command.
/// </summary>
public static class RestoreCommand
{
    /// <summary>
    /// Validates a project, resolves components, builds missing artifacts, and writes the lock file.
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
        var result = await restoreService
            .RestoreAsync(context, forceComponents)
            .ConfigureAwait(false);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Restore completed.");
        Console.ResetColor();
        Console.WriteLine($"Lock file: {result.LockFilePath}");
        Console.WriteLine($"Components: {result.ComponentReferences.Count}. Built: {result.BuiltCount}. Cached: {result.CachedCount}.");

        return 0;
    }
}
