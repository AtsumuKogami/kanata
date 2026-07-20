using System.Text.Json;

namespace Kanata.Packaging;

/// <summary>
/// Reads and writes the local installed package registry.
/// </summary>
public static class KpkgInstalledRegistry
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false
    };

    /// <summary>
    /// Reads the installed registry document. Missing registry files are treated as empty registries.
    /// </summary>
    /// <param name="store">The package store.</param>
    /// <returns>The installed registry document.</returns>
    public static KpkgInstalledRegistryDocument Read(KpkgPackageStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        if (!File.Exists(store.InstalledRegistryPath))
        {
            return new KpkgInstalledRegistryDocument();
        }

        try
        {
            var document = JsonSerializer.Deserialize<KpkgInstalledRegistryDocument>(
                File.ReadAllBytes(store.InstalledRegistryPath),
                JsonOptions);

            if (document is null)
            {
                throw new KpkgFormatException("Installed package registry deserialized to null.");
            }

            if (!string.Equals(document.Format, "kanata.package.installedRegistry", StringComparison.Ordinal))
            {
                throw new KpkgFormatException("Installed package registry format must be 'kanata.package.installedRegistry'.");
            }

            if (document.SchemaVersion != 1)
            {
                throw new KpkgFormatException("Installed package registry schemaVersion must be 1.");
            }

            return document;
        }
        catch (JsonException exception)
        {
            throw new KpkgFormatException($"Invalid installed package registry: {exception.Message}");
        }
    }

    /// <summary>
    /// Adds or replaces one package record in the installed registry.
    /// </summary>
    /// <param name="store">The package store.</param>
    /// <param name="record">The package record.</param>
    public static void Upsert(KpkgPackageStore store, KpkgInstalledPackageRecord record)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(record);

        var document = Read(store);
        var packages = document.Packages
            .Where(existing => !IsSameRecord(existing, record))
            .Append(record)
            .OrderBy(package => package.PackageId, StringComparer.Ordinal)
            .ThenBy(package => package.Version, StringComparer.Ordinal)
            .ThenBy(package => package.PackageSha256, StringComparer.Ordinal)
            .ToArray();

        Write(store, new KpkgInstalledRegistryDocument { Packages = packages });
    }

    private static bool IsSameRecord(KpkgInstalledPackageRecord left, KpkgInstalledPackageRecord right)
    {
        return string.Equals(left.PackageId, right.PackageId, StringComparison.Ordinal)
            && string.Equals(left.Version, right.Version, StringComparison.Ordinal)
            && string.Equals(left.PackageSha256, right.PackageSha256, StringComparison.Ordinal);
    }

    private static void Write(KpkgPackageStore store, KpkgInstalledRegistryDocument document)
    {
        Directory.CreateDirectory(store.RegistryPath);
        var tempPath = store.InstalledRegistryPath + ".tmp";
        var bytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        File.WriteAllBytes(tempPath, bytes);

        if (File.Exists(store.InstalledRegistryPath))
        {
            File.Delete(store.InstalledRegistryPath);
        }

        File.Move(tempPath, store.InstalledRegistryPath);
    }
}
