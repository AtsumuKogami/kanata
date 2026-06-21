namespace Kanata.ProjectSystem.Validation;

/// <summary>
/// Collects validation issues for a Kanata project.
/// </summary>
public sealed class ValidationResult
{
    private readonly List<ValidationIssue> _issues = [];

    /// <summary>
    /// Gets all collected validation issues.
    /// </summary>
    public IReadOnlyList<ValidationIssue> Issues => _issues;

    /// <summary>
    /// Gets a value indicating whether the result contains blocking errors.
    /// </summary>
    public bool HasErrors => _issues.Any(issue => issue.Severity == ValidationSeverity.Error);

    /// <summary>
    /// Adds a validation issue.
    /// </summary>
    /// <param name="issue">Issue to add.</param>
    public void Add(ValidationIssue issue)
    {
        _issues.Add(issue);
    }

    /// <summary>
    /// Adds a blocking validation error.
    /// </summary>
    /// <param name="code">Stable diagnostic code.</param>
    /// <param name="message">Human-readable diagnostic message.</param>
    /// <param name="path">Optional project field or file path.</param>
    public void AddError(string code, string message, string? path = null)
    {
        Add(new ValidationIssue(ValidationSeverity.Error, code, message, path));
    }

    /// <summary>
    /// Adds a validation warning.
    /// </summary>
    /// <param name="code">Stable diagnostic code.</param>
    /// <param name="message">Human-readable diagnostic message.</param>
    /// <param name="path">Optional project field or file path.</param>
    public void AddWarning(string code, string message, string? path = null)
    {
        Add(new ValidationIssue(ValidationSeverity.Warning, code, message, path));
    }

    /// <summary>
    /// Adds an informational validation message.
    /// </summary>
    /// <param name="code">Stable diagnostic code.</param>
    /// <param name="message">Human-readable diagnostic message.</param>
    /// <param name="path">Optional project field or file path.</param>
    public void AddInfo(string code, string message, string? path = null)
    {
        Add(new ValidationIssue(ValidationSeverity.Info, code, message, path));
    }
}
