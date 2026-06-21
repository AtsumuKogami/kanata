using System.Text.Json;
using Kanata.ProjectSystem.ProjectModel;

namespace Kanata.ProjectSystem.ProjectLoading;

/// <summary>
/// Reads Kanata project files from disk.
/// </summary>
public sealed class KanataProjectReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    /// Reads a Kanata project file from the specified path.
    /// </summary>
    /// <param name="projectFilePath">The path to the project file.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The loaded project model.</returns>
    public async Task<KanataProject> ReadAsync(string projectFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFilePath);

        if (!File.Exists(projectFilePath))
        {
            throw new FileNotFoundException("Kanata project file was not found.", projectFilePath);
        }

        await using var stream = File.OpenRead(projectFilePath);
        var project = await JsonSerializer.DeserializeAsync<KanataProject>(stream, JsonOptions, cancellationToken);

        return project ?? throw new InvalidOperationException("Kanata project file is empty or invalid.");
    }
}
