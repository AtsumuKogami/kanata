namespace Kanata.Packaging;

/// <summary>
/// Defines constants used by the Kanata package container format.
/// </summary>
public static class KpkgConstants
{
    /// <summary>
    /// The Kanata package file magic bytes.
    /// </summary>
    public static readonly byte[] Magic = [0x4B, 0x50, 0x4B, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    /// <summary>
    /// The Kanata package footer magic bytes.
    /// </summary>
    public static readonly byte[] FooterMagic = [0x4B, 0x50, 0x4B, 0x47, 0x45, 0x4F, 0x46, 0x0A];

    /// <summary>
    /// The V1 header length in bytes.
    /// </summary>
    public const uint HeaderLength = 128;

    /// <summary>
    /// The V1 footer length in bytes.
    /// </summary>
    public const uint FooterLength = 96;

    /// <summary>
    /// The V1 block table entry length in bytes.
    /// </summary>
    public const uint BlockTableEntryLength = 96;

    /// <summary>
    /// The supported major format version.
    /// </summary>
    public const ushort FormatMajor = 1;

    /// <summary>
    /// The supported minor format version.
    /// </summary>
    public const ushort FormatMinor = 0;
}
