using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Kanata.Hub.Views.Pages;

/// <summary>
/// Displays the component Library page scaffold.
/// </summary>
public partial class LibraryPageView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryPageView"/> class.
    /// </summary>
    public LibraryPageView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
