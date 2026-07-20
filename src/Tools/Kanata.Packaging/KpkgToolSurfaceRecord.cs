namespace Kanata.Packaging;

/// <summary>
/// Represents one optional user-facing surface exposed by an installed tool package.
/// </summary>
public sealed class KpkgToolSurfaceRecord
{
    /// <summary>
    /// Gets the surface id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the surface kind.
    /// </summary>
    public string Kind { get; init; } = string.Empty;

    /// <summary>
    /// Gets the user-facing surface title.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the surface description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether this surface is optional.
    /// </summary>
    public bool Optional { get; init; } = true;

    /// <summary>
    /// Gets the entry point kind.
    /// </summary>
    public string EntryPointKind { get; init; } = string.Empty;

    /// <summary>
    /// Gets the entry point package path.
    /// </summary>
    public string EntryPointPackagePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the resolved local entry point path.
    /// </summary>
    public string EntryPointLocalPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the local entry point exists.
    /// </summary>
    public bool EntryPointExists { get; init; }

    /// <summary>
    /// Gets the platforms declared for this surface.
    /// </summary>
    public IReadOnlyList<string> Platforms { get; init; } = [];
}
