namespace Kanata.ProjectSystem.Validation;

/// <summary>
/// Describes the severity of a project validation issue.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Indicates a non-blocking validation issue.
    /// </summary>
    Warning,

    /// <summary>
    /// Indicates a blocking validation issue.
    /// </summary>
    Error
}
