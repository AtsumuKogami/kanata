namespace Kanata.Packaging;

/// <summary>
/// Describes a package written by <see cref="KpkgWriter"/>.
/// </summary>
/// <param name="OutputPath">The output package path.</param>
/// <param name="PackageId">The package id.</param>
/// <param name="Version">The package version.</param>
/// <param name="InstallableCount">The number of installables declared by the package manifest.</param>
/// <param name="PayloadFileCount">The number of payload files written into the package.</param>
/// <param name="BlockCount">The number of package blocks.</param>
/// <param name="PackageLength">The final package length in bytes.</param>
public sealed record KpkgWriteResult(
    string OutputPath,
    string PackageId,
    string Version,
    int InstallableCount,
    int PayloadFileCount,
    int BlockCount,
    long PackageLength);
