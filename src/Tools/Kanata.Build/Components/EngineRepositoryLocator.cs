namespace Kanata.Build.Components;

internal static class EngineRepositoryLocator
{
    public static string FindRepositoryRoot()
    {
        var candidates = new[]
        {
            Environment.GetEnvironmentVariable("KANATA_REPOSITORY_ROOT"),
            Directory.GetCurrentDirectory(),
            AppContext.BaseDirectory,
        };

        foreach (var candidate in candidates)
        {
            if (string.IsNullOrWhiteSpace(candidate))
            {
                continue;
            }

            var root = TryFindFrom(candidate);
            if (root is not null)
            {
                return root;
            }
        }

        throw new DirectoryNotFoundException(
            "Unable to locate the Kanata source repository. " +
            "Expected a directory that contains src/Engine/Kanata.Core and src/Tools/Kanata.Build. " +
            $"Current directory: {Directory.GetCurrentDirectory()}");
    }

    private static string? TryFindFrom(string startDirectory)
    {
        var fullPath = Path.GetFullPath(startDirectory);
        var directory = File.Exists(fullPath)
            ? new FileInfo(fullPath).Directory
            : new DirectoryInfo(fullPath);

        while (directory is not null)
        {
            if (LooksLikeKanataRepository(directory.FullName))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static bool LooksLikeKanataRepository(string directory)
    {
        var hasSourceLayout = Directory.Exists(Path.Combine(directory, "src"))
            && File.Exists(Path.Combine(directory, "src", "Engine", "Kanata.Core", "Kanata.Core.csproj"))
            && File.Exists(Path.Combine(directory, "src", "Tools", "Kanata.Build", "Kanata.Build.csproj"));

        if (!hasSourceLayout)
        {
            return false;
        }

        var hasSolutionOrGitRoot = File.Exists(Path.Combine(directory, "Kanata.sln"))
            || File.Exists(Path.Combine(directory, "Kanata.slnx"))
            || Directory.Exists(Path.Combine(directory, ".git"));

        return hasSolutionOrGitRoot;
    }
}
