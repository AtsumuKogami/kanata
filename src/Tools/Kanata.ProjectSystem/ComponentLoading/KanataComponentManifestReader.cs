using System.Text.Json;
using Kanata.ProjectSystem.ComponentModel;

namespace Kanata.ProjectSystem.ComponentLoading;

/// <summary>
/// Reads Kanata component manifests from disk.
/// </summary>
public sealed class KanataComponentManifestReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = false,
    };

    /// <summary>
    /// Reads a component manifest file as JSONC.
    /// </summary>
    /// <param name="manifestPath">Absolute or relative path to the manifest file.</param>
    /// <param name="cancellationToken">Token used to cancel file reading.</param>
    /// <returns>The loaded component manifest.</returns>
    public async Task<KanataComponentManifest> ReadAsync(
        string manifestPath,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException("Kanata component manifest was not found.", manifestPath);
        }

        var json = await File.ReadAllTextAsync(manifestPath, cancellationToken).ConfigureAwait(false);
        var manifest = JsonSerializer.Deserialize<KanataComponentManifest>(json, JsonOptions);

        return manifest ?? throw new InvalidOperationException("Failed to read Kanata component manifest.");
    }
}
