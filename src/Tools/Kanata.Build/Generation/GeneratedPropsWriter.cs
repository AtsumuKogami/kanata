using System.Xml.Linq;
using Kanata.ProjectSystem.ProjectModel;

namespace Kanata.Build.Generation;

internal sealed class GeneratedPropsWriter
{
    public async Task<string> WriteAsync(
        KanataProject project,
        string projectFilePath,
        string targetName,
        string configuration,
        CancellationToken cancellationToken = default)
    {
        var projectRoot = Path.GetDirectoryName(Path.GetFullPath(projectFilePath)) ?? Directory.GetCurrentDirectory();
        var generatedRoot = Path.GetFullPath(Path.Combine(projectRoot, project.Paths!.Generated!));
        var buildRoot = Path.Combine(generatedRoot, "Build");
        var propsPath = Path.Combine(buildRoot, $"{targetName}.{configuration}.props");

        Directory.CreateDirectory(buildRoot);

        var source = project.Source!;
        var document = new XDocument(
            new XElement("Project",
                new XElement("PropertyGroup",
                    new XElement("KanataProjectFile", Path.GetFullPath(projectFilePath)),
                    new XElement("KanataProjectRoot", projectRoot),
                    new XElement("KanataTarget", targetName),
                    new XElement("KanataConfiguration", configuration),
                    new XElement("KanataProjectId", project.Id ?? string.Empty)),
                new XElement("ItemGroup",
                    ProjectReference(projectRoot, source.Shared!),
                    ProjectReference(projectRoot, source.Logic!),
                    ProjectReference(projectRoot, source.View!))));

        await File.WriteAllTextAsync(propsPath, document.ToString(), cancellationToken).ConfigureAwait(false);
        return propsPath;
    }

    private static XElement ProjectReference(string projectRoot, string relativePath)
    {
        return new XElement("ProjectReference",
            new XAttribute("Include", Path.GetFullPath(Path.Combine(projectRoot, relativePath))));
    }
}
