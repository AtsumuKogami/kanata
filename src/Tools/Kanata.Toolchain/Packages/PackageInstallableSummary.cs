namespace Kanata.Toolchain.Packages;

/// <summary>
/// Describes one installable declared by a package manifest.
/// </summary>
/// <param name="Id">The installable id.</param>
/// <param name="Version">The installable version.</param>
/// <param name="Kind">The installable kind.</param>
/// <param name="Description">The optional installable description.</param>
/// <param name="Provides">The capability ids provided by the installable.</param>
/// <param name="Dependencies">The component ids required by the installable.</param>
public sealed record PackageInstallableSummary(
    string Id,
    string Version,
    string Kind,
    string? Description,
    IReadOnlyList<string> Provides,
    IReadOnlyList<string> Dependencies);
