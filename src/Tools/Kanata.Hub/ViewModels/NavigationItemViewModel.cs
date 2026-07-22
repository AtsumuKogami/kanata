namespace Kanata.Hub.ViewModels;

/// <summary>
/// Represents one item in the Kanata Hub sidebar navigation.
/// </summary>
public sealed class NavigationItemViewModel : ViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationItemViewModel"/> class.
    /// </summary>
    /// <param name="title">The navigation title.</param>
    /// <param name="description">The short navigation description.</param>
    /// <param name="page">The page shown when the item is selected.</param>
    public NavigationItemViewModel(string title, string description, PageViewModelBase page)
    {
        Title = title;
        Description = description;
        Page = page;
    }

    /// <summary>
    /// Gets the navigation item title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the navigation item description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the page associated with this navigation item.
    /// </summary>
    public PageViewModelBase Page { get; }
}
