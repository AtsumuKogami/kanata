namespace Kanata.Packaging;

/// <summary>
/// Represents one inspected installable inside an installed package.
/// </summary>
public sealed class KpkgInstalledInstallableInspection
{
    /// <summary>
    /// Gets the installable id.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installable version.
    /// </summary>
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installable kind.
    /// </summary>
    public string Kind { get; init; } = string.Empty;

    /// <summary>
    /// Gets the descriptor file path.
    /// </summary>
    public string DescriptorPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets artifact inspections declared by the descriptor.
    /// </summary>
    public IReadOnlyList<KpkgInstalledPathInspection> Artifacts { get; init; } = [];

    /// <summary>
    /// Gets embedded source inspections declared by the descriptor.
    /// </summary>
    public IReadOnlyList<KpkgInstalledPathInspection> Sources { get; init; } = [];

    /// <summary>
    /// Gets command entry point inspections declared by tool descriptors.
    /// </summary>
    public IReadOnlyList<KpkgInstalledCommandInspection> Commands { get; init; } = [];

    /// <summary>
    /// Gets optional UI surface inspections declared by tool descriptors.
    /// </summary>
    public IReadOnlyList<KpkgInstalledSurfaceInspection> Surfaces { get; init; } = [];

    /// <summary>
    /// Gets dependency inspections declared by the descriptor.
    /// </summary>
    public IReadOnlyList<KpkgInstalledDependencyInspection> Dependencies { get; init; } = [];

    /// <summary>
    /// Gets the number of source references declared by the descriptor.
    /// </summary>
    public int SourceReferenceCount { get; init; }

    /// <summary>
    /// Gets installable-level problems found by the inspector.
    /// </summary>
    public IReadOnlyList<string> Problems { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the installable looks usable from installed artifacts.
    /// </summary>
    public bool IsUsable => Problems.Count == 0
        && Artifacts.Count > 0
        && Artifacts.All(artifact => artifact.Exists)
        && Dependencies.All(dependency => dependency.IsInstalled)
        && Commands.All(command => command.EntryPointExists)
        && Surfaces.Where(surface => !surface.Optional).All(surface => surface.EntryPointExists);
}
