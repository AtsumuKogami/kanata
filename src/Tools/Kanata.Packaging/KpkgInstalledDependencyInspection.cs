namespace Kanata.Packaging;

/// <summary>
/// Represents an installed package dependency inspection.
/// </summary>
public sealed class KpkgInstalledDependencyInspection
{
    /// <summary>
    /// Gets the dependency id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets a value indicating whether the dependency is present in the installed registry.
    /// </summary>
    public bool IsInstalled { get; init; }
}
