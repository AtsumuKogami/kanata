namespace Kanata.Build.Components;

internal sealed class ComponentArtifactPaths
{
    public required string Root { get; init; }

    public required string LibRoot { get; init; }

    public required string AssemblyPath { get; init; }

    public required string BuildInfoPath { get; init; }

    public required string ManifestCopyPath { get; init; }
}
