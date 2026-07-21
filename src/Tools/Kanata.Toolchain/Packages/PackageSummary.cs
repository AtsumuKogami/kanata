namespace Kanata.Toolchain.Packages;

/// <summary>
/// Describes a Kanata package without exposing binary package internals to renderers.
/// </summary>
public sealed class PackageSummary
{
    /// <summary>
    /// Gets the source package file path when this summary was loaded from disk.
    /// </summary>
    public string PackagePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the package id.
    /// </summary>
    public string PackageId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the package version.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets the optional display name.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Gets the optional package description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets package installables.
    /// </summary>
    public IReadOnlyList<PackageInstallableSummary> Installables { get; init; } = [];

    /// <summary>
    /// Gets package payload files from the file table.
    /// </summary>
    public IReadOnlyList<PackageFileSummary> Files { get; init; } = [];

    /// <summary>
    /// Gets package blocks.
    /// </summary>
    public IReadOnlyList<PackageBlockSummary> Blocks { get; init; } = [];

    /// <summary>
    /// Gets the package length in bytes.
    /// </summary>
    public ulong PackageLength { get; init; }
}
