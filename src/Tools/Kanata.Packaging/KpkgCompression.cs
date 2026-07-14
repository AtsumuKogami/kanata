namespace Kanata.Packaging;

/// <summary>
/// Defines compression algorithms used by package blocks or file entries.
/// </summary>
public enum KpkgCompression : ushort
{
    /// <summary>
    /// Content is stored without compression.
    /// </summary>
    None = 0,

    /// <summary>
    /// Content is compressed with Brotli.
    /// </summary>
    Brotli = 1
}
