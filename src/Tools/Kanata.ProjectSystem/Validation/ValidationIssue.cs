namespace Kanata.ProjectSystem.Validation;

/// <summary>
/// Represents a single project validation issue.
/// </summary>
public sealed class ValidationIssue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationIssue"/> class.
    /// </summary>
    /// <param name="severity">The validation issue severity.</param>
    /// <param name="code">The stable validation issue code.</param>
    /// <param name="message">The human-readable validation message.</param>
    /// <param name="location">The optional project field or file path related to the issue.</param>
    public ValidationIssue(ValidationSeverity severity, string code, string message, string? location = null)
    {
        Severity = severity;
        Code = code;
        Message = message;
        Location = location;
    }

    /// <summary>
    /// Gets the validation issue severity.
    /// </summary>
    public ValidationSeverity Severity { get; }

    /// <summary>
    /// Gets the stable validation issue code.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Gets the human-readable validation message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the optional project field or file path related to the issue.
    /// </summary>
    public string? Location { get; }
}
