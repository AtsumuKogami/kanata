namespace Kanata.Packaging;

/// <summary>
/// Represents one entry in the Kanata package block table.
/// </summary>
public sealed record KpkgBlockTableEntry(
    uint BlockId,
    uint RawBlockType,
    KpkgBlockFlags Flags,
    KpkgCompression Compression,
    KpkgHashAlgorithm HashAlgorithm,
    ulong Offset,
    ulong StoredLength,
    ulong UncompressedLength,
    byte[] Sha256)
{
    /// <summary>
    /// Gets the known block type when the raw value is known by this implementation.
    /// </summary>
    public KpkgBlockType? KnownBlockType => Enum.IsDefined(typeof(KpkgBlockType), RawBlockType)
        ? (KpkgBlockType)RawBlockType
        : null;

    /// <summary>
    /// Gets a value indicating whether the block type is known by this implementation.
    /// </summary>
    public bool IsKnownBlockType => KnownBlockType.HasValue;

    /// <summary>
    /// Reads a block table entry from the current stream position.
    /// </summary>
    /// <param name="reader">The binary reader.</param>
    /// <returns>The parsed block table entry.</returns>
    public static KpkgBlockTableEntry Read(BinaryReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var entry = new KpkgBlockTableEntry(
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            (KpkgBlockFlags)reader.ReadUInt32(),
            (KpkgCompression)reader.ReadUInt16(),
            (KpkgHashAlgorithm)reader.ReadUInt16(),
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            reader.ReadBytes(32));

        var reservedBytes = reader.ReadBytes(24);
        if (reservedBytes.Any(value => value != 0))
        {
            throw new KpkgFormatException("Block table entry reserved bytes must be zero.");
        }

        entry.Validate();
        return entry;
    }

    /// <summary>
    /// Validates this block table entry independently from other entries.
    /// </summary>
    public void Validate()
    {
        if (BlockId == 0)
        {
            throw new KpkgFormatException("Block id must be non-zero.");
        }

        if (!IsKnownBlockType && Flags.HasFlag(KpkgBlockFlags.Critical))
        {
            throw new KpkgFormatException($"Unknown critical block type: {RawBlockType}.");
        }

        if (Compression != KpkgCompression.None)
        {
            throw new KpkgFormatException("Block-level compression is not supported in KPKG V1.");
        }

        if (HashAlgorithm != KpkgHashAlgorithm.Sha256)
        {
            throw new KpkgFormatException("Unsupported block hash algorithm.");
        }

        if (Sha256.Length != 32)
        {
            throw new KpkgFormatException("Block SHA-256 hash must be 32 bytes.");
        }
    }

    /// <summary>
    /// Gets the exclusive end offset of the block.
    /// </summary>
    /// <returns>The block end offset.</returns>
    public ulong GetEndOffset() => Offset + StoredLength;
}
