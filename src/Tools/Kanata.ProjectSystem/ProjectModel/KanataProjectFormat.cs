namespace Kanata.ProjectSystem.ProjectModel;

/// <summary>
/// Contains constants for the Kanata project file format.
/// </summary>
public static class KanataProjectFormat
{
    /// <summary>
    /// Gets the expected format identifier for Kanata project files.
    /// </summary>
    public const string Format = "kanata.project";

    /// <summary>
    /// Gets the latest project schema version supported by this library.
    /// </summary>
    public const int CurrentSchemaVersion = 1;
}
