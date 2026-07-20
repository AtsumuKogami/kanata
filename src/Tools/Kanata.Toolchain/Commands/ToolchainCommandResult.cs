namespace Kanata.Toolchain.Commands;

/// <summary>
/// Represents a structured command execution result without a typed payload.
/// </summary>
public sealed class ToolchainCommandResult
{
    private ToolchainCommandResult(bool isSuccess, int exitCode, IReadOnlyList<ToolchainMessage> messages)
    {
        IsSuccess = isSuccess;
        ExitCode = exitCode;
        Messages = messages;
    }

    /// <summary>
    /// Gets a value indicating whether the command completed successfully.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets the recommended process exit code for CLI renderers.
    /// </summary>
    public int ExitCode { get; }

    /// <summary>
    /// Gets structured messages emitted by the command.
    /// </summary>
    public IReadOnlyList<ToolchainMessage> Messages { get; }

    /// <summary>
    /// Creates a successful command result.
    /// </summary>
    /// <param name="messages">The optional command messages.</param>
    /// <returns>The successful command result.</returns>
    public static ToolchainCommandResult Success(params ToolchainMessage[] messages)
    {
        return new ToolchainCommandResult(true, 0, messages);
    }

    /// <summary>
    /// Creates a failed command result.
    /// </summary>
    /// <param name="exitCode">The recommended non-zero exit code.</param>
    /// <param name="messages">The command error messages.</param>
    /// <returns>The failed command result.</returns>
    public static ToolchainCommandResult Failure(int exitCode, params ToolchainMessage[] messages)
    {
        return new ToolchainCommandResult(false, exitCode == 0 ? 1 : exitCode, messages);
    }
}
