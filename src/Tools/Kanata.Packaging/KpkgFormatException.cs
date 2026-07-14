namespace Kanata.Packaging;

/// <summary>
/// Represents a structural or semantic error in a Kanata package file.
/// </summary>
public sealed class KpkgFormatException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KpkgFormatException"/> class.
    /// </summary>
    /// <param name="message">The package format error message.</param>
    public KpkgFormatException(string message)
        : base(message)
    {
    }
}
