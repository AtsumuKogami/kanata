using Kanata.Packaging;
using Kanata.Toolchain.Commands;

namespace Kanata.Toolchain.Packages;

/// <summary>
/// Provides shared package command execution for CLI and GUI surfaces.
/// </summary>
public static class PackageCommands
{
    /// <summary>
    /// Opens a package and returns metadata without verifying payload hashes.
    /// </summary>
    /// <param name="packagePath">The package path.</param>
    /// <returns>The structured command result.</returns>
    public static ToolchainCommandResult<PackageSummary> OpenPackage(string packagePath)
    {
        try
        {
            var package = KpkgReader.ReadPackage(packagePath);
            return ToolchainCommandResult<PackageSummary>.Success(ToSummary(packagePath, package));
        }
        catch (Exception exception) when (IsExpectedPackageException(exception))
        {
            return Failure<PackageSummary>(exception.Message);
        }
    }

    /// <summary>
    /// Verifies a package and returns structured verification details.
    /// </summary>
    /// <param name="packagePath">The package path.</param>
    /// <param name="fast">Whether to skip full payload verification.</param>
    /// <returns>The structured command result.</returns>
    public static ToolchainCommandResult<PackageVerificationSummary> VerifyPackage(string packagePath, bool fast = false)
    {
        try
        {
            var mode = fast ? KpkgVerificationMode.Fast : KpkgVerificationMode.Full;
            var verification = KpkgVerifier.VerifyFile(packagePath, mode);
            var summary = new PackageVerificationSummary
            {
                IsValid = verification.IsValid,
                Errors = verification.Errors,
                Package = verification.Package is null ? null : ToSummary(packagePath, verification.Package),
            };

            if (verification.IsValid)
            {
                return ToolchainCommandResult<PackageVerificationSummary>.Success(
                    summary,
                    new ToolchainMessage(ToolchainMessageSeverity.Information, "Package is valid."));
            }

            return ToolchainCommandResult<PackageVerificationSummary>.Failure(
                1,
                new ToolchainMessage(
                    ToolchainMessageSeverity.Error,
                    $"Package verification failed: {string.Join("; ", verification.Errors)}"));
        }
        catch (Exception exception) when (IsExpectedPackageException(exception))
        {
            return Failure<PackageVerificationSummary>(exception.Message);
        }
    }

    /// <summary>
    /// Packs a staging directory into a Kanata package.
    /// </summary>
    /// <param name="sourceDirectory">The package staging directory.</param>
    /// <param name="outputPath">The output package path.</param>
    /// <param name="overwrite">Whether to overwrite an existing output file.</param>
    /// <returns>The structured command result.</returns>
    public static ToolchainCommandResult<PackageWriteSummary> PackPackage(
        string sourceDirectory,
        string outputPath,
        bool overwrite = false)
    {
        try
        {
            var result = KpkgWriter.PackDirectory(new KpkgWriterOptions
            {
                SourceDirectory = sourceDirectory,
                OutputPath = outputPath,
                Overwrite = overwrite,
            });

            return ToolchainCommandResult<PackageWriteSummary>.Success(new PackageWriteSummary
            {
                OutputPath = result.OutputPath,
                PackageId = result.PackageId,
                Version = result.Version,
                InstallableCount = result.InstallableCount,
                PayloadFileCount = result.PayloadFileCount,
                BlockCount = result.BlockCount,
                PackageLength = result.PackageLength,
            });
        }
        catch (Exception exception) when (IsExpectedPackageException(exception))
        {
            return Failure<PackageWriteSummary>(exception.Message);
        }
    }

    /// <summary>
    /// Installs a package into the local package store.
    /// </summary>
    /// <param name="packagePath">The package path.</param>
    /// <param name="overwrite">Whether to replace an already installed package instance.</param>
    /// <param name="storeRoot">The optional package store root.</param>
    /// <returns>The structured command result.</returns>
    public static ToolchainCommandResult<PackageInstallSummary> InstallPackage(
        string packagePath,
        bool overwrite = false,
        string? storeRoot = null)
    {
        try
        {
            var result = KpkgInstaller.Install(new KpkgInstallOptions
            {
                PackagePath = packagePath,
                Overwrite = overwrite,
                StoreRoot = storeRoot,
            });

            return ToolchainCommandResult<PackageInstallSummary>.Success(new PackageInstallSummary
            {
                PackageId = result.PackageId,
                Version = result.Version,
                PackageSha256 = result.PackageSha256,
                InstalledPath = result.InstalledPath,
                InstallableCount = result.InstallableCount,
                FileCount = result.FileCount,
            });
        }
        catch (Exception exception) when (IsExpectedPackageException(exception))
        {
            return Failure<PackageInstallSummary>(exception.Message);
        }
    }

