using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Kanata.Hub.Views.Pages;

/// <summary>
/// Displays the Environment page scaffold.
/// </summary>
public partial class EnvironmentPageView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentPageView"/> class.
    /// </summary>
    public EnvironmentPageView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
