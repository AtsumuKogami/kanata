using System.Text.RegularExpressions;
using Kanata.ProjectSystem.ProjectModel;

namespace Kanata.ProjectSystem.Validation;

/// <summary>
/// Validates Kanata project models and referenced workspace paths.
/// </summary>
public sealed partial class KanataProjectValidator
{
    /// <summary>
    /// Validates a Kanata project model.
    /// </summary>
    /// <param name="project">The project model to validate.</param>
    /// <param name="projectFilePath">The path to the source project file.</param>
    /// <returns>The validation result.</returns>
    public ValidationResult Validate(KanataProject project, string projectFilePath)
    {
        ArgumentNullException.ThrowIfNull(project);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFilePath);

        var result = new ValidationResult();
        var projectDirectory = Path.GetDirectoryName(Path.GetFullPath(projectFilePath)) ?? Directory.GetCurrentDirectory();

        ValidateHeader(project, result);
        ValidatePaths(project, projectDirectory, result);
        ValidateSourceProjects(project, projectDirectory, result);
        ValidateFeatures(project, result);
        ValidateTargets(project, projectDirectory, result);
        ValidateStart(project, projectDirectory, result);

        return result;
    }

    private static void ValidateHeader(KanataProject project, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(project.Format))
        {
            result.AddError("KANATA001", "Project format is required.", "format");
        }
        else if (!string.Equals(project.Format, KanataProjectFormat.Format, StringComparison.Ordinal))
        {
            result.AddError("KANATA002", $"Project format must be '{KanataProjectFormat.Format}'.", "format");
        }

        if (project.SchemaVersion <= 0)
        {
            result.AddError("KANATA003", "Project schema version is required.", "schemaVersion");
        }
        else if (project.SchemaVersion > KanataProjectFormat.CurrentSchemaVersion)
        {
            result.AddError("KANATA004", $"Project schema version {project.SchemaVersion} is not supported by this Kanata.ProjectSystem version.", "schemaVersion");
        }
        else if (project.SchemaVersion < KanataProjectFormat.CurrentSchemaVersion)
        {
            result.AddWarning("KANATA005", $"Project schema version {project.SchemaVersion} is older than the current version {KanataProjectFormat.CurrentSchemaVersion}.", "schemaVersion");
        }

        if (string.IsNullOrWhiteSpace(project.Id))
        {
            result.AddError("KANATA006", "Project id is required.", "id");
        }
        else if (!ProjectIdRegex().IsMatch(project.Id))
        {
            result.AddError("KANATA007", "Project id must contain only lowercase letters, digits, dots, and hyphens, and must start with a letter or digit.", "id");
        }

        if (string.IsNullOrWhiteSpace(project.Name))
        {
            result.AddError("KANATA008", "Project name is required.", "name");
        }

        if (string.IsNullOrWhiteSpace(project.KanataVersion))
        {
            result.AddError("KANATA009", "Kanata SDK version is required.", "kanataVersion");
        }
    }

    private static void ValidatePaths(KanataProject project, string projectDirectory, ValidationResult result)
    {
        CheckDirectory(project.Paths.Content, "paths.content", projectDirectory, result, required: true);
        CheckDirectory(project.Paths.Source, "paths.source", projectDirectory, result, required: true);
        CheckDirectory(project.Paths.Generated, "paths.generated", projectDirectory, result, required: false);
        CheckDirectory(project.Paths.Settings, "paths.settings", projectDirectory, result, required: true);
    }

    private static void ValidateSourceProjects(KanataProject project, string projectDirectory, ValidationResult result)
    {
        CheckFile(project.Source.Shared, "source.shared", projectDirectory, result, required: true, expectedExtension: ".csproj");
        CheckFile(project.Source.Logic, "source.logic", projectDirectory, result, required: true, expectedExtension: ".csproj");
        CheckFile(project.Source.View, "source.view", projectDirectory, result, required: true, expectedExtension: ".csproj");
    }

    private static void ValidateFeatures(KanataProject project, ValidationResult result)
    {
        if (project.Features.Count == 0)
        {
            result.AddWarning("KANATA020", "Project has no requested features.", "features");
            return;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < project.Features.Count; index++)
        {
            var feature = project.Features[index];
            var location = $"features[{index}]";

            if (string.IsNullOrWhiteSpace(feature))
            {
                result.AddError("KANATA021", "Feature id cannot be empty.", location);
                continue;
            }

            if (!seen.Add(feature))
            {
                result.AddWarning("KANATA022", $"Feature '{feature}' is listed more than once.", location);
            }
        }
    }

    private static void ValidateTargets(KanataProject project, string projectDirectory, ValidationResult result)
    {
        if (project.Targets.Count == 0)
        {
            result.AddError("KANATA030", "Project must define at least one target.", "targets");
            return;
        }

        foreach (var (targetName, target) in project.Targets)
        {
            var targetLocation = $"targets.{targetName}";

            if (string.IsNullOrWhiteSpace(targetName))
            {
                result.AddError("KANATA031", "Target name cannot be empty.", "targets");
            }

            CheckRequiredText(target.Platform, $"{targetLocation}.platform", "Target platform is required.", result, "KANATA032");
            CheckRequiredText(target.Backend, $"{targetLocation}.backend", "Target backend is required.", result, "KANATA033");
            CheckRequiredText(target.Session, $"{targetLocation}.session", "Target session is required.", result, "KANATA034");
            CheckFile(target.HostProject, $"{targetLocation}.hostProject", projectDirectory, result, required: true, expectedExtension: ".csproj");
        }
    }

    private static void ValidateStart(KanataProject project, string projectDirectory, ValidationResult result)
    {
        CheckFile(project.Start.Scene, "start.scene", projectDirectory, result, required: true, expectedExtension: ".kscene");
    }

    private static void CheckDirectory(string path, string location, string projectDirectory, ValidationResult result, bool required)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            if (required)
            {
                result.AddError("KANATA040", "Directory path is required.", location);
            }

            return;
        }

        var fullPath = Resolve(projectDirectory, path);
        if (!Directory.Exists(fullPath))
        {
            var message = required
                ? $"Required directory '{path}' does not exist."
                : $"Optional directory '{path}' does not exist and will be created when needed.";

            if (required)
            {
                result.AddError("KANATA041", message, location);
            }
            else
            {
                result.AddWarning("KANATA042", message, location);
            }
        }
    }

    private static void CheckFile(string path, string location, string projectDirectory, ValidationResult result, bool required, string expectedExtension)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            if (required)
            {
                result.AddError("KANATA050", "File path is required.", location);
            }

            return;
        }

        if (!string.Equals(Path.GetExtension(path), expectedExtension, StringComparison.OrdinalIgnoreCase))
        {
            result.AddError("KANATA051", $"File path must point to a '{expectedExtension}' file.", location);
        }

        var fullPath = Resolve(projectDirectory, path);
        if (!File.Exists(fullPath))
        {
            result.AddError("KANATA052", $"Required file '{path}' does not exist.", location);
        }
    }

    private static void CheckRequiredText(string value, string location, string message, ValidationResult result, string code)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result.AddError(code, message, location);
        }
    }

    private static string Resolve(string projectDirectory, string path)
    {
        return Path.GetFullPath(Path.IsPathRooted(path) ? path : Path.Combine(projectDirectory, path));
    }

    [GeneratedRegex("^[a-z0-9][a-z0-9.-]*$", RegexOptions.CultureInvariant)]
    private static partial Regex ProjectIdRegex();
}
