namespace Kanata.Toolchain.Packages;

/// <summary>
/// Describes an installed Kanata package.
/// </summary>
public sealed class PackageInstallSummary
{
    /// <summary>
    /// Gets the package id.
    /// </summary>
    public string PackageId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the package version.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets the package SHA-256 hash.
    /// </summary>
    public string PackageSha256 { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installed package path.
    /// </summary>
    public string InstalledPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installable count.
    /// </summary>
    public int InstallableCount { get; init; }

    /// <summary>
    /// Gets the installed file count.
    /// </summary>
    public int FileCount { get; init; }
}
