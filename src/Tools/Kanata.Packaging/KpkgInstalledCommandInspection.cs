namespace Kanata.Packaging;

/// <summary>
/// Represents a tool command entry point inspection.
/// </summary>
public sealed class KpkgInstalledCommandInspection
{
    /// <summary>
    /// Gets the command name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the command entry point kind.
    /// </summary>
    public string EntryPointKind { get; init; } = string.Empty;

    /// <summary>
    /// Gets the command entry point package path.
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
}
