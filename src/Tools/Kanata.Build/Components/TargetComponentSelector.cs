using Kanata.ProjectSystem.ProjectModel;

namespace Kanata.Build.Components;

internal static class TargetComponentSelector
{
    public static IReadOnlyList<string> Select(KanataProject project, KanataTarget target)
    {
        var result = new List<string>
        {
            "kanata.core",
        };

        if (!string.IsNullOrWhiteSpace(target.Backend))
        {
            result.Add(target.Backend);
        }

        return result.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }
}
