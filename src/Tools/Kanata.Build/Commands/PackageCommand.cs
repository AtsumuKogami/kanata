using Kanata.Packaging;

namespace Kanata.Build.Commands;

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

        var package = KpkgReader.ReadPackage(args[0]);
        PrintPackageInfo(package);
        return 0;
    }

    private static int RunVerify(string[] args)
    {
        if (args.Length == 0 || args.Length > 2 || IsHelp(args[0]))
        {
            PrintVerifyHelp();
            return args.Length == 1 && IsHelp(args[0]) ? 0 : 1;
        }

        var path = args[0];
        var mode = KpkgVerificationMode.Full;
        if (args.Length == 2)
        {
            if (!string.Equals(args[1], "--fast", StringComparison.OrdinalIgnoreCase))
            {
                Console.Error.WriteLine($"Unknown package verify option: {args[1]}");
                PrintVerifyHelp();
                return 1;
            }

            mode = KpkgVerificationMode.Fast;
        }

        var result = KpkgVerifier.VerifyFile(path, mode);
        if (result.IsValid)
        {
            Console.WriteLine(mode == KpkgVerificationMode.Fast
                ? "Package structure is valid."
                : "Package is valid.");

            if (result.Package is not null)
            {
                Console.WriteLine($"Package: {result.Package.Manifest.PackageId}");
                Console.WriteLine($"Version: {result.Package.Manifest.Version}");
            }

            return 0;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine("Package verification failed:");
        foreach (var error in result.Errors)
        {
            Console.Error.WriteLine($" - {error}");
        }

        Console.ResetColor();
        return 1;
    }

    private static int RunPack(string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintPackHelp();
            return args.Length == 1 && IsHelp(args[0]) ? 0 : 1;
        }

        string? sourceDirectory = null;
        string? outputPath = null;
        var force = false;

        for (var index = 0; index < args.Length; index++)
        {
            var arg = args[index];
            if (string.Equals(arg, "-o", StringComparison.OrdinalIgnoreCase)
                || string.Equals(arg, "--output", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 >= args.Length)
                {
                    Console.Error.WriteLine($"Missing value for {arg}.");
                    PrintPackHelp();
                    return 1;
                }

                outputPath = args[++index];
                continue;
            }

            if (string.Equals(arg, "--force", StringComparison.OrdinalIgnoreCase))
            {
                force = true;
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"Unknown package pack option: {arg}");
                PrintPackHelp();
                return 1;
            }

            if (sourceDirectory is not null)
            {
                Console.Error.WriteLine("Only one package source directory may be specified.");
                PrintPackHelp();
                return 1;
            }

            sourceDirectory = arg;
        }

        if (sourceDirectory is null || outputPath is null)
        {
            PrintPackHelp();
            return 1;
        }

        var writeResult = KpkgWriter.PackDirectory(new KpkgWriterOptions
        {
            SourceDirectory = sourceDirectory,
            OutputPath = outputPath,
            Overwrite = force
        });

        var verifyResult = KpkgVerifier.VerifyFile(writeResult.OutputPath);
        if (!verifyResult.IsValid)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine("Package was written but failed verification:");
            foreach (var error in verifyResult.Errors)
            {
                Console.Error.WriteLine($" - {error}");
            }

            Console.ResetColor();
            return 1;
        }

        Console.WriteLine($"Package written: {writeResult.OutputPath}");
        Console.WriteLine($"Package: {writeResult.PackageId}");
        Console.WriteLine($"Version: {writeResult.Version}");
        Console.WriteLine($"Installables: {writeResult.InstallableCount}");
        Console.WriteLine($"Payload files: {writeResult.PayloadFileCount}");
        Console.WriteLine($"Blocks: {writeResult.BlockCount}");
        Console.WriteLine($"Size: {writeResult.PackageLength} bytes");
        return 0;
    }


    private static int RunInstall(string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            PrintInstallHelp();
            return args.Length == 1 && IsHelp(args[0]) ? 0 : 1;
        }

        string? packagePath = null;
        var force = false;

        foreach (var arg in args)
        {
            if (string.Equals(arg, "--force", StringComparison.OrdinalIgnoreCase))
            {
                force = true;
                continue;
            }

            if (arg.StartsWith("-", StringComparison.Ordinal))
            {
                Console.Error.WriteLine($"Unknown package install option: {arg}");
                PrintInstallHelp();
                return 1;
            }

            if (packagePath is not null)
            {
                Console.Error.WriteLine("Only one package file may be specified.");
                PrintInstallHelp();
                return 1;
            }

            packagePath = arg;
        }

        if (packagePath is null)
        {
            PrintInstallHelp();
            return 1;
        }

        var result = KpkgInstaller.Install(new KpkgInstallOptions
        {
            PackagePath = packagePath,
            Overwrite = force
        });

        Console.WriteLine("Package installed.");
        Console.WriteLine($"Package: {result.PackageId}");
        Console.WriteLine($"Version: {result.Version}");
        Console.WriteLine($"Installables: {result.InstallableCount}");
        Console.WriteLine($"Files: {result.FileCount}");
        Console.WriteLine($"Hash: {result.PackageSha256}");
        Console.WriteLine($"Installed path: {result.InstalledPath}");
        return 0;
    }

    private static int RunList(string[] args)
    {
        if (args.Length > 1 || (args.Length == 1 && !IsHelp(args[0])))
        {
            PrintListHelp();
            return 1;
        }

        if (args.Length == 1 && IsHelp(args[0]))
        {
            PrintListHelp();
            return 0;
        }

        var store = KpkgPackageStore.Create();
        var registry = KpkgInstalledRegistry.Read(store);

        Console.WriteLine($"Package store: {store.RootPath}");
        if (registry.Packages.Count == 0)
        {
            Console.WriteLine("Installed packages: none");
            return 0;
        }

        Console.WriteLine("Installed packages:");
        foreach (var package in registry.Packages)
        {
            Console.WriteLine($" - {package.PackageId} {package.Version}");
            Console.WriteLine($"   Hash: {package.PackageSha256}");
            Console.WriteLine($"   Path: {package.InstalledPath}");

            if (package.Installables.Count > 0)
            {
                Console.WriteLine("   Installables:");
                foreach (var installable in package.Installables)
                {
                    Console.WriteLine($"    - {installable.Id} {installable.Version} {installable.Kind}");
                }
            }
        }

        return 0;
    }

    private static int RunInspect(string[] args)
    {
        if (args.Length > 1 || (args.Length == 1 && IsHelp(args[0])))
        {
            PrintInspectHelp();
            return args.Length == 1 && IsHelp(args[0]) ? 0 : 1;
        }

        var targetId = args.Length == 1 ? args[0] : null;
        var result = KpkgPackageInspector.Inspect(new KpkgPackageInspectorOptions
        {
            TargetId = targetId
        });

        PrintInspectionResult(result, targetId);
        if (!string.IsNullOrWhiteSpace(targetId) && result.Packages.Count == 0)
        {
            return 1;
        }

        return result.IsUsable ? 0 : 1;
    }


    private static int UnknownSubcommand(string subcommand)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"Unknown package command: {subcommand}");
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
        Console.WriteLine("Kanata package commands:");
        Console.WriteLine(" kanata package info <file.kpkg>");
        Console.WriteLine(" kanata package verify <file.kpkg> [--fast]");
        Console.WriteLine(" kanata package pack <source-folder> -o <output.kpkg> [--force]");
        Console.WriteLine(" kanata package install <file.kpkg> [--force]");
        Console.WriteLine(" kanata package list");
        Console.WriteLine(" kanata package inspect [package-or-installable-id]");
    }

    private static void PrintInfoHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine(" kanata package info <file.kpkg>");
    }

    private static void PrintVerifyHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine(" kanata package verify <file.kpkg> [--fast]");
        Console.WriteLine();
        Console.WriteLine("By default verify performs full block, footer, file table and payload hash checks.");
        Console.WriteLine("Use --fast to check only structural metadata.");
    }

    private static void PrintInstallHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine(" kanata package install <file.kpkg> [--force]");
        Console.WriteLine();
        Console.WriteLine("Installs an artifact-first package into the local Kanata package store.");
        Console.WriteLine("This command does not fetch sourceRefs and does not build sources.");
    }

    private static void PrintListHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine(" kanata package list");
        Console.WriteLine();
        Console.WriteLine("Lists packages installed in the local Kanata package store.");
    }

    private static void PrintInspectHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine(" kanata package inspect [package-or-installable-id]");
        Console.WriteLine();
        Console.WriteLine("Inspects installed package descriptors, artifacts, command entry points, UI surfaces and dependencies.");
        Console.WriteLine("This command does not run artifacts, fetch sourceRefs or build sources.");
    }

    private static void PrintPackHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine(" kanata package pack <source-folder> -o <output.kpkg> [--force]");
        Console.WriteLine();
        Console.WriteLine("Expected source folder layout:");
        Console.WriteLine(" <source-folder>/package.kmanifest");
        Console.WriteLine(" <source-folder>/descriptors/*.ktool|*.kbackend|*.kcomponent|...");
        Console.WriteLine(" <source-folder>/artifacts/**");
        Console.WriteLine(" <source-folder>/sources/**");
        Console.WriteLine();
        Console.WriteLine("artifacts and sources are both optional, but at least one descriptor is required.");
    }


    private static void PrintInspectionResult(KpkgPackageInspectionResult result, string? targetId)
    {
        Console.WriteLine($"Package store: {result.StoreRoot}");

        if (result.Packages.Count == 0)
        {
            Console.WriteLine(string.IsNullOrWhiteSpace(targetId)
                ? "Installed packages: none"
                : $"Installed package or installable not found: {targetId}");
            return;
        }

        Console.WriteLine("Installed package inspection:");
        foreach (var package in result.Packages)
        {
            Console.WriteLine($" - {package.PackageId} {package.Version}");
            Console.WriteLine($"   Status: {(package.IsUsable ? "usable" : "not usable")}");
            Console.WriteLine($"   Hash: {package.PackageSha256}");
            Console.WriteLine($"   Path: {package.InstalledPath}");

            foreach (var problem in package.Problems)
            {
                Console.WriteLine($"   Problem: {problem}");
            }

            if (package.Installables.Count == 0)
            {
                Console.WriteLine("   Installables: none");
                continue;
            }

            Console.WriteLine("   Installables:");
            foreach (var installable in package.Installables)
            {
                PrintInstallableInspection(installable);
            }
        }
    }

    private static void PrintInstallableInspection(KpkgInstalledInstallableInspection installable)
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

        if (installable.Commands.Count > 0)
        {
            Console.WriteLine("      Commands:");
            foreach (var command in installable.Commands)
            {
                Console.WriteLine($"       - {command.Name} ({command.EntryPointKind}) {command.EntryPointPackagePath}: {(command.EntryPointExists ? "found" : "missing")}");
            }
        }

        if (installable.Surfaces.Count > 0)
        {
            Console.WriteLine("      Surfaces:");
            foreach (var surface in installable.Surfaces)
            {
                var title = string.IsNullOrWhiteSpace(surface.Title) ? surface.Id : surface.Title;
                Console.WriteLine($"       - {surface.Id} {surface.Kind} '{title}' ({surface.EntryPointKind}) {surface.EntryPointPackagePath}: {(surface.EntryPointExists ? "found" : "missing")} {(surface.Optional ? "optional" : "required")}");
            }
        }

        if (installable.Sources.Count > 0 || installable.SourceReferenceCount > 0)
        {
            Console.WriteLine($"      Embedded sources: {installable.Sources.Count}");
            Console.WriteLine($"      Source refs: {installable.SourceReferenceCount}");
        }

        foreach (var problem in installable.Problems)
        {
            Console.WriteLine($"      Problem: {problem}");
        }
    }

    private static void PrintPackageInfo(KpkgPackage package)
    {
        Console.WriteLine($"Package: {package.Manifest.PackageId}");
        Console.WriteLine($"Version: {package.Manifest.Version}");

        if (!string.IsNullOrWhiteSpace(package.Manifest.DisplayName))
        {
            Console.WriteLine($"Display name: {package.Manifest.DisplayName}");
        }

        if (!string.IsNullOrWhiteSpace(package.Manifest.Description))
        {
            Console.WriteLine($"Description: {package.Manifest.Description}");
        }

        Console.WriteLine($"Format: {package.Header.FormatMajor}.{package.Header.FormatMinor}");
        Console.WriteLine($"Blocks: {package.Blocks.Count}");
        Console.WriteLine();
        Console.WriteLine("Installables:");

        foreach (var installable in package.Manifest.Installables)
        {
            Console.WriteLine($" - {installable.Id}");
            Console.WriteLine($"   Version: {installable.Version}");
            Console.WriteLine($"   Kind: {installable.Kind}");
            Console.WriteLine($"   Descriptor block: {installable.DescriptorBlockId}");

            if (!string.IsNullOrWhiteSpace(installable.Description))
            {
                Console.WriteLine($"   Description: {installable.Description}");
            }

            if (installable.Provides.Count > 0)
            {
                Console.WriteLine($"   Provides: {string.Join(", ", installable.Provides)}");
            }

            if (installable.Dependencies.Count > 0)
            {
                Console.WriteLine($"   Dependencies: {string.Join(", ", installable.Dependencies)}");
            }

            if (installable.Compatibility is not null)
            {
                if (!string.IsNullOrWhiteSpace(installable.Compatibility.KanataToolVersion))
                {
                    Console.WriteLine($"   Kanata Tool: {installable.Compatibility.KanataToolVersion}");
                }

                if (installable.Compatibility.Platforms.Count > 0)
                {
                    Console.WriteLine($"   Platforms: {string.Join(", ", installable.Compatibility.Platforms)}");
                }

                if (installable.Compatibility.Architectures.Count > 0)
                {
                    Console.WriteLine($"   Architectures: {string.Join(", ", installable.Compatibility.Architectures)}");
                }
            }

            if (installable.GameParticipation is not null)
            {
                Console.WriteLine($"   Game build graph: {(installable.GameParticipation.Build ? "yes" : "no")}");
                Console.WriteLine($"   Game runtime graph: {(installable.GameParticipation.Runtime ? "yes" : "no")}");
            }
        }
    }
}
