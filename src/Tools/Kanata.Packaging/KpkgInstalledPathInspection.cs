namespace Kanata.Packaging;

/// <summary>
/// Represents an installed artifact or embedded source path inspection.
/// </summary>
public sealed class KpkgInstalledPathInspection
{
    /// <summary>
    /// Gets the logical role declared by the descriptor.
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Gets the package path declared by the descriptor.
    /// </summary>
    public string PackagePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the resolved local filesystem path.
    /// </summary>
    public string LocalPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the resolved local path exists.
    /// </summary>
    public bool Exists { get; init; }
}
