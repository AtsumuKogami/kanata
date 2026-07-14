namespace Kanata.Packaging;

/// <summary>
/// Options used when writing a Kanata package from a staging directory.
/// </summary>
public sealed class KpkgWriterOptions
{
    /// <summary>
    /// Gets or sets the staging directory that contains package.kmanifest, descriptors, artifacts and sources.
    /// </summary>
    public string SourceDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the output .kpkg file path.
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether an existing output file may be replaced.
    /// </summary>
    public bool Overwrite { get; set; }
}
