namespace Kanata.Toolchain.Packages;

/// <summary>
/// Describes one block in a Kanata package block table for UI and CLI rendering.
/// </summary>
/// <param name="BlockId">The block id.</param>
/// <param name="Type">The known or raw block type name.</param>
/// <param name="Offset">The block offset in bytes.</param>
/// <param name="StoredLength">The stored block length in bytes.</param>
/// <param name="UncompressedLength">The uncompressed block length in bytes.</param>
public sealed record PackageBlockSummary(
    uint BlockId,
    string Type,
    ulong Offset,
    ulong StoredLength,
    ulong UncompressedLength);
