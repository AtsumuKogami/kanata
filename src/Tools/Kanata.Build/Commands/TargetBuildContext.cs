using Kanata.ProjectSystem.ProjectModel;

namespace Kanata.Build.Commands;

internal sealed class TargetBuildContext
{
    public required string ProjectFilePath { get; init; }

    public required string ProjectRoot { get; init; }

    public required KanataProject Project { get; init; }

    public required string TargetName { get; init; }

    public required KanataTarget Target { get; init; }

    public required string Configuration { get; init; }

    public required string HostProjectPath { get; init; }
}
