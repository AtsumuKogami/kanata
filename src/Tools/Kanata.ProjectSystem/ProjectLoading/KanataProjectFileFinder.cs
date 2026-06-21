namespace Kanata.ProjectSystem.ProjectLoading;

/// <summary>
/// Locates Kanata project files in directories or by explicit file path.
/// </summary>
public sealed class KanataProjectFileFinder
{
    /// <summary>
    /// Finds a Kanata project file from an optional path argument.
    /// </summary>
    /// <param name="path">Project file path, project directory, or <see langword="null"/> for the current directory.</param>
    /// <returns>Absolute path to the discovered <c>.kanata</c> file.</returns>
    public string FindProjectFile(string? path = null)
    {
        var resolvedPath = Path.GetFullPath(path ?? Directory.GetCurrentDirectory());

        if (File.Exists(resolvedPath))
        {
            if (!string.Equals(Path.GetExtension(resolvedPath), ".kanata", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"File is not a Kanata project: {resolvedPath}");
            }

            return resolvedPath;
        }

        if (Directory.Exists(resolvedPath))
        {
            return FindProjectFileFromDirectory(resolvedPath);
        }

        throw new FileNotFoundException($"Project path was not found: {resolvedPath}", resolvedPath);
    }

    private static string FindProjectFileFromDirectory(string directory)
    {
        var current = directory;

        while (!string.IsNullOrWhiteSpace(current))
        {
            var files = Directory.GetFiles(current, "*.kanata", SearchOption.TopDirectoryOnly);

            if (files.Length == 1)
            {
                return files[0];
            }

            if (files.Length > 1)
            {
                throw new InvalidOperationException($"Directory contains multiple Kanata project files: {current}");
            }

            current = Directory.GetParent(current)?.FullName;
        }

        throw new FileNotFoundException($"No .kanata project file was found in '{directory}' or its parent directories.");
    }
}
