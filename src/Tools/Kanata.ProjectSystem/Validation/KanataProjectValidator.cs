using System.Text.RegularExpressions;
using Kanata.ProjectSystem.ProjectModel;

namespace Kanata.ProjectSystem.Validation;

/// <summary>
/// Validates Kanata project files before generation or build operations.
/// </summary>
public sealed class KanataProjectValidator
{
    /// <summary>
    /// Validates a loaded Kanata project model.
    /// </summary>
    /// <param name="project">Project model to validate.</param>
    /// <param name="projectFilePath">Path to the source project file.</param>
    /// <returns>Collected validation issues.</returns>
    public ValidationResult Validate(KanataProject? project, string projectFilePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFilePath);

        var result = new ValidationResult();

        if (project is null)
        {
            result.AddError("KANATA001", "Project file could not be loaded.", projectFilePath);
            return result;
        }

        var projectRoot = GetProjectRoot(projectFilePath);

        ValidateIdentity(project, result);
        ValidatePaths(project, projectRoot, result);
        ValidateSourceProjects(project, projectRoot, result);
        ValidateFeatures(project, result);
        ValidateTargets(project, projectRoot, result);
        ValidateStart(project, projectRoot, result);

        return result;
    }

    private static void ValidateIdentity(KanataProject project, ValidationResult result)
    {
        if (!string.Equals(project.Format, KanataProjectFormat.Project, StringComparison.Ordinal))
        {
            result.AddError("KANATA010", $"Project format must be '{KanataProjectFormat.Project}'.", "format");
        }

        if (project.SchemaVersion != KanataProjectFormat.SupportedSchemaVersion)
        {
            result.AddError(
                "KANATA011",
                $"Project schema version '{project.SchemaVersion}' is not supported. Supported version: {KanataProjectFormat.SupportedSchemaVersion}.",
                "schemaVersion");
        }

        if (string.IsNullOrWhiteSpace(project.Id))
        {
            result.AddError("KANATA012", "Project id is required.", "id");
        }
        else if (!Regex.IsMatch(project.Id, "^[a-z0-9][a-z0-9.-]*[a-z0-9]$|^[a-z0-9]$", RegexOptions.CultureInvariant))
        {
            result.AddWarning("KANATA013", "Project id should use lowercase letters, digits, dots or hyphens.", "id");
        }

        if (string.IsNullOrWhiteSpace(project.Name))
        {
            result.AddError("KANATA014", "Project name is required.", "name");
        }

        if (string.IsNullOrWhiteSpace(project.KanataVersion))
        {
            result.AddError("KANATA015", "Kanata SDK version is required.", "kanataVersion");
        }
    }

    private static void ValidatePaths(KanataProject project, string projectRoot, ValidationResult result)
    {
        if (project.Paths is null)
        {
            result.AddError("KANATA020", "Paths section is required.", "paths");
            return;
        }

        ValidateRequiredDirectory(projectRoot, project.Paths.Content, "paths.content", result);
        ValidateRequiredDirectory(projectRoot, project.Paths.Source, "paths.source", result);
        ValidateRequiredDirectory(projectRoot, project.Paths.Generated, "paths.generated", result);
        ValidateRequiredDirectory(projectRoot, project.Paths.Settings, "paths.settings", result);
    }

    private static void ValidateSourceProjects(KanataProject project, string projectRoot, ValidationResult result)
    {
        if (project.Source is null)
        {
            result.AddError("KANATA030", "Source section is required.", "source");
            return;
        }

        ValidateRequiredFile(projectRoot, project.Source.Shared, "source.shared", result);
        ValidateRequiredFile(projectRoot, project.Source.Logic, "source.logic", result);
        ValidateRequiredFile(projectRoot, project.Source.View, "source.view", result);
    }

    private static void ValidateFeatures(KanataProject project, ValidationResult result)
    {
        if (project.Features.Count == 0)
        {
            result.AddWarning("KANATA040", "Project does not declare any features.", "features");
            return;
        }

        var duplicates = project.Features
            .Where(feature => !string.IsNullOrWhiteSpace(feature))
            .GroupBy(feature => feature, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        foreach (var duplicate in duplicates)
        {
            result.AddWarning("KANATA041", $"Feature '{duplicate}' is declared more than once.", "features");
        }
    }

    private static void ValidateTargets(KanataProject project, string projectRoot, ValidationResult result)
    {
        if (project.Targets.Count == 0)
        {
            result.AddError("KANATA050", "At least one build target is required.", "targets");
            return;
        }

        foreach (var (targetName, target) in project.Targets)
        {
            var targetPath = $"targets.{targetName}";

            if (string.IsNullOrWhiteSpace(target.Platform))
            {
                result.AddError("KANATA051", $"Target '{targetName}' must declare a platform.", $"{targetPath}.platform");
            }

            if (string.IsNullOrWhiteSpace(target.Backend))
            {
                result.AddError("KANATA052", $"Target '{targetName}' must declare a backend.", $"{targetPath}.backend");
            }

            if (string.IsNullOrWhiteSpace(target.Session))
            {
                result.AddError("KANATA053", $"Target '{targetName}' must declare a session mode.", $"{targetPath}.session");
            }

            ValidateRequiredFile(projectRoot, target.HostProject, $"{targetPath}.hostProject", result);
        }
    }

    private static void ValidateStart(KanataProject project, string projectRoot, ValidationResult result)
    {
        if (project.Start is null)
        {
            result.AddError("KANATA060", "Start section is required.", "start");
            return;
        }

        ValidateRequiredFile(projectRoot, project.Start.Scene, "start.scene", result);
    }

    private static void ValidateRequiredDirectory(string projectRoot, string? relativePath, string fieldPath, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            result.AddError("KANATA070", "Directory path is required.", fieldPath);
            return;
        }

        var fullPath = Path.GetFullPath(Path.Combine(projectRoot, relativePath));

        if (!Directory.Exists(fullPath))
        {
            result.AddError("KANATA071", $"Directory does not exist: {relativePath}", fieldPath);
        }
    }

    private static void ValidateRequiredFile(string projectRoot, string? relativePath, string fieldPath, ValidationResult result)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
        {
            result.AddError("KANATA080", "File path is required.", fieldPath);
            return;
        }

        var fullPath = Path.GetFullPath(Path.Combine(projectRoot, relativePath));

        if (!File.Exists(fullPath))
        {
            result.AddError("KANATA081", $"File does not exist: {relativePath}", fieldPath);
        }
    }

    private static string GetProjectRoot(string projectFilePath)
    {
        return Path.GetDirectoryName(Path.GetFullPath(projectFilePath)) ?? Directory.GetCurrentDirectory();
    }
}
