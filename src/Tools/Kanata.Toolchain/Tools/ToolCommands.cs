using Kanata.Packaging;
using Kanata.Toolchain.Commands;

namespace Kanata.Toolchain.Tools;

/// <summary>
/// Provides shared installed tool command execution for CLI and GUI surfaces.
/// </summary>
public static class ToolCommands
{
    /// <summary>
    /// Lists installed tool packages from the local package store.
    /// </summary>
    /// <param name="storeRoot">The optional package store root.</param>
    /// <returns>The structured command result.</returns>
    public static ToolchainCommandResult<KpkgToolRegistryDocument> ListTools(string? storeRoot = null)
    {
        try
        {
            var registry = KpkgToolRegistry.Read(new KpkgToolRegistryOptions
            {
                StoreRoot = storeRoot,
            });

            return ToolchainCommandResult<KpkgToolRegistryDocument>.Success(registry);
        }
        catch (Exception exception) when (IsExpectedToolException(exception))
        {
            return Failure(exception.Message);
        }
    }

    /// <summary>
    /// Inspects one installed tool package from the local package store.
    /// </summary>
    /// <param name="toolId">The tool package id.</param>
    /// <param name="storeRoot">The optional package store root.</param>
    /// <returns>The structured command result.</returns>
    public static ToolchainCommandResult<KpkgToolRegistryDocument> InspectTool(string toolId, string? storeRoot = null)
    {
        try
        {
            var registry = KpkgToolRegistry.Read(new KpkgToolRegistryOptions
            {
                TargetId = toolId,
                StoreRoot = storeRoot,
            });

            if (registry.Tools.Count == 0)
            {
                return Failure($"Installed tool package not found: {toolId}");
            }

            return ToolchainCommandResult<KpkgToolRegistryDocument>.Success(registry);
        }
        catch (Exception exception) when (IsExpectedToolException(exception))
        {
            return Failure(exception.Message);
        }
    }

    private static ToolchainCommandResult<KpkgToolRegistryDocument> Failure(string message)
    {
        return ToolchainCommandResult<KpkgToolRegistryDocument>.Failure(
            1,
            new ToolchainMessage(ToolchainMessageSeverity.Error, message));
    }

    private static bool IsExpectedToolException(Exception exception)
    {
        return exception is KpkgFormatException or IOException or UnauthorizedAccessException or ArgumentException;
    }
}
