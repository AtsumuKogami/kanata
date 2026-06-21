namespace Kanata.ProjectSystem.Validation;

/// <summary>
/// Defines severity levels for project validation issues.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// Indicates a non-blocking validation message.
    /// </summary>
    Info,

    /// <summary>
    /// Indicates a suspicious but non-blocking project configuration.
    /// </summary>
    Warning,

    /// <summary>
    /// Indicates a validation issue that blocks build operations.
    /// </summary>
    Error,
}
