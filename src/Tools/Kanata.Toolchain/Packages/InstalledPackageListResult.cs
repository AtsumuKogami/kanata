namespace Kanata.Toolchain.Packages;

/// <summary>
/// Describes the local installed package registry state.
/// </summary>
public sealed class InstalledPackageListResult
{
    /// <summary>
    /// Gets the package store root path.
    /// </summary>
    public string StoreRoot { get; init; } = string.Empty;

    /// <summary>
    /// Gets installed packages.
    /// </summary>
    public IReadOnlyList<InstalledPackageSummary> Packages { get; init; } = [];
}
