using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Kanata.Packaging;

/// <summary>
/// Writes Kanata package files from a prepared package staging directory.
/// </summary>
public static class KpkgWriter
{
    private const string PackageManifestFileName = "package.kmanifest";

    private static readonly HashSet<string> DescriptorExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".kcomponent",
        ".ktool",
        ".kbackend",
        ".kruntime",
        ".keditor",
        ".kplugin",
        ".kmanifest"
    };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false
    };

    /// <summary>
    /// Packs a staging directory into a .kpkg file.
    /// </summary>
    /// <param name="options">The writer options.</param>
    /// <returns>Information about the written package.</returns>
    public static KpkgWriteResult PackDirectory(KpkgWriterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.SourceDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.OutputPath);

        var sourceDirectory = Path.GetFullPath(options.SourceDirectory);
        var outputPath = Path.GetFullPath(options.OutputPath);

        if (!Directory.Exists(sourceDirectory))
        {
            throw new KpkgFormatException($"Package source directory does not exist: {sourceDirectory}.");
        }

        if (File.Exists(outputPath) && !options.Overwrite)
        {
            throw new KpkgFormatException($"Output package already exists: {outputPath}. Use --force to overwrite it.");
        }

        var manifestPath = Path.Combine(sourceDirectory, PackageManifestFileName);
        if (!File.Exists(manifestPath))
        {
            throw new KpkgFormatException($"Package source must contain {PackageManifestFileName}.");
        }

        var manifest = ReadJsonObject(manifestPath, "package manifest");
        var descriptorNodes = ReadDescriptorNodes(sourceDirectory);
        var normalizedManifest = NormalizeManifest(manifest, descriptorNodes, out var orderedDescriptors);
        var payloadFiles = CollectPayloadFiles(sourceDirectory, out var payloadBytes);
        ValidateDescriptorPayloadReferences(orderedDescriptors, payloadFiles.Select(file => file.PackagePath).ToHashSet(StringComparer.Ordinal));

        var manifestBytes = JsonSerializer.SerializeToUtf8Bytes(normalizedManifest, JsonOptions);
        var descriptorBytes = orderedDescriptors
            .Select(descriptor => JsonSerializer.SerializeToUtf8Bytes(descriptor.Node, JsonOptions))
            .ToArray();

        var fileTableBlockId = orderedDescriptors.Last().BlockId + 1;
        var payloadBlockId = payloadFiles.Count > 0 ? fileTableBlockId + 1 : 0u;
        var integrityBlockId = payloadBlockId != 0 ? payloadBlockId + 1 : fileTableBlockId + 1;

        var fileTableBytes = JsonSerializer.SerializeToUtf8Bytes(CreateFileTable(payloadFiles, payloadBlockId), JsonOptions);
        var integrityBytes = JsonSerializer.SerializeToUtf8Bytes(CreateIntegrityBlock(), JsonOptions);

        var blockBuilder = new List<BlockPayload>
        {
            new(1, KpkgBlockType.PackageManifest, manifestBytes)
        };

        for (var index = 0; index < orderedDescriptors.Count; index++)
        {
            blockBuilder.Add(new(orderedDescriptors[index].BlockId, KpkgBlockType.InstallableDescriptor, descriptorBytes[index]));
        }

        blockBuilder.Add(new(fileTableBlockId, KpkgBlockType.FileTable, fileTableBytes));

        if (payloadBlockId != 0)
        {
            blockBuilder.Add(new(payloadBlockId, KpkgBlockType.Payload, payloadBytes.ToArray()));
        }

        blockBuilder.Add(new(integrityBlockId, KpkgBlockType.Integrity, integrityBytes));

        var packageBytes = BuildPackageBytes(blockBuilder, fileTableBlockId, integrityBlockId);
        var outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        File.WriteAllBytes(outputPath, packageBytes);

        var packageId = GetRequiredString(normalizedManifest, "packageId", "package manifest");
        var version = GetRequiredString(normalizedManifest, "version", "package manifest");
        var installableCount = normalizedManifest["installables"]?.AsArray().Count ?? 0;

        return new KpkgWriteResult(
            outputPath,
            packageId,
            version,
            installableCount,
            payloadFiles.Count,
            blockBuilder.Count,
            packageBytes.Length);
    }

    private static List<DescriptorNode> ReadDescriptorNodes(string sourceDirectory)
    {
        var descriptorsDirectory = Path.Combine(sourceDirectory, "descriptors");
        if (!Directory.Exists(descriptorsDirectory))
        {
            throw new KpkgFormatException("Package source must contain a descriptors directory.");
        }

        var descriptorFiles = Directory.EnumerateFiles(descriptorsDirectory, "*", SearchOption.TopDirectoryOnly)
            .Where(path => DescriptorExtensions.Contains(Path.GetExtension(path)))
            .OrderBy(path => Path.GetFileName(path), StringComparer.Ordinal)
            .ToArray();

        if (descriptorFiles.Length == 0)
        {
            throw new KpkgFormatException("Package source descriptors directory must contain at least one descriptor file.");
        }

        var descriptors = new List<DescriptorNode>();
        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var descriptorFile in descriptorFiles)
        {
            var node = ReadJsonObject(descriptorFile, $"descriptor {Path.GetFileName(descriptorFile)}");
            var id = GetRequiredString(node, "id", descriptorFile);
            if (!ids.Add(id))
            {
                throw new KpkgFormatException($"Duplicate descriptor id: {id}.");
            }

            descriptors.Add(new DescriptorNode(id, descriptorFile, node, 0));
        }

        return descriptors;
    }

    private static JsonObject NormalizeManifest(
        JsonObject manifest,
        IReadOnlyList<DescriptorNode> descriptors,
        out List<DescriptorNode> orderedDescriptors)
    {
        if (!string.Equals(GetRequiredString(manifest, "format", "package manifest"), "kanata.package", StringComparison.Ordinal))
        {
            throw new KpkgFormatException("Package manifest format must be 'kanata.package'.");
        }

        if (GetRequiredInt32(manifest, "schemaVersion", "package manifest") != 1)
        {
            throw new KpkgFormatException("Package manifest schemaVersion must be 1.");
        }

        _ = GetRequiredString(manifest, "packageId", "package manifest");
        _ = GetRequiredString(manifest, "version", "package manifest");

        if (manifest["installables"] is not JsonArray installables || installables.Count == 0)
        {
            throw new KpkgFormatException("Package manifest must contain a non-empty installables array.");
        }

        var descriptorsById = descriptors.ToDictionary(descriptor => descriptor.Id, StringComparer.Ordinal);
        var usedDescriptorIds = new HashSet<string>(StringComparer.Ordinal);
        orderedDescriptors = [];

        uint nextDescriptorBlockId = 2;
        foreach (var installableNode in installables)
        {
            if (installableNode is not JsonObject installable)
            {
                throw new KpkgFormatException("Package manifest installables entries must be objects.");
            }

            var installableId = GetRequiredString(installable, "id", "installable");
            _ = GetRequiredString(installable, "version", $"installable {installableId}");
            _ = GetRequiredString(installable, "kind", $"installable {installableId}");

            if (!descriptorsById.TryGetValue(installableId, out var descriptor))
            {
                throw new KpkgFormatException($"Installable {installableId} has no matching descriptor file.");
            }

            ValidateDescriptorMatchesInstallable(descriptor.Node, installable, installableId);
            usedDescriptorIds.Add(installableId);

            descriptor = descriptor with { BlockId = nextDescriptorBlockId };
            orderedDescriptors.Add(descriptor);
            installable["descriptorBlockId"] = nextDescriptorBlockId;
            nextDescriptorBlockId++;
        }

        var unusedDescriptors = descriptors
            .Where(descriptor => !usedDescriptorIds.Contains(descriptor.Id))
            .Select(descriptor => descriptor.Id)
            .ToArray();
        if (unusedDescriptors.Length > 0)
        {
            throw new KpkgFormatException($"Descriptor files are not referenced by package manifest: {string.Join(", ", unusedDescriptors)}.");
        }

        return manifest;
    }

    private static void ValidateDescriptorMatchesInstallable(JsonObject descriptor, JsonObject installable, string installableId)
    {
        var descriptorVersion = GetRequiredString(descriptor, "version", $"descriptor {installableId}");
        var installableVersion = GetRequiredString(installable, "version", $"installable {installableId}");
        if (!string.Equals(descriptorVersion, installableVersion, StringComparison.Ordinal))
        {
            throw new KpkgFormatException($"Descriptor {installableId} version does not match package manifest installable version.");
        }

        var descriptorKind = GetRequiredString(descriptor, "kind", $"descriptor {installableId}");
        var installableKind = GetRequiredString(installable, "kind", $"installable {installableId}");
        if (!string.Equals(descriptorKind, installableKind, StringComparison.Ordinal))
        {
            throw new KpkgFormatException($"Descriptor {installableId} kind does not match package manifest installable kind.");
        }
    }

    private static List<KpkgPayloadFile> CollectPayloadFiles(string sourceDirectory, out MemoryStream payloadBytes)
    {
        var physicalFiles = new List<(string PhysicalPath, string PackagePath)>();
        CollectPayloadRoot(sourceDirectory, "artifacts", physicalFiles);
        CollectPayloadRoot(sourceDirectory, "sources", physicalFiles);

        var orderedFiles = physicalFiles
            .OrderBy(file => file.PackagePath, StringComparer.Ordinal)
            .ToArray();

        var ordinalPaths = new HashSet<string>(StringComparer.Ordinal);
        var windowsPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in orderedFiles)
        {
            KpkgPathValidator.Validate(file.PackagePath);
            if (!ordinalPaths.Add(file.PackagePath))
            {
                throw new KpkgFormatException($"Duplicate payload path: {file.PackagePath}.");
            }

            if (!windowsPaths.Add(file.PackagePath))
            {
                throw new KpkgFormatException($"Case-insensitive duplicate payload path: {file.PackagePath}.");
            }
        }

        payloadBytes = new MemoryStream();
        var result = new List<KpkgPayloadFile>();
        foreach (var file in orderedFiles)
        {
            var offset = (ulong)payloadBytes.Length;
            using var input = File.OpenRead(file.PhysicalPath);
            using var sha256 = SHA256.Create();
            var buffer = new byte[64 * 1024];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                payloadBytes.Write(buffer, 0, read);
                sha256.TransformBlock(buffer, 0, read, null, 0);
            }

            sha256.TransformFinalBlock([], 0, 0);
            var hash = sha256.Hash ?? throw new KpkgFormatException("SHA-256 computation failed.");
            result.Add(new KpkgPayloadFile(
                file.PhysicalPath,
                file.PackagePath,
                offset,
                (ulong)new FileInfo(file.PhysicalPath).Length,
                KpkgHex.EncodeSha256(hash)));
        }

        payloadBytes.Position = 0;
        return result;
    }

    private static void CollectPayloadRoot(
        string sourceDirectory,
        string rootName,
        ICollection<(string PhysicalPath, string PackagePath)> files)
    {
        var root = Path.Combine(sourceDirectory, rootName);
        if (!Directory.Exists(root))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(root, file).Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
            files.Add((file, $"{rootName}/{relative}"));
        }
    }

    private static void ValidateDescriptorPayloadReferences(IEnumerable<DescriptorNode> descriptors, ISet<string> payloadPaths)
    {
        foreach (var descriptor in descriptors)
        {
            ValidatePathArrayReferences(descriptor, "artifacts", payloadPaths);
            ValidatePathArrayReferences(descriptor, "sources", payloadPaths);
            ValidateCommandEntryPointReferences(descriptor, payloadPaths);
        }
    }

    private static void ValidatePathArrayReferences(DescriptorNode descriptor, string propertyName, ISet<string> payloadPaths)
    {
        if (descriptor.Node[propertyName] is not JsonArray items)
        {
            return;
        }

        foreach (var item in items)
        {
            if (item is not JsonObject itemObject || itemObject["path"] is null)
            {
                continue;
            }

            var path = GetRequiredString(itemObject, "path", $"descriptor {descriptor.Id} {propertyName} entry");
            KpkgPathValidator.Validate(path);
            if (!payloadPaths.Contains(path))
            {
                throw new KpkgFormatException($"Descriptor {descriptor.Id} references missing payload file: {path}.");
            }
        }
    }


    private static void ValidateCommandEntryPointReferences(DescriptorNode descriptor, ISet<string> payloadPaths)
    {
        if (descriptor.Node["commands"] is not JsonArray commands)
        {
            return;
        }

        foreach (var command in commands)
        {
            if (command is not JsonObject commandObject
                || commandObject["entryPoint"] is not JsonObject entryPoint
                || entryPoint["path"] is null)
            {
                continue;
            }

            var path = GetRequiredString(entryPoint, "path", $"descriptor {descriptor.Id} command entryPoint");
            KpkgPathValidator.Validate(path);
            if (!payloadPaths.Contains(path))
            {
                throw new KpkgFormatException($"Descriptor {descriptor.Id} command entry point references missing payload file: {path}.");
            }
        }
    }

    private static KpkgFileTable CreateFileTable(IReadOnlyList<KpkgPayloadFile> payloadFiles, uint payloadBlockId)
    {
        return new KpkgFileTable
        {
            Format = "kanata.package.fileTable",
            SchemaVersion = 1,
            Files = payloadFiles
                .Select(file => new KpkgFileEntry
                {
                    Path = file.PackagePath,
                    PayloadBlockId = payloadBlockId,
                    PayloadOffset = file.PayloadOffset,
                    StoredLength = file.Length,
                    Length = file.Length,
                    Compression = "none",
                    Sha256 = file.Sha256
                })
                .ToArray()
        };
    }

    private static JsonObject CreateIntegrityBlock()
    {
        return new JsonObject
        {
            ["format"] = "kanata.package.integrity",
            ["schemaVersion"] = 1,
            ["blockHash"] = "sha256",
            ["fileHash"] = "sha256",
            ["contentHash"] = "sha256"
        };
    }

    private static byte[] BuildPackageBytes(
        IReadOnlyList<BlockPayload> blocks,
        uint fileTableBlockId,
        uint integrityBlockId)
    {
        var blockTableOffset = KpkgConstants.HeaderLength;
        var blockTableLength = (ulong)blocks.Count * KpkgConstants.BlockTableEntryLength;
        var currentOffset = blockTableOffset + blockTableLength;

        var entries = new List<BlockEntryData>();
        foreach (var block in blocks)
        {
            var hash = SHA256.HashData(block.Bytes);
            entries.Add(new BlockEntryData(
                block.BlockId,
                block.Type,
                currentOffset,
                (ulong)block.Bytes.Length,
                hash,
                block.Bytes));
            currentOffset += (ulong)block.Bytes.Length;
        }

        var footerOffset = currentOffset;
        var packageLength = footerOffset + KpkgConstants.FooterLength;
        if (packageLength > int.MaxValue)
        {
            throw new KpkgFormatException("The current in-memory KPKG writer supports packages up to 2 GB.");
        }

        using var preFooter = new MemoryStream((int)packageLength);
        using (var writer = new BinaryWriter(preFooter, Encoding.UTF8, leaveOpen: true))
        {
            WriteHeader(writer, (uint)blocks.Count, blockTableOffset, blockTableLength, packageLength, footerOffset, fileTableBlockId, integrityBlockId);
            foreach (var entry in entries)
            {
                WriteBlockTableEntry(writer, entry);
            }

            foreach (var entry in entries)
            {
                writer.Write(entry.Bytes);
            }
        }

        var contentHash = SHA256.HashData(preFooter.ToArray());
        using var output = new MemoryStream((int)packageLength);
        preFooter.Position = 0;
        preFooter.CopyTo(output);
        using (var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true))
        {
            WriteFooter(writer, packageLength, footerOffset, contentHash);
        }

        return output.ToArray();
    }

    private static void WriteHeader(
        BinaryWriter writer,
        uint blockCount,
        ulong blockTableOffset,
        ulong blockTableLength,
        ulong packageLength,
        ulong footerOffset,
        uint fileTableBlockId,
        uint integrityBlockId)
    {
        writer.Write(KpkgConstants.Magic);
        writer.Write(KpkgConstants.FormatMajor);
        writer.Write(KpkgConstants.FormatMinor);
        writer.Write(KpkgConstants.HeaderLength);
        writer.Write(0u);
        writer.Write(blockCount);
        writer.Write(blockTableOffset);
        writer.Write(blockTableLength);
        writer.Write(packageLength);
        writer.Write(footerOffset);
        writer.Write(1u);
        writer.Write(fileTableBlockId);
        writer.Write(integrityBlockId);
        writer.Write(0u);
        writer.Write(new byte[56]);
    }

    private static void WriteBlockTableEntry(BinaryWriter writer, BlockEntryData entry)
    {
        writer.Write(entry.BlockId);
        writer.Write((uint)entry.Type);
        writer.Write((uint)KpkgBlockFlags.None);
        writer.Write((ushort)KpkgCompression.None);
        writer.Write((ushort)KpkgHashAlgorithm.Sha256);
        writer.Write(entry.Offset);
        writer.Write(entry.StoredLength);
        writer.Write(entry.StoredLength);
        writer.Write(entry.Sha256);
        writer.Write(new byte[24]);
    }

    private static void WriteFooter(BinaryWriter writer, ulong packageLength, ulong footerOffset, byte[] contentHash)
    {
        writer.Write(KpkgConstants.FooterMagic);
        writer.Write(KpkgConstants.FooterLength);
        writer.Write(KpkgConstants.FormatMajor);
        writer.Write(KpkgConstants.FormatMinor);
        writer.Write(packageLength);
        writer.Write(footerOffset);
        writer.Write((ushort)KpkgHashAlgorithm.Sha256);
        writer.Write((ushort)0);
        writer.Write(0u);
        writer.Write(contentHash);
        writer.Write(new byte[24]);
    }

    private static JsonObject ReadJsonObject(string path, string description)
    {
        try
        {
            var node = JsonNode.Parse(File.ReadAllText(path), documentOptions: new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Disallow,
                AllowTrailingCommas = false
            });

            return node as JsonObject ?? throw new KpkgFormatException($"{description} must be a JSON object.");
        }
        catch (JsonException exception)
        {
            throw new KpkgFormatException($"Invalid {description}: {exception.Message}");
        }
    }

    private static string GetRequiredString(JsonObject node, string propertyName, string context)
    {
        if (node[propertyName] is null)
        {
            throw new KpkgFormatException($"{context} must contain '{propertyName}'.");
        }

        var value = node[propertyName]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new KpkgFormatException($"{context} property '{propertyName}' must not be empty.");
        }

        return value;
    }

    private static int GetRequiredInt32(JsonObject node, string propertyName, string context)
    {
        if (node[propertyName] is null)
        {
            throw new KpkgFormatException($"{context} must contain '{propertyName}'.");
        }

        return node[propertyName]!.GetValue<int>();
    }

    private sealed record DescriptorNode(string Id, string PhysicalPath, JsonObject Node, uint BlockId);

    private sealed record BlockPayload(uint BlockId, KpkgBlockType Type, byte[] Bytes);

    private sealed record BlockEntryData(
        uint BlockId,
        KpkgBlockType Type,
        ulong Offset,
        ulong StoredLength,
        byte[] Sha256,
        byte[] Bytes);
}
