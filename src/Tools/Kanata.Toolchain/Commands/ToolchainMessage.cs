namespace Kanata.Toolchain.Commands;

/// <summary>
/// Represents a structured message emitted by a Kanata toolchain command.
/// </summary>
/// <param name="Severity">The message severity.</param>
/// <param name="Text">The message text.</param>
public sealed record ToolchainMessage(ToolchainMessageSeverity Severity, string Text);
