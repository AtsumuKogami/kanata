using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Kanata.Hub.Views.Pages;

/// <summary>
/// Displays the project-first Home page placeholder.
/// </summary>
public partial class HomePageView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomePageView"/> class.
    /// </summary>
    public HomePageView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
