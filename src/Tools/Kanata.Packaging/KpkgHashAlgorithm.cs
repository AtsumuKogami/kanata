namespace Kanata.Packaging;

/// <summary>
/// Defines hash algorithms supported by the Kanata package format.
/// </summary>
public enum KpkgHashAlgorithm : ushort
{
    /// <summary>
    /// No hash algorithm.
    /// </summary>
    None = 0,

    /// <summary>
    /// SHA-256 hash algorithm.
    /// </summary>
    Sha256 = 1
}
