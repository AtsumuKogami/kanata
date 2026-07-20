namespace Kanata.Toolchain.Commands;

/// <summary>
/// Represents a structured command execution result with a typed payload.
/// </summary>
/// <typeparam name="T">The command payload type.</typeparam>
public sealed class ToolchainCommandResult<T>
{
    private ToolchainCommandResult(bool isSuccess, int exitCode, T? value, IReadOnlyList<ToolchainMessage> messages)
    {
        IsSuccess = isSuccess;
        ExitCode = exitCode;
        Value = value;
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
    /// Gets the command payload when execution produced one.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// Gets structured messages emitted by the command.
    /// </summary>
    public IReadOnlyList<ToolchainMessage> Messages { get; }

    /// <summary>
    /// Creates a successful command result.
    /// </summary>
    /// <param name="value">The command payload.</param>
    /// <param name="messages">The optional command messages.</param>
    /// <returns>The successful command result.</returns>
    public static ToolchainCommandResult<T> Success(T value, params ToolchainMessage[] messages)
    {
        return new ToolchainCommandResult<T>(true, 0, value, messages);
    }

    /// <summary>
    /// Creates a failed command result.
    /// </summary>
    /// <param name="exitCode">The recommended non-zero exit code.</param>
    /// <param name="messages">The command error messages.</param>
    /// <returns>The failed command result.</returns>
    public static ToolchainCommandResult<T> Failure(int exitCode, params ToolchainMessage[] messages)
    {
        return new ToolchainCommandResult<T>(false, exitCode == 0 ? 1 : exitCode, default, messages);
    }
}
