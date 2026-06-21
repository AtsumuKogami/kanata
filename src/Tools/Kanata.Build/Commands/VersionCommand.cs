using Kanata.Build.Infrastructure;

namespace Kanata.Build.Commands;

/// <summary>
/// Implements the <c>version</c> command.
/// </summary>
public static class VersionCommand
{
    /// <summary>
    /// Prints the current Kanata toolchain version.
    /// </summary>
    /// <returns>Process exit code.</returns>
    public static int Run()
    {
        Console.WriteLine($"Kanata {KanataVersionProvider.CurrentVersion}");
        return 0;
    }
}