    /// <summary>
    /// Lists packages installed in the local package store.
    /// </summary>
    /// <param name="storeRoot">The optional package store root.</param>
    /// <returns>The structured command result.</returns>
    public static ToolchainCommandResult<InstalledPackageListResult> ListInstalledPackages(string? storeRoot = null)
    {
        try
        {
            var store = KpkgPackageStore.Create(storeRoot);
            var registry = KpkgInstalledRegistry.Read(store);
            var packages = registry.Packages
                .OrderBy(package => package.PackageId, StringComparer.Ordinal)
                .ThenBy(package => package.Version, StringComparer.Ordinal)
                .Select(package => new InstalledPackageSummary
                {
                    PackageId = package.PackageId,
                    Version = package.Version,
                    PackageSha256 = package.PackageSha256,
                    InstalledPath = package.InstalledPath,
                    Installables = package.Installables
                        .Select(installable => new InstalledInstallableSummary(
                            installable.Id,
                            installable.Version,
                            installable.Kind))
                        .ToArray(),
                })
                .ToArray();

            return ToolchainCommandResult<InstalledPackageListResult>.Success(new InstalledPackageListResult
            {
                StoreRoot = store.RootPath,
                Packages = packages,
            });
        }
        catch (Exception exception) when (IsExpectedPackageException(exception))
        {
            return Failure<InstalledPackageListResult>(exception.Message);
        }
    }

    /// <summary>
    /// Inspects installed packages and checks artifact usability.
    /// </summary>
    /// <param name="targetId">The optional package id or installable id filter.</param>
    /// <param name="storeRoot">The optional package store root.</param>
    /// <returns>The structured command result.</returns>
    public static ToolchainCommandResult<KpkgPackageInspectionResult> InspectInstalledPackages(
        string? targetId = null,
        string? storeRoot = null)
    {
        try
        {
            var result = KpkgPackageInspector.Inspect(new KpkgPackageInspectorOptions
            {
                TargetId = targetId,
                StoreRoot = storeRoot,
            });

            if (!string.IsNullOrWhiteSpace(targetId) && result.Packages.Count == 0)
            {
                return ToolchainCommandResult<KpkgPackageInspectionResult>.Failure(
                    1,
                    new ToolchainMessage(ToolchainMessageSeverity.Error, $"Installed package not found: {targetId}"));
            }

            return ToolchainCommandResult<KpkgPackageInspectionResult>.Success(result);
        }
        catch (Exception exception) when (IsExpectedPackageException(exception))
        {
            return Failure<KpkgPackageInspectionResult>(exception.Message);
        }
    }

    private static PackageSummary ToSummary(string packagePath, KpkgPackage package)
    {
        return new PackageSummary
        {
            PackagePath = packagePath,
            PackageId = package.Manifest.PackageId,
            Version = package.Manifest.Version,
            DisplayName = package.Manifest.DisplayName,
            Description = package.Manifest.Description,
            Installables = package.Manifest.Installables.Select(installable => new PackageInstallableSummary(
                installable.Id,
                installable.Version,
                installable.Kind,
                installable.Description,
                installable.Provides,
                installable.Dependencies)).ToArray(),
            Blocks = package.Blocks.Select(block => new PackageBlockSummary(
                block.BlockId,
                block.KnownBlockType?.ToString() ?? $"unknown:{block.RawBlockType}",
                block.Offset,
                block.StoredLength,
                block.UncompressedLength)).ToArray(),
            PackageLength = package.Header.PackageLength,
        };
    }

    private static ToolchainCommandResult<T> Failure<T>(string message)
    {
        return ToolchainCommandResult<T>.Failure(
            1,
            new ToolchainMessage(ToolchainMessageSeverity.Error, message));
    }

    private static bool IsExpectedPackageException(Exception exception)
    {
        return exception is KpkgFormatException or IOException or UnauthorizedAccessException or ArgumentException;
    }
}
