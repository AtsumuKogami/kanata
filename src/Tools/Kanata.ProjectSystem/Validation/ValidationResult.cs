namespace Kanata.ProjectSystem.Validation;

/// <summary>
/// Represents the result of project validation.
/// </summary>
public sealed class ValidationResult
{
    private readonly List<ValidationIssue> _issues = new();

    /// <summary>
    /// Gets all validation issues found during validation.
    /// </summary>
    public IReadOnlyList<ValidationIssue> Issues => _issues;

    /// <summary>
    /// Gets a value indicating whether validation has blocking errors.
    /// </summary>
    public bool HasErrors => _issues.Any(issue => issue.Severity == ValidationSeverity.Error);

    /// <summary>
    /// Adds an issue to the validation result.
    /// </summary>
    /// <param name="issue">The issue to add.</param>
    public void Add(ValidationIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);
        _issues.Add(issue);
    }

    /// <summary>
    /// Adds a blocking validation error.
    /// </summary>
    /// <param name="code">The stable validation issue code.</param>
    /// <param name="message">The human-readable validation message.</param>
    /// <param name="location">The optional project field or file path related to the issue.</param>
    public void AddError(string code, string message, string? location = null)
    {
        Add(new ValidationIssue(ValidationSeverity.Error, code, message, location));
    }

    /// <summary>
    /// Adds a non-blocking validation warning.
    /// </summary>
    /// <param name="code">The stable validation issue code.</param>
    /// <param name="message">The human-readable validation message.</param>
    /// <param name="location">The optional project field or file path related to the issue.</param>
    public void AddWarning(string code, string message, string? location = null)
    {
        Add(new ValidationIssue(ValidationSeverity.Warning, code, message, location));
    }
}
