namespace Kanata.Packaging;

internal sealed record KpkgPayloadFile(
    string PhysicalPath,
    string PackagePath,
    ulong PayloadOffset,
    ulong Length,
    string Sha256);
