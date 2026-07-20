namespace Kanata.Packaging;

/// <summary>
/// Provides options for installing a Kanata package into a local package store.
/// </summary>
public sealed class KpkgInstallOptions
{
    /// <summary>
    /// Gets or sets the package file path.
    /// </summary>
    public string PackagePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the package store root. When empty, the default user package store is used.
    /// </summary>
    public string? StoreRoot { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an existing installed package entry may be replaced.
    /// </summary>
    public bool Overwrite { get; set; }
}
