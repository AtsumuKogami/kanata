using System.Text.Json.Serialization;

namespace Kanata.Packaging;

/// <summary>
/// Represents one package installation record in the local package registry.
/// </summary>
public sealed class KpkgInstalledPackageRecord
{
    /// <summary>
    /// Gets the installed package id.
    /// </summary>
    [JsonPropertyName("packageId")]
    public string PackageId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the installed package version.
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;

    /// <summary>
    /// Gets the SHA-256 hash of the installed source package file.
    /// </summary>
    [JsonPropertyName("packageSha256")]
    public string PackageSha256 { get; init; } = string.Empty;

    /// <summary>
    /// Gets the absolute installed package root path.
    /// </summary>
    [JsonPropertyName("installedPath")]
    public string InstalledPath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the UTC installation timestamp.
    /// </summary>
    [JsonPropertyName("installedAtUtc")]
    public DateTimeOffset InstalledAtUtc { get; init; }

    /// <summary>
    /// Gets the installables contained in this installed package.
    /// </summary>
    [JsonPropertyName("installables")]
    public IReadOnlyList<KpkgInstalledInstallableRecord> Installables { get; init; } = [];
}
