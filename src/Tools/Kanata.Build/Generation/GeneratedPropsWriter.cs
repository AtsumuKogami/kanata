using System.Xml.Linq;
using Kanata.Build.Components;
using Kanata.ProjectSystem.ProjectModel;

namespace Kanata.Build.Generation;

internal sealed class GeneratedPropsWriter
{
    public async Task<string> WriteAsync(
        KanataProject project,
        string projectFilePath,
        string targetName,
        string configuration,
        IReadOnlyList<ComponentReference>? componentReferences = null,
        CancellationToken cancellationToken = default)
    {
        var projectRoot = Path.GetDirectoryName(Path.GetFullPath(projectFilePath)) ?? Directory.GetCurrentDirectory();
        var generatedRoot = Path.GetFullPath(Path.Combine(projectRoot, project.Paths!.Generated!));
        var buildRoot = Path.Combine(generatedRoot, "Build");
        var propsPath = Path.Combine(buildRoot, $"{targetName}.{configuration}.props");

        Directory.CreateDirectory(buildRoot);

        var source = project.Source!;
        var itemGroup = new XElement("ItemGroup",
            ProjectReference(projectRoot, source.Shared!),
            ProjectReference(projectRoot, source.Logic!),
            ProjectReference(projectRoot, source.View!));

        foreach (var componentReference in componentReferences ?? Array.Empty<ComponentReference>())
        {
            itemGroup.Add(AssemblyReference(componentReference));
        }

        var document = new XDocument(
            new XElement("Project",
                new XElement("PropertyGroup",
                    new XElement("KanataProjectFile", Path.GetFullPath(projectFilePath)),
                    new XElement("KanataProjectRoot", projectRoot),
                    new XElement("KanataTarget", targetName),
                    new XElement("KanataConfiguration", configuration),
                    new XElement("KanataProjectId", project.Id ?? string.Empty)),
                itemGroup));

        await File.WriteAllTextAsync(propsPath, document.ToString(), cancellationToken).ConfigureAwait(false);
        return propsPath;
    }

    private static XElement ProjectReference(string projectRoot, string relativePath)
    {
        return new XElement("ProjectReference",
            new XAttribute("Include", Path.GetFullPath(Path.Combine(projectRoot, relativePath))));
    }

    private static XElement AssemblyReference(ComponentReference reference)
    {
        return new XElement("Reference",
            new XAttribute("Include", reference.AssemblyName),
            new XElement("HintPath", reference.AssemblyPath),
            new XElement("Private", "true"));
    }
}
