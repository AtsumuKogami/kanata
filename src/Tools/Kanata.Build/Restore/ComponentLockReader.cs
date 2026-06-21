using System.Text.Json;
using Kanata.ProjectSystem.LockModel;

namespace Kanata.Build.Restore;

internal sealed class ComponentLockReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = false,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public async Task<KanataLockFile?> ReadAsync(
        string projectRoot,
        CancellationToken cancellationToken = default)
    {
        var lockPath = Path.Combine(projectRoot, "Kanata.lock.json");
        if (!File.Exists(lockPath))
        {
            return null;
        }

        await using var stream = File.OpenRead(lockPath);
        return await JsonSerializer
            .DeserializeAsync<KanataLockFile>(stream, JsonOptions, cancellationToken)
            .ConfigureAwait(false);
    }
}
