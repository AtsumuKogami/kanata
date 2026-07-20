namespace Kanata.Packaging;

/// <summary>
/// Represents the result of installing a Kanata package.
/// </summary>
public sealed record KpkgInstallResult(
    string PackageId,
    string Version,
    string PackageSha256,
    string InstalledPath,
    int InstallableCount,
    int FileCount);
