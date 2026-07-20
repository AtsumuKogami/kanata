using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Kanata.Packaging;

/// <summary>
/// Installs verified Kanata packages into a local artifact-first package store.
/// </summary>
public static class KpkgInstaller
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false
    };

    /// <summary>
    /// Installs a package file into the configured local package store.
    /// </summary>
    /// <param name="options">The install options.</param>
    /// <returns>The install result.</returns>
    public static KpkgInstallResult Install(KpkgInstallOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.PackagePath);

        var packagePath = Path.GetFullPath(options.PackagePath);
        var verification = KpkgVerifier.VerifyFile(packagePath, KpkgVerificationMode.Full);
        if (!verification.IsValid)
        {
            throw new KpkgFormatException($"Package verification failed: {string.Join("; ", verification.Errors)}");
        }

        var store = KpkgPackageStore.Create(options.StoreRoot);
        store.EnsureCreated();

        var packageSha256 = KpkgHex.EncodeSha256(SHA256.HashData(File.ReadAllBytes(packagePath)));

        using var stream = File.OpenRead(packagePath);
        var package = KpkgReader.ReadPackage(stream);
        var installRoot = store.GetInstallRoot(package.Manifest.PackageId, package.Manifest.Version, packageSha256);
        var tempRoot = Path.Combine(store.TempPath, $"install-{Guid.NewGuid():N}");

        if (Directory.Exists(installRoot) && !options.Overwrite)
        {
            throw new KpkgFormatException($"Package is already installed: {installRoot}. Use --force to replace it.");
        }

        try
        {
            Directory.CreateDirectory(tempRoot);
            var descriptorsDirectory = Path.Combine(tempRoot, "descriptors");
            var filesDirectory = Path.Combine(tempRoot, "files");
            Directory.CreateDirectory(descriptorsDirectory);
            Directory.CreateDirectory(filesDirectory);

            WriteInstalledDescriptors(stream, package, descriptorsDirectory);
            var fileCount = ExtractPayloadFiles(stream, package, filesDirectory);
            WriteInstallManifest(packagePath, package, packageSha256, tempRoot, fileCount);

            if (Directory.Exists(installRoot))
            {
                Directory.Delete(installRoot, recursive: true);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(installRoot)!);
            Directory.Move(tempRoot, installRoot);

            var record = CreateInstalledRecord(package, packageSha256, installRoot);
            KpkgInstalledRegistry.Upsert(store, record);

            return new KpkgInstallResult(
                package.Manifest.PackageId,
                package.Manifest.Version,
                packageSha256,
                installRoot,
                package.Manifest.Installables.Count,
                fileCount);
        }
        catch
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }

            throw;
        }
    }

    private static void WriteInstalledDescriptors(Stream stream, KpkgPackage package, string descriptorsDirectory)
    {
        foreach (var installable in package.Manifest.Installables)
        {
            var descriptorBlock = KpkgReader.GetRequiredBlock(
                package.Blocks,
                installable.DescriptorBlockId,
                KpkgBlockType.InstallableDescriptor);

            var descriptorBytes = ReadBlockBytes(stream, descriptorBlock);
            var descriptorFileName = $"{KpkgPackageStore.ToStoreSegment(installable.Id, "installable id")}{GetDescriptorExtension(installable.Kind)}";
            File.WriteAllBytes(Path.Combine(descriptorsDirectory, descriptorFileName), descriptorBytes);
        }
    }

    private static int ExtractPayloadFiles(Stream stream, KpkgPackage package, string filesDirectory)
    {
        var fileTableBlock = KpkgReader.GetRequiredBlock(
            package.Blocks,
            package.Header.FileTableBlockId,
            KpkgBlockType.FileTable);

        var fileTable = KpkgReader.ReadJsonBlock<KpkgFileTable>(stream, fileTableBlock);
        var payloadBlocks = package.Blocks
            .Where(block => block.KnownBlockType == KpkgBlockType.Payload)
            .ToDictionary(block => block.BlockId);

        foreach (var file in fileTable.Files)
        {
            KpkgPathValidator.Validate(file.Path);

            if (!payloadBlocks.TryGetValue(file.PayloadBlockId, out var payloadBlock))
            {
                throw new KpkgFormatException($"File {file.Path} references missing payload block {file.PayloadBlockId}.");
            }

            var storedBytes = ReadRange(stream, payloadBlock.Offset + file.PayloadOffset, file.StoredLength);
            var contentBytes = DecodeFileContent(file, storedBytes);
            var destinationPath = GetPayloadDestinationPath(filesDirectory, file.Path);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.WriteAllBytes(destinationPath, contentBytes);
        }

        return fileTable.Files.Count;
    }

    private static string GetPayloadDestinationPath(string filesDirectory, string packagePath)
    {
        var relativePath = packagePath.Replace('/', Path.DirectorySeparatorChar);
        var destinationPath = Path.GetFullPath(Path.Combine(filesDirectory, relativePath));
        var rootPath = Path.GetFullPath(filesDirectory);

        if (!destinationPath.StartsWith(rootPath + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            && !string.Equals(destinationPath, rootPath, StringComparison.Ordinal))
        {
            throw new KpkgFormatException($"Package path escapes the install directory: {packagePath}.");
        }

        return destinationPath;
    }

    private static void WriteInstallManifest(
        string sourcePackagePath,
        KpkgPackage package,
        string packageSha256,
        string tempRoot,
        int fileCount)
    {
        var installableNodes = new JsonArray();
        foreach (var installable in package.Manifest.Installables)
        {
            installableNodes.Add(new JsonObject
            {
                ["id"] = installable.Id,
                ["version"] = installable.Version,
                ["kind"] = installable.Kind,
                ["descriptorFile"] = $"descriptors/{KpkgPackageStore.ToStoreSegment(installable.Id, "installable id")}{GetDescriptorExtension(installable.Kind)}"
            });
        }

        var installManifest = new JsonObject
        {
            ["format"] = "kanata.package.install",
            ["schemaVersion"] = 1,
            ["packageId"] = package.Manifest.PackageId,
            ["version"] = package.Manifest.Version,
            ["packageSha256"] = packageSha256,
            ["sourcePackagePath"] = sourcePackagePath,
            ["installedAtUtc"] = DateTimeOffset.UtcNow.ToString("O"),
            ["fileCount"] = fileCount,
            ["installables"] = installableNodes
        };

        File.WriteAllBytes(
            Path.Combine(tempRoot, "package.install.kmanifest"),
            JsonSerializer.SerializeToUtf8Bytes(installManifest, JsonOptions));
    }

    private static KpkgInstalledPackageRecord CreateInstalledRecord(
        KpkgPackage package,
        string packageSha256,
        string installRoot)
    {
        return new KpkgInstalledPackageRecord
        {
            PackageId = package.Manifest.PackageId,
            Version = package.Manifest.Version,
            PackageSha256 = packageSha256,
            InstalledPath = installRoot,
            InstalledAtUtc = DateTimeOffset.UtcNow,
            Installables = package.Manifest.Installables.Select(installable => new KpkgInstalledInstallableRecord
            {
                Id = installable.Id,
                Version = installable.Version,
                Kind = installable.Kind
            }).ToArray()
        };
    }

    private static byte[] DecodeFileContent(KpkgFileEntry file, byte[] storedBytes)
    {
        if (string.Equals(file.Compression, "none", StringComparison.OrdinalIgnoreCase))
        {
            return storedBytes;
        }

        if (string.Equals(file.Compression, "brotli", StringComparison.OrdinalIgnoreCase))
        {
            using var input = new MemoryStream(storedBytes);
            using var brotli = new BrotliStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            brotli.CopyTo(output);
            return output.ToArray();
        }

        throw new KpkgFormatException($"Unsupported compression '{file.Compression}' for file {file.Path}.");
    }

    private static string GetDescriptorExtension(string kind)
    {
        return kind.ToLowerInvariant() switch
        {
            "tool" => ".ktool",
            "backend" => ".kbackend",
            "runtime" => ".kruntime",
            "editor" => ".keditor",
            "plugin" => ".kplugin",
            _ => ".kcomponent"
        };
    }

    private static byte[] ReadBlockBytes(Stream stream, KpkgBlockTableEntry block)
    {
        return ReadRange(stream, block.Offset, block.StoredLength);
    }

    private static byte[] ReadRange(Stream stream, ulong offset, ulong length)
    {
        if (length > int.MaxValue)
        {
            throw new KpkgFormatException("Package entry is too large for the current installer implementation.");
        }

        var bytes = new byte[(int)length];
        stream.Position = (long)offset;
        var totalRead = 0;
        while (totalRead < bytes.Length)
        {
            var read = stream.Read(bytes, totalRead, bytes.Length - totalRead);
            if (read == 0)
            {
                throw new KpkgFormatException("Unexpected end of stream while reading package data.");
            }

            totalRead += read;
        }

        return bytes;
    }
}
