namespace Kanata.ProjectSystem.ProjectLoading;

/// <summary>
/// Locates Kanata project files in files or directories.
/// </summary>
public sealed class KanataProjectFileFinder
{
    /// <summary>
    /// Finds a Kanata project file from a file path, directory path, or current directory.
    /// </summary>
    /// <param name="path">The optional file or directory path to inspect.</param>
    /// <returns>The full path to the discovered project file.</returns>
    public string Find(string? path = null)
    {
        var effectivePath = string.IsNullOrWhiteSpace(path)
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(path);

        if (File.Exists(effectivePath))
        {
            if (!string.Equals(Path.GetExtension(effectivePath), ".kanata", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"File '{effectivePath}' is not a .kanata project file.");
            }

            return effectivePath;
        }

        if (!Directory.Exists(effectivePath))
        {
            throw new DirectoryNotFoundException($"Path '{effectivePath}' does not exist.");
        }

        var projectFiles = Directory.GetFiles(effectivePath, "*.kanata", SearchOption.TopDirectoryOnly);

        return projectFiles.Length switch
        {
            0 => throw new FileNotFoundException($"No .kanata project file was found in '{effectivePath}'."),
            1 => projectFiles[0],
            _ => throw new InvalidOperationException($"Multiple .kanata project files were found in '{effectivePath}'. Pass the project file path explicitly.")
        };
    }
}
