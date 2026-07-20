namespace Kanata.Packaging;

/// <summary>
/// Describes the local Kanata package store directory layout.
/// </summary>
public sealed class KpkgPackageStore
{
    private const string EnvironmentVariableName = "KANATA_PACKAGE_STORE";

    private KpkgPackageStore(string rootPath)
    {
        RootPath = rootPath;
    }

    /// <summary>
    /// Gets the root package store directory.
    /// </summary>
    public string RootPath { get; }

    /// <summary>
    /// Gets the installed packages directory.
    /// </summary>
    public string InstalledPath => Path.Combine(RootPath, "installed");

    /// <summary>
    /// Gets the package store registry directory.
    /// </summary>
    public string RegistryPath => Path.Combine(RootPath, "registry");

    /// <summary>
    /// Gets the package store temporary directory.
    /// </summary>
    public string TempPath => Path.Combine(RootPath, ".tmp");

    /// <summary>
    /// Gets the installed package registry file path.
    /// </summary>
    public string InstalledRegistryPath => Path.Combine(RegistryPath, "installed.kmanifest");

    /// <summary>
    /// Creates a package store model from an optional root path.
    /// </summary>
    /// <param name="rootPath">The optional store root path.</param>
    /// <returns>The package store model.</returns>
    public static KpkgPackageStore Create(string? rootPath = null)
    {
        var resolvedRoot = string.IsNullOrWhiteSpace(rootPath)
            ? GetDefaultRootPath()
            : Path.GetFullPath(rootPath);

        return new KpkgPackageStore(resolvedRoot);
    }

    /// <summary>
    /// Gets the default package store root path.
    /// </summary>
    /// <returns>The default store path.</returns>
    public static string GetDefaultRootPath()
    {
        var environmentOverride = Environment.GetEnvironmentVariable(EnvironmentVariableName);
        if (!string.IsNullOrWhiteSpace(environmentOverride))
        {
            return Path.GetFullPath(environmentOverride);
        }

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrWhiteSpace(userProfile))
        {
            throw new KpkgFormatException("Could not resolve the user profile directory for the default package store.");
        }

        return Path.Combine(userProfile, ".kanata", "packages");
    }

    /// <summary>
    /// Ensures all base store directories exist.
    /// </summary>
    public void EnsureCreated()
    {
        Directory.CreateDirectory(InstalledPath);
        Directory.CreateDirectory(RegistryPath);
        Directory.CreateDirectory(TempPath);
    }

    /// <summary>
    /// Gets the immutable install root for a package version and package hash.
    /// </summary>
    /// <param name="packageId">The package id.</param>
    /// <param name="version">The package version.</param>
    /// <param name="packageSha256">The package SHA-256 hash.</param>
    /// <returns>The package install root path.</returns>
    public string GetInstallRoot(string packageId, string version, string packageSha256)
    {
        return Path.Combine(
            InstalledPath,
            ToStoreSegment(packageId, "package id"),
            ToStoreSegment(version, "package version"),
            ToStoreSegment(packageSha256, "package hash"));
    }

    /// <summary>
    /// Converts a logical id into a safe package store path segment.
    /// </summary>
    /// <param name="value">The logical value.</param>
    /// <param name="description">The value description for error messages.</param>
    /// <returns>The safe store segment.</returns>
    public static string ToStoreSegment(string value, string description)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new KpkgFormatException($"Store {description} must not be empty.");
        }

        if (value.Length > 128)
        {
            throw new KpkgFormatException($"Store {description} is too long: {value}.");
        }

        foreach (var character in value)
        {
            var isAllowed = char.IsAsciiLetterOrDigit(character)
                || character == '.'
                || character == '-'
                || character == '_';

            if (!isAllowed)
            {
                throw new KpkgFormatException($"Store {description} contains an unsupported character: {value}.");
            }
        }

        if (value is "." or "..")
        {
            throw new KpkgFormatException($"Store {description} must not be a relative path segment.");
        }

        return value;
    }
}
