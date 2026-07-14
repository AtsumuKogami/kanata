namespace Kanata.Packaging;

/// <summary>
/// Represents the result of a package verification run.
/// </summary>
public sealed class KpkgVerificationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KpkgVerificationResult"/> class.
    /// </summary>
    /// <param name="package">The parsed package, when parsing reached manifest loading.</param>
    /// <param name="errors">The collected verification errors.</param>
    public KpkgVerificationResult(KpkgPackage? package, IReadOnlyList<string> errors)
    {
        Package = package;
        Errors = errors;
    }

    /// <summary>
    /// Gets the parsed package, when available.
    /// </summary>
    public KpkgPackage? Package { get; }

    /// <summary>
    /// Gets collected verification errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Gets a value indicating whether the package is valid.
    /// </summary>
    public bool IsValid => Errors.Count == 0;
}
