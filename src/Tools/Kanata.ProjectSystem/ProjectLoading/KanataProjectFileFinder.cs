namespace Kanata.ProjectSystem.ProjectLoading;

/// <summary>
/// Locates Kanata project files in directories or by explicit file path.
/// </summary>
public sealed class KanataProjectFileFinder
{
    /// <summary>
    /// Finds a Kanata project file from an optional explicit path.
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
            return FindProjectFileInDirectoryOnly(resolvedPath);
        }

        throw new FileNotFoundException($"Project path was not found: {resolvedPath}", resolvedPath);
    }

    /// <summary>
    /// Finds a Kanata project file in the current directory only.
    /// </summary>
    /// <returns>Absolute path to the discovered <c>.kanata</c> file.</returns>
    public string FindProjectFileInCurrentDirectory()
    {
        return FindProjectFileInDirectoryOnly(Directory.GetCurrentDirectory());
    }

    private static string FindProjectFileInDirectoryOnly(string directory)
    {
        var files = Directory.GetFiles(directory, "*.kanata", SearchOption.TopDirectoryOnly);

        if (files.Length == 1)
        {
            return files[0];
        }

        if (files.Length > 1)
        {
            throw new InvalidOperationException(
                "Multiple Kanata project files were found in the current directory. " +
                "Keep exactly one .kanata file in the project root.");
        }

        throw new FileNotFoundException(
            "No Kanata project file was found in the current directory. " +
            "Run this command from the project root directory that contains a .kanata file.");
    }
}
