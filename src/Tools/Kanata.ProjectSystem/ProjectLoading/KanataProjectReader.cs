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
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = false,
    };

    /// <summary>
    /// Reads a Kanata project file asynchronously.
    /// </summary>
    /// <param name="projectFilePath">Path to a <c>.kanata</c> project file.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The loaded Kanata project model.</returns>
    public async Task<KanataProject> ReadAsync(string projectFilePath, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectFilePath);

        if (!File.Exists(projectFilePath))
        {
            throw new FileNotFoundException("Kanata project file was not found.", projectFilePath);
        }

        var json = await File.ReadAllTextAsync(projectFilePath, cancellationToken).ConfigureAwait(false);
        var project = JsonSerializer.Deserialize<KanataProject>(json, JsonOptions);

        return project ?? throw new InvalidOperationException("Kanata project file is empty or invalid.");
    }
}
