namespace Kanata.Packaging;

/// <summary>
/// Provides options for inspecting packages installed in the local Kanata package store.
/// </summary>
public sealed class KpkgPackageInspectorOptions
{
    /// <summary>
    /// Gets or sets the optional package store root path.
    /// </summary>
    public string? StoreRoot { get; init; }

    /// <summary>
    /// Gets or sets an optional package id or installable id filter.
    /// </summary>
    public string? TargetId { get; init; }
}
