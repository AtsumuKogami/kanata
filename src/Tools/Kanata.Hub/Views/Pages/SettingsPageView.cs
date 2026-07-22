using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Kanata.Hub.Views.Pages;

/// <summary>
/// Displays the Settings page scaffold.
/// </summary>
public partial class SettingsPageView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPageView"/> class.
    /// </summary>
    public SettingsPageView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
