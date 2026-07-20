namespace Kanata.Packaging;

/// <summary>
/// Represents an optional tool UI surface entry point inspection.
/// </summary>
public sealed class KpkgInstalledSurfaceInspection
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
    /// Gets a value indicating whether the surface is optional for the tool.
    /// </summary>
    public bool Optional { get; init; } = true;

    /// <summary>
    /// Gets the surface entry point kind.
    /// </summary>
    public string EntryPointKind { get; init; } = string.Empty;

    /// <summary>
    /// Gets the surface entry point package path.
    /// </summary>
    public string EntryPointPackagePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the resolved entry point local filesystem path.
    /// </summary>
    public string EntryPointLocalPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the resolved entry point exists.
    /// </summary>
    public bool EntryPointExists { get; init; }

    /// <summary>
    /// Gets the platforms declared for this surface.
    /// </summary>
    public IReadOnlyList<string> Platforms { get; init; } = [];
}
