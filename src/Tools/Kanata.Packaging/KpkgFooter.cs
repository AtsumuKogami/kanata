namespace Kanata.Packaging;

/// <summary>
/// Represents the fixed V1 Kanata package footer.
/// </summary>
public sealed record KpkgFooter(
    uint FooterLength,
    ushort FormatMajor,
    ushort FormatMinor,
    ulong PackageLength,
    ulong FooterOffset,
    KpkgHashAlgorithm ContentHashAlgorithm,
    byte[] ContentSha256)
{
    /// <summary>
    /// Reads and validates the package footer from the current stream position.
    /// </summary>
    /// <param name="reader">The binary reader.</param>
    /// <param name="header">The already parsed package header.</param>
    /// <returns>The parsed package footer.</returns>
    public static KpkgFooter Read(BinaryReader reader, KpkgHeader header)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(header);

        var magic = reader.ReadBytes(KpkgConstants.FooterMagic.Length);
        if (!magic.SequenceEqual(KpkgConstants.FooterMagic))
        {
            throw new KpkgFormatException("Invalid KPKG footer magic.");
        }

        var footerLength = reader.ReadUInt32();
        var formatMajor = reader.ReadUInt16();
        var formatMinor = reader.ReadUInt16();
        var packageLength = reader.ReadUInt64();
        var footerOffset = reader.ReadUInt64();
        var hashAlgorithm = (KpkgHashAlgorithm)reader.ReadUInt16();
        var reserved16 = reader.ReadUInt16();
        var reserved32 = reader.ReadUInt32();
        var contentSha256 = reader.ReadBytes(32);
        var reservedBytes = reader.ReadBytes(24);

        if (reserved16 != 0 || reserved32 != 0 || reservedBytes.Any(value => value != 0))
        {
            throw new KpkgFormatException("Footer reserved fields must be zero.");
        }

        var footer = new KpkgFooter(
            footerLength,
            formatMajor,
            formatMinor,
            packageLength,
            footerOffset,
            hashAlgorithm,
            contentSha256);

        footer.Validate(header);
        return footer;
    }

    /// <summary>
    /// Validates the footer against the package header.
    /// </summary>
    /// <param name="header">The parsed package header.</param>
    public void Validate(KpkgHeader header)
    {
        ArgumentNullException.ThrowIfNull(header);

        if (FooterLength != KpkgConstants.FooterLength)
        {
            throw new KpkgFormatException($"Unsupported KPKG footer length: {FooterLength}.");
        }

        if (FormatMajor != header.FormatMajor || FormatMinor != header.FormatMinor)
        {
            throw new KpkgFormatException("Footer format version does not match the header.");
        }

        if (PackageLength != header.PackageLength)
        {
            throw new KpkgFormatException("Footer package length does not match the header.");
        }

        if (FooterOffset != header.FooterOffset)
        {
            throw new KpkgFormatException("Footer offset does not match the header.");
        }

        if (ContentHashAlgorithm != KpkgHashAlgorithm.Sha256)
        {
            throw new KpkgFormatException("Unsupported package content hash algorithm.");
        }

        if (ContentSha256.Length != 32)
        {
            throw new KpkgFormatException("Footer SHA-256 hash must be 32 bytes.");
        }
    }
}
