namespace Kanata.Packaging;

/// <summary>
/// Represents package metadata loaded from a Kanata package file.
/// </summary>
public sealed class KpkgPackage
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KpkgPackage"/> class.
    /// </summary>
    /// <param name="header">The package header.</param>
    /// <param name="footer">The package footer.</param>
    /// <param name="blocks">The package block table entries.</param>
    /// <param name="manifest">The package manifest.</param>
    public KpkgPackage(
        KpkgHeader header,
        KpkgFooter footer,
        IReadOnlyList<KpkgBlockTableEntry> blocks,
        KpkgPackageManifest manifest)
    {
        Header = header;
        Footer = footer;
        Blocks = blocks;
        Manifest = manifest;
    }

    /// <summary>
    /// Gets the package header.
    /// </summary>
    public KpkgHeader Header { get; }

    /// <summary>
    /// Gets the package footer.
    /// </summary>
    public KpkgFooter Footer { get; }

    /// <summary>
    /// Gets all package block table entries.
    /// </summary>
    public IReadOnlyList<KpkgBlockTableEntry> Blocks { get; }

    /// <summary>
    /// Gets the package manifest.
    /// </summary>
    public KpkgPackageManifest Manifest { get; }
}
