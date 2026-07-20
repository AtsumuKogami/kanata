namespace Kanata.Toolchain.Commands;

/// <summary>
/// Defines the severity of a message emitted by a Kanata toolchain command.
/// </summary>
public enum ToolchainMessageSeverity
{
    /// <summary>
    /// Informational message.
    /// </summary>
    Information,

    /// <summary>
    /// Warning message.
    /// </summary>
    Warning,

    /// <summary>
    /// Error message.
    /// </summary>
    Error,
}
