using Kanata.Packaging;

namespace Kanata.Cli.Commands;

internal static class ToolCommand
{
    public static Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintHelp();
            return Task.FromResult(0);
        }

        var subcommand = args[0].ToLowerInvariant();
        var subcommandArgs = args.Skip(1).ToArray();
        var result = subcommand switch
        {
            "list" => RunList(subcommandArgs),
            _ => UnknownSubcommand(subcommand),
        };

        return Task.FromResult(result);
    }

    private static int RunList(string[] args)
    {
        if (args.Length != 0)
        {
            PrintListHelp();
            return 1;
        }

        var store = KpkgPackageStore.Create();
        var registry = KpkgInstalledRegistry.Read(store);
        var toolInstallables = registry.Packages
            .SelectMany(package => package.Installables.Select(installable => new
            {
                Package = package,
                Installable = installable
            }))
            .Where(entry => string.Equals(entry.Installable.Kind, "tool", StringComparison.OrdinalIgnoreCase))
            .OrderBy(entry => entry.Installable.Id, StringComparer.Ordinal)
            .ThenBy(entry => entry.Installable.Version, StringComparer.Ordinal)
            .ToArray();

        if (toolInstallables.Length == 0)
        {
            Console.WriteLine("No installed tool packages found.");
            Console.WriteLine($"Store: {store.RootPath}");
            return 0;
        }

        Console.WriteLine("Installed tool packages:");
        foreach (var entry in toolInstallables)
        {
            Console.WriteLine($" - {entry.Installable.Id} {entry.Installable.Version}");
            Console.WriteLine($"   Package: {entry.Package.PackageId} {entry.Package.Version}");
            Console.WriteLine($"   Path: {entry.Package.InstalledPath}");
        }

        return 0;
    }

    private static int UnknownSubcommand(string subcommand)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Unknown tool subcommand: {subcommand}");
        Console.ResetColor();
        PrintHelp();
        return 1;
    }

    private static bool IsHelp(string value)
    {
        return string.Equals(value, "help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "--help", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "-h", StringComparison.OrdinalIgnoreCase);
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Kanata tool commands");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine(" kanata tool list");
        Console.WriteLine();
        Console.WriteLine("Tool command routing is a bootstrap responsibility. This command currently reports installed tool packages from the local package store.");
    }

    private static void PrintListHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine(" kanata tool list");
    }
}
