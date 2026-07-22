using System.Collections.ObjectModel;

namespace Kanata.Hub.ViewModels;

/// <summary>
/// Provides state and navigation for the Kanata Hub shell.
/// </summary>
public sealed class MainWindowViewModel : ViewModelBase
{
    private NavigationItemViewModel selectedNavigationItem;
    private PageViewModelBase currentPage;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    public MainWindowViewModel()
    {
        var home = new NavigationItemViewModel("Home", "Start or continue projects.", new HomePageViewModel());
        NavigationItems = new ObservableCollection<NavigationItemViewModel>
        {
            home,
            new("Templates", "Project recipes and builder flows.", new TemplatesPageViewModel()),
            new("Library", "Components and backends.", new LibraryPageViewModel()),
            new("Engine", "Engine build maintenance.", new EnginePageViewModel()),
            new("Settings", "Paths, sources, appearance.", new SettingsPageViewModel()),
            new("Environment", "Tools and health checks.", new EnvironmentPageViewModel()),
        };

        selectedNavigationItem = home;
        currentPage = home.Page;
        Console = new CommandConsoleViewModel();
    }

    /// <summary>
    /// Gets sidebar navigation items.
    /// </summary>
    public ObservableCollection<NavigationItemViewModel> NavigationItems { get; }

    /// <summary>
    /// Gets the command console overlay state.
    /// </summary>
    public CommandConsoleViewModel Console { get; }

    /// <summary>
    /// Gets or sets the selected sidebar navigation item.
    /// </summary>
    public NavigationItemViewModel SelectedNavigationItem
    {
        get => selectedNavigationItem;
        set
        {
            if (SetProperty(ref selectedNavigationItem, value))
            {
                CurrentPage = value.Page;
            }
        }
    }

    /// <summary>
    /// Gets the currently displayed page.
    /// </summary>
    public PageViewModelBase CurrentPage
    {
        get => currentPage;
        private set => SetProperty(ref currentPage, value);
    }
}
