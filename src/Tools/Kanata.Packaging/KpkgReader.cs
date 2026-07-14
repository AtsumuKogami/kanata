using System.Text.Json;

namespace Kanata.Packaging;

/// <summary>
/// Reads metadata from Kanata package files.
/// </summary>
public static class KpkgReader
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Disallow,
        AllowTrailingCommas = false
    };

    /// <summary>
    /// Opens a package file and reads its manifest without reading payload files.
    /// </summary>
    /// <param name="path">The package file path.</param>
    /// <returns>The parsed package metadata.</returns>
    public static KpkgPackage ReadPackage(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        using var stream = File.OpenRead(path);
        return ReadPackage(stream);
    }

    /// <summary>
    /// Reads package metadata from a seekable stream without reading payload files.
    /// </summary>
    /// <param name="stream">The package stream.</param>
    /// <returns>The parsed package metadata.</returns>
    public static KpkgPackage ReadPackage(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanSeek)
        {
            throw new KpkgFormatException("KPKG V1 requires a seekable stream.");
        }

        if (stream.Length < KpkgConstants.HeaderLength + KpkgConstants.FooterLength)
        {
            throw new KpkgFormatException("File is too small to be a KPKG package.");
        }

        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        stream.Position = 0;
        var header = KpkgHeader.Read(reader, stream.Length);

        var blocks = ReadBlockTable(reader, header);
        ValidateBlockTable(header, blocks);

        stream.Position = (long)header.FooterOffset;
        var footer = KpkgFooter.Read(reader, header);

        var manifestBlock = GetRequiredBlock(blocks, header.ManifestBlockId, KpkgBlockType.PackageManifest);
        var manifest = ReadJsonBlock<KpkgPackageManifest>(stream, manifestBlock);
        ValidateManifest(manifest, blocks);

        return new KpkgPackage(header, footer, blocks, manifest);
    }

    /// <summary>
    /// Reads a typed JSON block from the package stream.
    /// </summary>
    /// <typeparam name="T">The JSON model type.</typeparam>
    /// <param name="stream">The package stream.</param>
    /// <param name="block">The block table entry.</param>
    /// <returns>The parsed JSON model.</returns>
    public static T ReadJsonBlock<T>(Stream stream, KpkgBlockTableEntry block)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(block);

        if (block.StoredLength > int.MaxValue)
        {
            throw new KpkgFormatException("JSON block is too large to load.");
        }

        var bytes = new byte[(int)block.StoredLength];
        stream.Position = (long)block.Offset;
        var read = stream.Read(bytes, 0, bytes.Length);
        if (read != bytes.Length)
        {
            throw new KpkgFormatException("Could not read the complete JSON block.");
        }

        try
        {
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions)
                ?? throw new KpkgFormatException("JSON block deserialized to null.");
        }
        catch (JsonException exception)
        {
            throw new KpkgFormatException($"Invalid JSON block: {exception.Message}");
        }
    }

    internal static IReadOnlyList<KpkgBlockTableEntry> ReadBlockTable(BinaryReader reader, KpkgHeader header)
    {
        reader.BaseStream.Position = (long)header.BlockTableOffset;
        var blocks = new List<KpkgBlockTableEntry>((int)header.BlockCount);
        for (var index = 0; index < header.BlockCount; index++)
        {
            blocks.Add(KpkgBlockTableEntry.Read(reader));
        }

        return blocks;
    }

    internal static void ValidateBlockTable(KpkgHeader header, IReadOnlyList<KpkgBlockTableEntry> blocks)
    {
        var ids = new HashSet<uint>();
        foreach (var block in blocks)
        {
            if (!ids.Add(block.BlockId))
            {
                throw new KpkgFormatException($"Duplicate block id: {block.BlockId}.");
            }

            if (block.Offset + block.StoredLength < block.Offset)
            {
                throw new KpkgFormatException($"Block range overflows for block {block.BlockId}.");
            }

            if (block.Offset < header.BlockTableOffset + header.BlockTableLength)
            {
                throw new KpkgFormatException($"Block {block.BlockId} overlaps header or block table.");
            }

            if (block.GetEndOffset() > header.FooterOffset)
            {
                throw new KpkgFormatException($"Block {block.BlockId} range is outside package content.");
            }
        }

        var sorted = blocks.OrderBy(block => block.Offset).ToArray();
        for (var index = 1; index < sorted.Length; index++)
        {
            if (sorted[index - 1].GetEndOffset() > sorted[index].Offset)
            {
                throw new KpkgFormatException($"Blocks {sorted[index - 1].BlockId} and {sorted[index].BlockId} overlap.");
            }
        }

        GetRequiredBlock(blocks, header.ManifestBlockId, KpkgBlockType.PackageManifest);
        GetRequiredBlock(blocks, header.FileTableBlockId, KpkgBlockType.FileTable);
        GetRequiredBlock(blocks, header.IntegrityBlockId, KpkgBlockType.Integrity);

        if (!blocks.Any(block => block.KnownBlockType == KpkgBlockType.InstallableDescriptor))
        {
            throw new KpkgFormatException("Package must contain at least one installable descriptor block.");
        }
    }

    internal static KpkgBlockTableEntry GetRequiredBlock(
        IReadOnlyList<KpkgBlockTableEntry> blocks,
        uint blockId,
        KpkgBlockType expectedType)
    {
        var block = blocks.SingleOrDefault(entry => entry.BlockId == blockId);
        if (block is null)
        {
            throw new KpkgFormatException($"Required block id {blockId} was not found.");
        }

        if (block.KnownBlockType != expectedType)
        {
            throw new KpkgFormatException($"Block {blockId} must be of type {expectedType}.");
        }

        return block;
    }

    internal static void ValidateManifest(KpkgPackageManifest manifest, IReadOnlyList<KpkgBlockTableEntry> blocks)
    {
        if (!string.Equals(manifest.Format, "kanata.package", StringComparison.Ordinal))
        {
            throw new KpkgFormatException("Package manifest format must be 'kanata.package'.");
        }

        if (manifest.SchemaVersion != 1)
        {
            throw new KpkgFormatException("Package manifest schemaVersion must be 1.");
        }

        if (string.IsNullOrWhiteSpace(manifest.PackageId))
        {
            throw new KpkgFormatException("Package manifest packageId is required.");
        }

        if (string.IsNullOrWhiteSpace(manifest.Version))
        {
            throw new KpkgFormatException("Package manifest version is required.");
        }

        if (manifest.Installables.Count == 0)
        {
            throw new KpkgFormatException("Package manifest must contain at least one installable.");
        }

        var descriptorBlockIds = blocks
            .Where(block => block.KnownBlockType == KpkgBlockType.InstallableDescriptor)
            .Select(block => block.BlockId)
            .ToHashSet();

        foreach (var installable in manifest.Installables)
        {
            if (string.IsNullOrWhiteSpace(installable.Id))
            {
                throw new KpkgFormatException("Installable id is required.");
            }

            if (string.IsNullOrWhiteSpace(installable.Version))
            {
                throw new KpkgFormatException($"Installable {installable.Id} version is required.");
            }

            if (string.IsNullOrWhiteSpace(installable.Kind))
            {
                throw new KpkgFormatException($"Installable {installable.Id} kind is required.");
            }

            if (installable.DescriptorBlockId == 0)
            {
                throw new KpkgFormatException($"Installable {installable.Id} descriptorBlockId is required.");
            }

            if (!descriptorBlockIds.Contains(installable.DescriptorBlockId))
            {
                throw new KpkgFormatException($"Installable {installable.Id} references missing descriptor block {installable.DescriptorBlockId}.");
            }
        }
    }
}
