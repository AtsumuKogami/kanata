using Kanata.Build.Components;
using Kanata.Build.Infrastructure;

namespace Kanata.Build.Commands;

/// <summary>
/// Implements commands for building Kanata source components.
/// </summary>
public static class EngineCommand
{
    /// <summary>
    /// Runs an engine command such as <c>build</c> or <c>status</c>.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Process exit code.</returns>
    public static async Task<int> RunAsync(IReadOnlyList<string> args)
    {
        var options = CommandLineOptions.Parse(args);
        var action = options.Positionals.Count > 0 ? options.Positionals[0].ToLowerInvariant() : "build";
        var configuration = options.Positionals.Count > 1 ? options.Positionals[1] : "Debug";
        var force = options.HasFlag("force");

        return action switch
        {
            "build" => await BuildAsync(configuration, force).ConfigureAwait(false),
            "status" => await StatusAsync(configuration).ConfigureAwait(false),
            _ => UnknownAction(action),
        };
    }

    private static async Task<int> BuildAsync(string configuration, bool force)
    {
        var coordinator = new ComponentBuildCoordinator();
        var results = await coordinator
            .EnsureAllBundledComponentsAsync(configuration, force)
            .ConfigureAwait(false);

        var built = results.Count(result => result.WasBuilt);
        var skipped = results.Count(result => result.WasSkipped);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Engine components ready. Built: {built}. Cached: {skipped}.");
        Console.ResetColor();

        return 0;
    }

    private static async Task<int> StatusAsync(string configuration)
    {
        var coordinator = new ComponentBuildCoordinator();
        var results = await coordinator
            .EnsureAllBundledComponentsAsync(configuration, force: false)
            .ConfigureAwait(false);

        Console.WriteLine($"Engine component status ({configuration}):");
        foreach (var result in results)
        {
            var state = result.WasSkipped ? "cached" : "built";
            Console.WriteLine($"  {result.Reference.Id} {result.Reference.Version}: {state}");
        }

        return 0;
    }

    private static int UnknownAction(string action)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Unknown engine action: {action}");
        Console.ResetColor();
        Console.WriteLine("Usage:");
        Console.WriteLine("  kanata engine build [configuration] [--force]");
        Console.WriteLine("  kanata engine status [configuration]");
        return 1;
    }
}
