using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Kanata.Hub.Views.Pages;

/// <summary>
/// Displays the Templates page scaffold.
/// </summary>
public partial class TemplatesPageView : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplatesPageView"/> class.
    /// </summary>
    public TemplatesPageView()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
