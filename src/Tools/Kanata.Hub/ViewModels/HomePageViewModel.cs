namespace Kanata.Hub.ViewModels;

/// <summary>
/// Provides state for the Home page placeholder.
/// </summary>
public sealed class HomePageViewModel : PageViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HomePageViewModel"/> class.
    /// </summary>
    public HomePageViewModel()
        : base("Home", "Create, open, or continue a Kanata project.")
    {
    }

    /// <summary>
    /// Gets the welcome title shown on the first UI slice.
    /// </summary>
    public string WelcomeTitle => "Welcome to Kanata Hub";

    /// <summary>
    /// Gets the welcome text shown on the first UI slice.
    /// </summary>
    public string WelcomeText => "This first MVVM shell keeps the opening screen intentionally simple. Projects, creation flow, and the basic template builder will be filled in component by component.";
}
