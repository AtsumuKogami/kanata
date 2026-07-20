namespace Kanata.Packaging;

/// <summary>
/// Configures local installed tool registry reads.
/// </summary>
public sealed class KpkgToolRegistryOptions
{
    /// <summary>
    /// Gets the optional package store root override.
    /// </summary>
    public string? StoreRoot { get; init; }

    /// <summary>
    /// Gets the optional target tool id.
    /// </summary>
    public string? TargetId { get; init; }
}
