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
            "inspect" => RunInspect(subcommandArgs),
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

        var registry = KpkgToolRegistry.Read();
        Console.WriteLine($"Package store: {registry.StoreRoot}");

        if (registry.Tools.Count == 0)
        {
            Console.WriteLine("Installed tool packages: none");
            return 0;
        }

        Console.WriteLine("Installed tool packages:");
        foreach (var tool in registry.Tools)
        {
            Console.WriteLine($" - {tool.Id} {tool.Version}");
            Console.WriteLine($"   Status: {(tool.IsUsable ? "usable" : "not usable")}");
            Console.WriteLine($"   Package: {tool.PackageId} {tool.PackageVersion}");
            Console.WriteLine($"   Path: {tool.InstalledPath}");

            if (tool.Commands.Count > 0)
            {
                Console.WriteLine($"   Commands: {string.Join(", ", tool.Commands.Select(command => command.Name))}");
            }

            if (tool.Surfaces.Count > 0)
            {
                Console.WriteLine($"   Surfaces: {string.Join(", ", tool.Surfaces.Select(FormatSurfaceSummary))}");
            }
        }

        if (registry.Problems.Count > 0)
        {
            Console.WriteLine("Registry problems:");
            foreach (var problem in registry.Problems)
            {
                Console.WriteLine($" - {problem}");
            }
        }

        return registry.Problems.Count == 0 && registry.Tools.All(tool => tool.IsUsable) ? 0 : 1;
    }

    private static int RunInspect(string[] args)
    {
        if (args.Length != 1 || IsHelp(args[0]))
        {
            PrintInspectHelp();
            return args.Length == 1 && IsHelp(args[0]) ? 0 : 1;
        }

        var targetId = args[0];
        var registry = KpkgToolRegistry.Read(new KpkgToolRegistryOptions { TargetId = targetId });
        Console.WriteLine($"Package store: {registry.StoreRoot}");

        if (registry.Tools.Count == 0)
        {
            Console.WriteLine($"Installed tool package not found: {targetId}");
            return 1;
        }

        Console.WriteLine("Installed tool inspection:");
        foreach (var tool in registry.Tools)
        {
            PrintTool(tool);
        }

        if (registry.Problems.Count > 0)
        {
            Console.WriteLine("Registry problems:");
            foreach (var problem in registry.Problems)
            {
                Console.WriteLine($" - {problem}");
            }
        }

        return registry.Problems.Count == 0 && registry.Tools.All(tool => tool.IsUsable) ? 0 : 1;
    }

    private static void PrintTool(KpkgInstalledToolRecord tool)
    {
        Console.WriteLine($" - {tool.Id} {tool.Version}");
        Console.WriteLine($"   Status: {(tool.IsUsable ? "usable" : "not usable")}");
        Console.WriteLine($"   Package: {tool.PackageId} {tool.PackageVersion}");
        Console.WriteLine($"   Hash: {tool.PackageSha256}");
        Console.WriteLine($"   Path: {tool.InstalledPath}");
        Console.WriteLine($"   Descriptor: {tool.DescriptorPath}");

        if (tool.Provides.Count > 0)
        {
            Console.WriteLine($"   Provides: {string.Join(", ", tool.Provides)}");
        }

        if (tool.Commands.Count == 0)
        {
            Console.WriteLine("   Commands: none");
        }
        else
        {
            Console.WriteLine("   Commands:");
            foreach (var command in tool.Commands)
            {
                Console.WriteLine($"    - {command.Name}");
                if (!string.IsNullOrWhiteSpace(command.Description))
                {
                    Console.WriteLine($"      Description: {command.Description}");
                }

                if (command.Aliases.Count > 0)
                {
                    Console.WriteLine($"      Aliases: {string.Join(", ", command.Aliases)}");
                }

                Console.WriteLine($"      Entry point: {command.EntryPointKind} {command.EntryPointPackagePath}: {(command.EntryPointExists ? "found" : "missing")}");
                Console.WriteLine($"      Launch mode: {command.LaunchMode}");
                Console.WriteLine($"      Required: {(command.Required ? "yes" : "no")}");
            }
        }

        if (tool.Surfaces.Count == 0)
        {
            Console.WriteLine("   Surfaces: none");
        }
        else
        {
            Console.WriteLine("   Surfaces:");
            foreach (var surface in tool.Surfaces)
            {
                Console.WriteLine($"    - {surface.Id} {surface.Kind}");
                if (!string.IsNullOrWhiteSpace(surface.Title))
                {
                    Console.WriteLine($"      Title: {surface.Title}");
                }

                if (!string.IsNullOrWhiteSpace(surface.Description))
                {
                    Console.WriteLine($"      Description: {surface.Description}");
                }

                Console.WriteLine($"      Entry point: {surface.EntryPointKind} {surface.EntryPointPackagePath}: {(surface.EntryPointExists ? "found" : "missing")}");
                Console.WriteLine($"      Optional: {(surface.Optional ? "yes" : "no")}");

                if (surface.Platforms.Count > 0)
                {
                    Console.WriteLine($"      Platforms: {string.Join(", ", surface.Platforms)}");
                }
            }
        }

        foreach (var problem in tool.Problems)
        {
            Console.WriteLine($"   Problem: {problem}");
        }
    }

    private static string FormatSurfaceSummary(KpkgToolSurfaceRecord surface)
    {
        var title = string.IsNullOrWhiteSpace(surface.Title) ? surface.Id : surface.Title;
        return $"{surface.Kind}:{title}";
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
        Console.WriteLine(" kanata tool inspect <tool-id>");
        Console.WriteLine();
        Console.WriteLine("Tool command routing is a bootstrap responsibility. This command reports installed tool packages, CLI commands and optional UI surfaces from the local package store.");
    }

    private static void PrintListHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine(" kanata tool list");
    }

    private static void PrintInspectHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine(" kanata tool inspect <tool-id>");
    }
}
