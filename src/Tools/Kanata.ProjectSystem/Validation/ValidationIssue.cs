namespace Kanata.ProjectSystem.Validation;

/// <summary>
/// Represents a single Kanata project validation issue.
/// </summary>
/// <param name="Severity">Issue severity.</param>
/// <param name="Code">Stable diagnostic code.</param>
/// <param name="Message">Human-readable diagnostic message.</param>
/// <param name="Path">Optional project field or file path related to the issue.</param>
public sealed record ValidationIssue(
    ValidationSeverity Severity,
    string Code,
    string Message,
    string? Path = null);
