namespace Kanata.Toolchain.Packages;

/// <summary>
/// Describes one package installed in the local Kanata package store.
/// </summary>
public sealed class InstalledPackageSummary
{
    /// <summary>
    /// Gets the installed package id.
    /// </summary>
    public string PackageId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installed package version.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installed package SHA-256 hash.
    /// </summary>
    public string PackageSha256 { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installed package path.
    /// </summary>
    public string InstalledPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets installed installables.
    /// </summary>
    public IReadOnlyList<InstalledInstallableSummary> Installables { get; init; } = [];
}
