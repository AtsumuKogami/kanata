namespace Kanata.Packaging;

/// <summary>
/// Represents one inspected installed package.
/// </summary>
public sealed class KpkgInstalledPackageInspection
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
    /// Gets the installed package root path.
    /// </summary>
    public string InstalledPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the inspected installables.
    /// </summary>
    public IReadOnlyList<KpkgInstalledInstallableInspection> Installables { get; init; } = [];

    /// <summary>
    /// Gets package-level problems found by the inspector.
    /// </summary>
    public IReadOnlyList<string> Problems { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the package looks usable from installed artifacts.
    /// </summary>
    public bool IsUsable => Problems.Count == 0 && Installables.Count > 0 && Installables.All(installable => installable.IsUsable);
}
