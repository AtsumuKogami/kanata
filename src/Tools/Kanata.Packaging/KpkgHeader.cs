namespace Kanata.Packaging;

/// <summary>
/// Represents the fixed V1 Kanata package header.
/// </summary>
public sealed record KpkgHeader(
    ushort FormatMajor,
    ushort FormatMinor,
    uint HeaderLength,
    uint HeaderFlags,
    uint BlockCount,
    ulong BlockTableOffset,
    ulong BlockTableLength,
    ulong PackageLength,
    ulong FooterOffset,
    uint ManifestBlockId,
    uint FileTableBlockId,
    uint IntegrityBlockId)
{
    /// <summary>
    /// Reads and validates the package header from the current stream position.
    /// </summary>
    /// <param name="reader">The binary reader.</param>
    /// <param name="actualPackageLength">The actual stream length in bytes.</param>
    /// <returns>The parsed package header.</returns>
    public static KpkgHeader Read(BinaryReader reader, long actualPackageLength)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var magic = reader.ReadBytes(KpkgConstants.Magic.Length);
        if (!magic.SequenceEqual(KpkgConstants.Magic))
        {
            throw new KpkgFormatException("Invalid KPKG magic.");
        }

        var header = new KpkgHeader(
            reader.ReadUInt16(),
            reader.ReadUInt16(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            reader.ReadUInt64(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32());

        var reserved = reader.ReadUInt32();
        if (reserved != 0)
        {
            throw new KpkgFormatException("Header reserved field must be zero.");
        }

        var reservedBytes = reader.ReadBytes(56);
        if (reservedBytes.Any(value => value != 0))
        {
            throw new KpkgFormatException("Header reserved bytes must be zero.");
        }

        header.Validate(actualPackageLength);
        return header;
    }

    /// <summary>
    /// Validates the header against V1 structural rules.
    /// </summary>
    /// <param name="actualPackageLength">The actual stream length in bytes.</param>
    public void Validate(long actualPackageLength)
    {
        if (FormatMajor != KpkgConstants.FormatMajor)
        {
            throw new KpkgFormatException($"Unsupported KPKG format major version: {FormatMajor}.");
        }

        if (HeaderLength != KpkgConstants.HeaderLength)
        {
            throw new KpkgFormatException($"Unsupported KPKG header length: {HeaderLength}.");
        }

        if (PackageLength != (ulong)actualPackageLength)
        {
            throw new KpkgFormatException("Header package length does not match the stream length.");
        }

        if (BlockCount == 0)
        {
            throw new KpkgFormatException("KPKG block table must contain at least one block.");
        }

        if (BlockTableOffset < HeaderLength)
        {
            throw new KpkgFormatException("Block table overlaps the package header.");
        }

        if (BlockTableLength != BlockCount * KpkgConstants.BlockTableEntryLength)
        {
            throw new KpkgFormatException("Block table length does not match block count.");
        }

        if (BlockTableOffset + BlockTableLength > FooterOffset)
        {
            throw new KpkgFormatException("Block table range is outside the package content range.");
        }

        if (FooterOffset + KpkgConstants.FooterLength > PackageLength)
        {
            throw new KpkgFormatException("Footer range is outside the package file.");
        }

        if (ManifestBlockId == 0 || FileTableBlockId == 0 || IntegrityBlockId == 0)
        {
            throw new KpkgFormatException("Required block ids in the header must be non-zero.");
        }
    }
}
