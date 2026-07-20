namespace Kanata.Packaging;

/// <summary>
/// Represents the result of inspecting the local installed package store.
/// </summary>
public sealed class KpkgPackageInspectionResult
{
    /// <summary>
    /// Gets the inspected package store root path.
    /// </summary>
    public string StoreRoot { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installed package inspections.
    /// </summary>
    public IReadOnlyList<KpkgInstalledPackageInspection> Packages { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether every inspected package is usable.
    /// </summary>
    public bool IsUsable => Packages.All(package => package.IsUsable);
}
