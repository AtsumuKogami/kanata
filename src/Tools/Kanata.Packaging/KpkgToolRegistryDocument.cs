namespace Kanata.Packaging;

/// <summary>
/// Represents the computed local installed tool registry view.
/// </summary>
public sealed class KpkgToolRegistryDocument
{
    /// <summary>
    /// Gets the package store root path used to compute this registry view.
    /// </summary>
    public string StoreRoot { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installed tools discovered in the package store.
    /// </summary>
    public IReadOnlyList<KpkgInstalledToolRecord> Tools { get; init; } = [];

    /// <summary>
    /// Gets registry-level problems such as command conflicts.
    /// </summary>
    public IReadOnlyList<string> Problems { get; init; } = [];
}
