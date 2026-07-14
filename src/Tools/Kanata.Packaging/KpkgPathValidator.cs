namespace Kanata.Packaging;

/// <summary>
/// Validates canonical paths stored in Kanata package file tables.
/// </summary>
public static class KpkgPathValidator
{
    private static readonly HashSet<string> ReservedWindowsNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
    };

    /// <summary>
    /// Throws when the supplied package path is not a safe canonical V1 path.
    /// </summary>
    /// <param name="path">The package path to validate.</param>
    public static void Validate(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new KpkgFormatException("Package path must not be empty.");
        }

        if (path.Length > 512)
        {
            throw new KpkgFormatException($"Package path is too long: {path}.");
        }

        if (path.Contains("\0", StringComparison.Ordinal))
        {
            throw new KpkgFormatException($"Package path contains a NUL byte: {path}.");
        }

        if (path.Contains("\\", StringComparison.Ordinal))
        {
            throw new KpkgFormatException($"Package path must use '/' separators only: {path}.");
        }

        if (path.StartsWith("/", StringComparison.Ordinal))
        {
            throw new KpkgFormatException($"Package path must be relative: {path}.");
        }

        if (path.Length >= 2 && char.IsLetter(path[0]) && path[1] == ':')
        {
            throw new KpkgFormatException($"Package path must not contain a drive letter: {path}.");
        }

        if (path.StartsWith("//", StringComparison.Ordinal))
        {
            throw new KpkgFormatException($"Package path must not be a UNC path: {path}.");
        }

        var segments = path.Split('/');
        foreach (var segment in segments)
        {
            if (segment.Length == 0)
            {
                throw new KpkgFormatException($"Package path contains an empty segment: {path}.");
            }

            if (segment.Length > 128)
            {
                throw new KpkgFormatException($"Package path segment is too long: {path}.");
            }

            if (segment is "." or "..")
            {
                throw new KpkgFormatException($"Package path contains a forbidden segment: {path}.");
            }

            if (segment.EndsWith(" ", StringComparison.Ordinal) || segment.EndsWith(".", StringComparison.Ordinal))
            {
                throw new KpkgFormatException($"Package path segment must not end with space or dot: {path}.");
            }

            var nameWithoutExtension = segment.Split('.')[0];
            if (ReservedWindowsNames.Contains(nameWithoutExtension))
            {
                throw new KpkgFormatException($"Package path uses a reserved Windows device name: {path}.");
            }
        }
    }
}
