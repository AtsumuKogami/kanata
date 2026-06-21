using System.Reflection;

namespace Kanata.Build.Infrastructure;

/// <summary>
/// Provides the version of the currently running Kanata toolchain.
/// </summary>
public static class KanataVersionProvider
{
    /// <summary>
    /// Gets the current Kanata toolchain version.
    /// </summary>
    public static string CurrentVersion { get; } = ResolveVersion();

    private static string ResolveVersion()
    {
        var assembly = typeof(KanataVersionProvider).Assembly;
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (!string.IsNullOrWhiteSpace(informationalVersion))
        {
            return informationalVersion;
        }

        return assembly.GetName().Version?.ToString() ?? "0.0.0";
    }
}
