namespace Kanata.Packaging;

/// <summary>
/// Represents one CLI command exposed by an installed tool package.
/// </summary>
public sealed class KpkgToolCommandRecord
{
    /// <summary>
    /// Gets the command name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the command description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets command aliases.
    /// </summary>
    public IReadOnlyList<string> Aliases { get; init; } = [];

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
    /// Gets arguments prepended when the command is dispatched.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; init; } = [];

    /// <summary>
    /// Gets the command launch mode.
    /// </summary>
    public string LaunchMode { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the command is required for tool usability.
    /// </summary>
    public bool Required { get; init; } = true;
}
