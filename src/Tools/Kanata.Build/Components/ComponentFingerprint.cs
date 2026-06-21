using System.Security.Cryptography;
using System.Text;

namespace Kanata.Build.Components;

internal static class ComponentFingerprint
{
    public static string Compute(ComponentSource source, string configuration, string repositoryRoot)
    {
        var files = new List<string>
        {
            source.ManifestPath,
            source.ProjectPath,
        };

        var directoryBuildProps = Path.Combine(repositoryRoot, "Directory.Build.props");
        if (File.Exists(directoryBuildProps))
        {
            files.Add(directoryBuildProps);
        }

        files.AddRange(Directory.EnumerateFiles(source.ComponentRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => !IsGeneratedBuildOutput(path)));

        files.Sort(StringComparer.OrdinalIgnoreCase);

        using var sha = SHA256.Create();
        AppendText(sha, source.Id);
        AppendText(sha, source.Version);
        AppendText(sha, configuration);
        AppendText(sha, source.TargetFramework);

        foreach (var file in files)
        {
            AppendText(sha, Path.GetRelativePath(repositoryRoot, file).Replace('\\', '/'));
            var bytes = File.ReadAllBytes(file);
            sha.TransformBlock(bytes, 0, bytes.Length, null, 0);
        }

        sha.TransformFinalBlock([], 0, 0);
        return Convert.ToHexString(sha.Hash!).ToLowerInvariant();
    }

    private static bool IsGeneratedBuildOutput(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase);
    }

    private static void AppendText(HashAlgorithm algorithm, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value + "\n");
        algorithm.TransformBlock(bytes, 0, bytes.Length, null, 0);
    }
}
