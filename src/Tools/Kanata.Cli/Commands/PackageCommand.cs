using Kanata.Packaging;
using Kanata.Toolchain.Commands;
using Kanata.Toolchain.Packages;

namespace Kanata.Cli.Commands;

internal static class PackageCommand
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
            "info" => RunInfo(subcommandArgs),
            "verify" => RunVerify(subcommandArgs),
            "pack" => RunPack(subcommandArgs),
            "install" => RunInstall(subcommandArgs),
            "list" => RunList(subcommandArgs),
            "inspect" => RunInspect(subcommandArgs),
            _ => UnknownSubcommand(subcommand),
        };

        return Task.FromResult(result);
    }

    private static int RunInfo(string[] args)
    {
        if (args.Length != 1 || IsHelp(args[0]))
        {
            PrintInfoHelp();
            return args.Length == 1 && IsHelp(args[0]) ? 0 : 1;
        }

        var result = PackageCommands.OpenPackage(args[0]);
        if (!result.IsSuccess || result.Value is null)
        {
            return PrintMessages(result);
        }

        PrintPackageSummary(result.Value);
        return result.ExitCode;
    }

    private static int RunVerify(string[] args)
    {
        if (args.Length is < 1 or > 2 || args.Any(IsHelp))
        {
            PrintVerifyHelp();
            return args.Length == 1 && IsHelp(args[0]) ? 0 : 1;
        }

        var fast = args.Any(arg => string.Equals(arg, "--fast", StringComparison.Ordinal));
        var packagePath = args.FirstOrDefault(arg => !string.Equals(arg, "--fast", StringComparison.Ordinal));
        if (string.IsNullOrWhiteSpace(packagePath))
        {
            PrintVerifyHelp();
            return 1;
        }

        var result = PackageCommands.VerifyPackage(packagePath, fast);
        if (result.Value?.Package is not null)
        {
            PrintPackageSummary(result.Value.Package);
        }

        if (result.Value is { IsValid: false })
        {
            Console.WriteLine("Package is invalid.");
            foreach (var error in result.Value.Errors)
            {
                Console.WriteLine($" - {error}");
            }
        }
        else if (result.Value is { IsValid: true })
        {
            Console.WriteLine("Package is valid.");
        }
        else
        {
            PrintMessages(result);
        }

        return result.ExitCode;
    }

    private static int RunPack(string[] args)
    {
        if (args.Length == 0 || args.Any(IsHelp))
        {
            PrintPackHelp();
            return args.Length == 1 && IsHelp(args[0]) ? 0 : 1;
        }

        var sourceDirectory = args[0];
        string? outputPath = null;
        var overwrite = false;

        for (var index = 1; index < args.Length; index++)
        {
            var arg = args[index];
            if (string.Equals(arg, "--force", StringComparison.Ordinal))
            {
                overwrite = true;
                continue;
            }

            if (string.Equals(arg, "-o", StringComparison.Ordinal) || string.Equals(arg, "--output", StringComparison.Ordinal))
            {
                if (index + 1 >= args.Length)
                {
                    PrintPackHelp();
                    return 1;
                }

                outputPath = args[++index];
                continue;
            }

            Console.Error.WriteLine($"Unknown package pack option: {arg}");
            PrintPackHelp();
            return 1;
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            PrintPackHelp();
            return 1;
        }

        var result = PackageCommands.PackPackage(sourceDirectory, outputPath, overwrite);
        if (!result.IsSuccess || result.Value is null)
        {
            return PrintMessages(result);
        }

        Console.WriteLine($"Package written: {result.Value.OutputPath}");
        Console.WriteLine($"Package: {result.Value.PackageId}");
        Console.WriteLine($"Version: {result.Value.Version}");
        Console.WriteLine($"Installables: {result.Value.InstallableCount}");
        Console.WriteLine($"Payload files: {result.Value.PayloadFileCount}");
        Console.WriteLine($"Blocks: {result.Value.BlockCount}");
        Console.WriteLine($"Size: {result.Value.PackageLength} bytes");
        return result.ExitCode;
    }

    private static int RunInstall(string[] args)
    {
        if (args.Length is < 1 or > 2 || args.Any(IsHelp))
        {
            PrintInstallHelp();
            return args.Length == 1 && IsHelp(args[0]) ? 0 : 1;
        }

        var overwrite = args.Any(arg => string.Equals(arg, "--force", StringComparison.Ordinal));
        var packagePath = args.FirstOrDefault(arg => !string.Equals(arg, "--force", StringComparison.Ordinal));
        if (string.IsNullOrWhiteSpace(packagePath))
        {
            PrintInstallHelp();
            return 1;
        }

        var result = PackageCommands.InstallPackage(packagePath, overwrite);
        if (!result.IsSuccess || result.Value is null)
        {
            return PrintMessages(result);
        }

        Console.WriteLine($"Package installed: {result.Value.PackageId} {result.Value.Version}");
        Console.WriteLine($"Hash: {result.Value.PackageSha256}");
        Console.WriteLine($"Path: {result.Value.InstalledPath}");
        Console.WriteLine($"Installables: {result.Value.InstallableCount}");
        Console.WriteLine($"Files: {result.Value.FileCount}");
        return result.ExitCode;
    }

    private static int RunList(string[] args)
    {
        if (args.Length != 0)
        {
            PrintListHelp();
            return 1;
        }

        var result = PackageCommands.ListInstalledPackages();
        if (!result.IsSuccess || result.Value is null)
        {
            return PrintMessages(result);
        }

        Console.WriteLine($"Package store: {result.Value.StoreRoot}");
        if (result.Value.Packages.Count == 0)
        {
            Console.WriteLine("Installed packages: none");
            return 0;
        }

        Console.WriteLine("Installed packages:");
        foreach (var package in result.Value.Packages)
        {
            Console.WriteLine($" - {package.PackageId} {package.Version}");
            Console.WriteLine($"   Hash: {package.PackageSha256}");
            Console.WriteLine($"   Path: {package.InstalledPath}");
            Console.WriteLine("   Installables:");
            foreach (var installable in package.Installables)
            {
                Console.WriteLine($"    - {installable.Id} {installable.Version} {installable.Kind}");
            }
        }

        return result.ExitCode;
    }

    private static int RunInspect(string[] args)
    {
        if (args.Length > 1 || args.Any(IsHelp))
        {
            PrintInspectHelp();
            return args.Length == 1 && IsHelp(args[0]) ? 0 : 1;
        }

        var targetId = args.Length == 0 ? null : args[0];
        var result = PackageCommands.InspectInstalledPackages(targetId);
        if (!result.IsSuccess || result.Value is null)
        {
            return PrintMessages(result);
        }

        Console.WriteLine($"Package store: {result.Value.StoreRoot}");
        if (result.Value.Packages.Count == 0)
        {
            Console.WriteLine("Installed packages: none");
            return 0;
        }

        Console.WriteLine("Installed package inspection:");
        foreach (var package in result.Value.Packages)
        {
            PrintInstalledPackage(package);
        }

        return result.Value.IsUsable ? 0 : 1;
    }

    private static void PrintPackageSummary(PackageSummary package)
    {
        Console.WriteLine($"Package: {package.PackageId}");
        Console.WriteLine($"Version: {package.Version}");
        if (!string.IsNullOrWhiteSpace(package.DisplayName))
        {
            Console.WriteLine($"Display name: {package.DisplayName}");
        }

        if (!string.IsNullOrWhiteSpace(package.Description))
        {
            Console.WriteLine($"Description: {package.Description}");
        }

        Console.WriteLine($"Installables: {package.Installables.Count}");
        Console.WriteLine($"Blocks: {package.Blocks.Count}");
        Console.WriteLine($"Size: {package.PackageLength} bytes");
    }

    private static void PrintInstalledPackage(KpkgInstalledPackageInspection package)
    {
        Console.WriteLine($" - {package.PackageId} {package.Version}");
        Console.WriteLine($"   Status: {(package.IsUsable ? "usable" : "not usable")}");
        Console.WriteLine($"   Hash: {package.PackageSha256}");
        Console.WriteLine($"   Path: {package.InstalledPath}");
        foreach (var problem in package.Problems)
        {
            Console.WriteLine($"   Problem: {problem}");
        }

        Console.WriteLine("   Installables:");
        foreach (var installable in package.Installables)
        {
            Console.WriteLine($"    - {installable.Id} {installable.Version} {installable.Kind}");
            Console.WriteLine($"      Status: {(installable.IsUsable ? "usable" : "not usable")}");
            Console.WriteLine($"      Descriptor: {installable.DescriptorPath}");

            if (installable.Dependencies.Count > 0)
            {
                Console.WriteLine("      Dependencies:");
                foreach (var dependency in installable.Dependencies)
                {
                    Console.WriteLine($"       - {dependency.Id}: {(dependency.IsInstalled ? "installed" : "missing")}");
                }
            }

            Console.WriteLine($"      Artifacts: {installable.Artifacts.Count}");
            foreach (var artifact in installable.Artifacts)
            {
                Console.WriteLine($"       - {artifact.Role} {artifact.PackagePath}: {(artifact.Exists ? "found" : "missing")}");
            }

            if (installable.Surfaces.Count > 0)
            {
                Console.WriteLine($"      Surfaces: {installable.Surfaces.Count}");
                foreach (var surface in installable.Surfaces)
                {
                    Console.WriteLine($"       - {surface.Id} {surface.Kind}: {(surface.EntryPointExists ? "found" : "missing")}");
                }
            }

            foreach (var problem in installable.Problems)
            {
                Console.WriteLine($"      Problem: {problem}");
            }
        }
    }

    private static int PrintMessages<T>(ToolchainCommandResult<T> result)
    {
        foreach (var message in result.Messages)
        {
            if (message.Severity == ToolchainMessageSeverity.Error)
            {
                Console.Error.WriteLine(message.Text);
            }
            else
            {
                Console.WriteLine(message.Text);
            }
        }

        return result.ExitCode;
    }

    private static int UnknownSubcommand(string subcommand)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Unknown package subcommand: {subcommand}");
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
        Console.WriteLine("Kanata package commands");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  kanata package info <file.kpkg>");
        Console.WriteLine("  kanata package verify <file.kpkg> [--fast]");
        Console.WriteLine("  kanata package pack <source-folder> -o <output.kpkg> [--force]");
        Console.WriteLine("  kanata package install <file.kpkg> [--force]");
        Console.WriteLine("  kanata package list");
        Console.WriteLine("  kanata package inspect [package-or-installable-id]");
    }

    private static void PrintInfoHelp() => Console.WriteLine("Usage:\n  kanata package info <file.kpkg>");

    private static void PrintVerifyHelp() => Console.WriteLine("Usage:\n  kanata package verify <file.kpkg> [--fast]");

    private static void PrintPackHelp() => Console.WriteLine("Usage:\n  kanata package pack <source-folder> -o <output.kpkg> [--force]");

    private static void PrintInstallHelp() => Console.WriteLine("Usage:\n  kanata package install <file.kpkg> [--force]");

    private static void PrintListHelp() => Console.WriteLine("Usage:\n  kanata package list");

    private static void PrintInspectHelp() => Console.WriteLine("Usage:\n  kanata package inspect [package-or-installable-id]");
}
