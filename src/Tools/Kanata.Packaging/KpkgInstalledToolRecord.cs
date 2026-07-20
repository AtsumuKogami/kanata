namespace Kanata.Packaging;

/// <summary>
/// Represents one installed tool package discovered in the local package store.
/// </summary>
public sealed class KpkgInstalledToolRecord
{
    /// <summary>
    /// Gets the tool installable id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the tool version.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets the package id that installed this tool.
    /// </summary>
    public string PackageId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the package version that installed this tool.
    /// </summary>
    public string PackageVersion { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installed package hash.
    /// </summary>
    public string PackageSha256 { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installed package root path.
    /// </summary>
    public string InstalledPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the resolved descriptor path.
    /// </summary>
    public string DescriptorPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the capabilities provided by this tool.
    /// </summary>
    public IReadOnlyList<string> Provides { get; init; } = [];

    /// <summary>
    /// Gets the commands exposed by this tool.
    /// </summary>
    public IReadOnlyList<KpkgToolCommandRecord> Commands { get; init; } = [];

    /// <summary>
    /// Gets optional user-facing surfaces exposed by this tool.
    /// </summary>
    public IReadOnlyList<KpkgToolSurfaceRecord> Surfaces { get; init; } = [];

    /// <summary>
    /// Gets problems found while reading this tool descriptor.
    /// </summary>
    public IReadOnlyList<string> Problems { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the required tool command surface is available.
    /// </summary>
    public bool IsUsable => Problems.Count == 0
        && Commands.Count > 0
        && Commands.Where(command => command.Required).All(command => command.EntryPointExists)
        && Surfaces.Where(surface => !surface.Optional).All(surface => surface.EntryPointExists);
}
