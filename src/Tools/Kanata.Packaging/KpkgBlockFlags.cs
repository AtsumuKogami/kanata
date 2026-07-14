namespace Kanata.Packaging;

/// <summary>
/// Defines flags attached to a package block table entry.
/// </summary>
[Flags]
public enum KpkgBlockFlags : uint
{
    /// <summary>
    /// The block has no special flags.
    /// </summary>
    None = 0,

    /// <summary>
    /// The block is critical. Unknown critical blocks must fail validation.
    /// </summary>
    Critical = 1
}
