using System.IO.Compression;
using System.Security.Cryptography;

namespace Kanata.Packaging;

/// <summary>
/// Verifies Kanata package files according to the V1 package format rules.
/// </summary>
public static class KpkgVerifier
{
    /// <summary>
    /// Verifies a package file.
    /// </summary>
    /// <param name="path">The package file path.</param>
    /// <param name="mode">The verification mode.</param>
    /// <returns>The verification result.</returns>
    public static KpkgVerificationResult VerifyFile(string path, KpkgVerificationMode mode = KpkgVerificationMode.Full)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        try
        {
            using var stream = File.OpenRead(path);
            return Verify(stream, mode);
        }
        catch (KpkgFormatException exception)
        {
            return new KpkgVerificationResult(null, [exception.Message]);
        }
        catch (IOException exception)
        {
            return new KpkgVerificationResult(null, [$"I/O error: {exception.Message}"]);
        }
        catch (UnauthorizedAccessException exception)
        {
            return new KpkgVerificationResult(null, [$"Access error: {exception.Message}"]);
        }
    }

    /// <summary>
    /// Verifies a package stream.
    /// </summary>
    /// <param name="stream">The package stream.</param>
    /// <param name="mode">The verification mode.</param>
    /// <returns>The verification result.</returns>
    public static KpkgVerificationResult Verify(Stream stream, KpkgVerificationMode mode = KpkgVerificationMode.Full)
    {
        var errors = new List<string>();
        KpkgPackage? package = null;

        try
        {
            package = KpkgReader.ReadPackage(stream);
        }
        catch (KpkgFormatException exception)
        {
            errors.Add(exception.Message);
            return new KpkgVerificationResult(package, errors);
        }

        try
        {
            if (mode == KpkgVerificationMode.Full)
            {
                VerifyBlockHashes(stream, package.Blocks);
                VerifyFooterContentHash(stream, package.Footer);
                VerifyFileTableAndPayload(stream, package);
            }
        }
        catch (KpkgFormatException exception)
        {
            errors.Add(exception.Message);
        }

        return new KpkgVerificationResult(package, errors);
    }

    private static void VerifyBlockHashes(Stream stream, IReadOnlyList<KpkgBlockTableEntry> blocks)
    {
        foreach (var block in blocks)
        {
            var actualHash = ComputeRangeSha256(stream, block.Offset, block.StoredLength);
            if (!actualHash.SequenceEqual(block.Sha256))
            {
                throw new KpkgFormatException($"Block {block.BlockId} SHA-256 hash mismatch.");
            }
        }
    }

    private static void VerifyFooterContentHash(Stream stream, KpkgFooter footer)
    {
        var actualHash = ComputeRangeSha256(stream, 0, footer.FooterOffset);
        if (!actualHash.SequenceEqual(footer.ContentSha256))
        {
            throw new KpkgFormatException("Package footer content SHA-256 hash mismatch.");
        }
    }

    private static void VerifyFileTableAndPayload(Stream stream, KpkgPackage package)
    {
        var fileTableBlock = KpkgReader.GetRequiredBlock(
            package.Blocks,
            package.Header.FileTableBlockId,
            KpkgBlockType.FileTable);

        var fileTable = KpkgReader.ReadJsonBlock<KpkgFileTable>(stream, fileTableBlock);
        ValidateFileTable(fileTable, package.Blocks);
        VerifyFiles(stream, fileTable, package.Blocks);
    }

    private static void ValidateFileTable(KpkgFileTable fileTable, IReadOnlyList<KpkgBlockTableEntry> blocks)
    {
        if (!string.Equals(fileTable.Format, "kanata.package.fileTable", StringComparison.Ordinal))
        {
            throw new KpkgFormatException("File table format must be 'kanata.package.fileTable'.");
        }

        if (fileTable.SchemaVersion != 1)
        {
            throw new KpkgFormatException("File table schemaVersion must be 1.");
        }

        var ordinalPaths = new HashSet<string>(StringComparer.Ordinal);
        var windowsPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var payloadBlocks = blocks
            .Where(block => block.KnownBlockType == KpkgBlockType.Payload)
            .ToDictionary(block => block.BlockId);

        foreach (var file in fileTable.Files)
        {
            KpkgPathValidator.Validate(file.Path);

            if (!ordinalPaths.Add(file.Path))
            {
                throw new KpkgFormatException($"Duplicate file path: {file.Path}.");
            }

            if (!windowsPaths.Add(file.Path))
            {
                throw new KpkgFormatException($"Case-insensitive duplicate file path: {file.Path}.");
            }

            if (!payloadBlocks.TryGetValue(file.PayloadBlockId, out var payloadBlock))
            {
                throw new KpkgFormatException($"File {file.Path} references missing payload block {file.PayloadBlockId}.");
            }

            if (file.PayloadOffset + file.StoredLength < file.PayloadOffset)
            {
                throw new KpkgFormatException($"File {file.Path} payload range overflows.");
            }

            if (file.PayloadOffset + file.StoredLength > payloadBlock.StoredLength)
            {
                throw new KpkgFormatException($"File {file.Path} payload range is outside payload block {file.PayloadBlockId}.");
            }

            _ = KpkgHex.DecodeSha256(file.Sha256, $"File {file.Path} sha256");
        }
    }

    private static void VerifyFiles(Stream stream, KpkgFileTable fileTable, IReadOnlyList<KpkgBlockTableEntry> blocks)
    {
        var payloadBlocks = blocks
            .Where(block => block.KnownBlockType == KpkgBlockType.Payload)
            .ToDictionary(block => block.BlockId);

        foreach (var file in fileTable.Files)
        {
            var payloadBlock = payloadBlocks[file.PayloadBlockId];
            var storedBytes = ReadRange(stream, payloadBlock.Offset + file.PayloadOffset, file.StoredLength);
            var contentBytes = DecodeFileContent(file, storedBytes);

            if ((ulong)contentBytes.Length != file.Length)
            {
                throw new KpkgFormatException($"File {file.Path} decompressed length mismatch.");
            }

            var expectedHash = KpkgHex.DecodeSha256(file.Sha256, $"File {file.Path} sha256");
            var actualHash = SHA256.HashData(contentBytes);
            if (!actualHash.SequenceEqual(expectedHash))
            {
                throw new KpkgFormatException($"File {file.Path} SHA-256 hash mismatch.");
            }
        }
    }

    private static byte[] DecodeFileContent(KpkgFileEntry file, byte[] storedBytes)
    {
        if (string.Equals(file.Compression, "none", StringComparison.OrdinalIgnoreCase))
        {
            if (file.StoredLength != file.Length)
            {
                throw new KpkgFormatException($"Uncompressed file {file.Path} storedLength must equal length.");
            }

            return storedBytes;
        }

        if (string.Equals(file.Compression, "brotli", StringComparison.OrdinalIgnoreCase))
        {
            if (file.Length > int.MaxValue)
            {
                throw new KpkgFormatException($"File {file.Path} is too large for the current verifier implementation.");
            }

            using var input = new MemoryStream(storedBytes);
            using var brotli = new BrotliStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            var buffer = new byte[64 * 1024];
            while (true)
            {
                var read = brotli.Read(buffer, 0, buffer.Length);
                if (read == 0)
                {
                    break;
                }

                output.Write(buffer, 0, read);
                if ((ulong)output.Length > file.Length)
                {
                    throw new KpkgFormatException($"File {file.Path} decompressed data exceeds declared length.");
                }
            }

            return output.ToArray();
        }

        throw new KpkgFormatException($"Unsupported compression '{file.Compression}' for file {file.Path}.");
    }

    private static byte[] ComputeRangeSha256(Stream stream, ulong offset, ulong length)
    {
        using var sha256 = SHA256.Create();
        var buffer = new byte[64 * 1024];
        var remaining = length;
        stream.Position = (long)offset;

        while (remaining > 0)
        {
            var toRead = (int)Math.Min((ulong)buffer.Length, remaining);
            var read = stream.Read(buffer, 0, toRead);
            if (read == 0)
            {
                throw new KpkgFormatException("Unexpected end of stream while hashing package data.");
            }

            sha256.TransformBlock(buffer, 0, read, null, 0);
            remaining -= (ulong)read;
        }

        sha256.TransformFinalBlock([], 0, 0);
        return sha256.Hash ?? throw new KpkgFormatException("SHA-256 computation failed.");
    }

    private static byte[] ReadRange(Stream stream, ulong offset, ulong length)
    {
        if (length > int.MaxValue)
        {
            throw new KpkgFormatException("File entry is too large for the current verifier implementation.");
        }

        var bytes = new byte[(int)length];
        stream.Position = (long)offset;
        var read = stream.Read(bytes, 0, bytes.Length);
        if (read != bytes.Length)
        {
            throw new KpkgFormatException("Unexpected end of stream while reading payload file data.");
        }

        return bytes;
    }
}
