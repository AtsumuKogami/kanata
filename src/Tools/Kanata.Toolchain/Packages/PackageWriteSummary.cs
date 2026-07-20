namespace Kanata.Toolchain.Packages;

/// <summary>
/// Describes a package written from a staging directory.
/// </summary>
public sealed class PackageWriteSummary
{
    /// <summary>
    /// Gets the output package path.
    /// </summary>
    public string OutputPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the package id.
    /// </summary>
    public string PackageId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the package version.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installable count.
    /// </summary>
    public int InstallableCount { get; init; }

    /// <summary>
    /// Gets the payload file count.
    /// </summary>
    public int PayloadFileCount { get; init; }

    /// <summary>
    /// Gets the block count.
    /// </summary>
    public int BlockCount { get; init; }

    /// <summary>
    /// Gets the final package length in bytes.
    /// </summary>
    public long PackageLength { get; init; }
}
