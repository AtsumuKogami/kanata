namespace Kanata.Hub.ViewModels;

/// <summary>
/// Provides state for the Environment page scaffold.
/// </summary>
public sealed class EnvironmentPageViewModel : PageViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentPageViewModel"/> class.
    /// </summary>
    public EnvironmentPageViewModel()
        : base("Environment", "Quiet maintenance area for tools, critical components, version locks, and package store health.")
    {
    }
}
