namespace Kanata.Hub.ViewModels;

/// <summary>
/// Represents a page shown inside the Kanata Hub shell.
/// </summary>
public abstract class PageViewModelBase : ViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PageViewModelBase"/> class.
    /// </summary>
    /// <param name="title">The page title.</param>
    /// <param name="subtitle">The page subtitle.</param>
    protected PageViewModelBase(string title, string subtitle)
    {
        Title = title;
        Subtitle = subtitle;
    }

    /// <summary>
    /// Gets the page title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the page subtitle.
    /// </summary>
    public string Subtitle { get; }
}
