namespace Kanata.ProjectSystem.LockModel;

/// <summary>
/// Defines constants for the Kanata lock file format.
/// </summary>
public static class KanataLockFileFormat
{
    /// <summary>
    /// Gets the expected lock file format identifier.
    /// </summary>
    public const string Format = "kanata.lock";

    /// <summary>
    /// Gets the supported lock file schema version.
    /// </summary>
    public const int SupportedSchemaVersion = 1;
}
