using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Kanata.Hub.Views.Pages;

/// <summary>
/// Displays the Engine page scaffold.
/// </summary>
public partial class EnginePageView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnginePageView"/> class.
    /// </summary>
    public EnginePageView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
