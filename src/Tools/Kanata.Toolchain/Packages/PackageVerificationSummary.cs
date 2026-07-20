namespace Kanata.Toolchain.Packages;

/// <summary>
/// Describes the result of verifying one Kanata package.
/// </summary>
public sealed class PackageVerificationSummary
{
    /// <summary>
    /// Gets a value indicating whether the package is valid.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets collected verification errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; init; } = [];

    /// <summary>
    /// Gets the parsed package summary when metadata could be read.
    /// </summary>
    public PackageSummary? Package { get; init; }
}
