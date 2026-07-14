namespace Kanata.Packaging;

/// <summary>
/// Defines known block types in the Kanata package container.
/// </summary>
public enum KpkgBlockType : uint
{
    /// <summary>
    /// The package manifest block.
    /// </summary>
    PackageManifest = 1,

    /// <summary>
    /// A typed installable descriptor block.
    /// </summary>
    InstallableDescriptor = 2,

    /// <summary>
    /// The payload file table block.
    /// </summary>
    FileTable = 3,

    /// <summary>
    /// A binary payload block.
    /// </summary>
    Payload = 4,

    /// <summary>
    /// The package integrity metadata block.
    /// </summary>
    Integrity = 5,

    /// <summary>
    /// Optional metadata block.
    /// </summary>
    Metadata = 6
}
