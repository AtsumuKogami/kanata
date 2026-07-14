namespace Kanata.Packaging;

/// <summary>
/// Defines package verification depth.
/// </summary>
public enum KpkgVerificationMode
{
    /// <summary>
    /// Checks structural metadata without payload file hash verification.
    /// </summary>
    Fast,

    /// <summary>
    /// Checks structure, block hashes, footer hash, file table and payload file hashes.
    /// </summary>
    Full
}
