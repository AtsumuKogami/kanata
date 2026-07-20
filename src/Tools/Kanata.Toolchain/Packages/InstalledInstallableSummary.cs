namespace Kanata.Toolchain.Packages;

/// <summary>
/// Describes one installable in the local installed package registry.
/// </summary>
/// <param name="Id">The installable id.</param>
/// <param name="Version">The installable version.</param>
/// <param name="Kind">The installable kind.</param>
public sealed record InstalledInstallableSummary(string Id, string Version, string Kind);
