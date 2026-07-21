namespace Kanata.Toolchain.Packages;

/// <summary>
/// Describes one payload file declared by a Kanata package file table.
/// </summary>
/// <param name="Path">The canonical package path.</param>
/// <param name="Length">The uncompressed file length in bytes.</param>
/// <param name="StoredLength">The stored file length in bytes.</param>
/// <param name="Compression">The file compression algorithm.</param>
/// <param name="Sha256">The SHA-256 hash of the uncompressed file bytes.</param>
/// <param name="PayloadBlockId">The payload block containing this file.</param>
public sealed record PackageFileSummary(
    string Path,
    ulong Length,
    ulong StoredLength,
    string Compression,
    string Sha256,
    uint PayloadBlockId);
