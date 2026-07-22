namespace Kanata.Hub.ViewModels;

/// <summary>
/// Provides state for the component Library page scaffold.
/// </summary>
public sealed class LibraryPageViewModel : PageViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryPageViewModel"/> class.
    /// </summary>
    public LibraryPageViewModel()
        : base("Library", "Components, backends, modules, and asset packs available to templates and projects.")
    {
    }
}
