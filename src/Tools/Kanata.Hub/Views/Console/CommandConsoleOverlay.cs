using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Kanata.Hub.Views.Console;

/// <summary>
/// Displays the command console overlay and command hints.
/// </summary>
public partial class CommandConsoleOverlay : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandConsoleOverlay"/> class.
    /// </summary>
    public CommandConsoleOverlay()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
