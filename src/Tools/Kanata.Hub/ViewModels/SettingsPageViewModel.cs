namespace Kanata.Hub.ViewModels;

/// <summary>
/// Provides state for the Settings page scaffold.
/// </summary>
public sealed class SettingsPageViewModel : PageViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsPageViewModel"/> class.
    /// </summary>
    public SettingsPageViewModel()
        : base("Settings", "General preferences, paths, update sources, theme settings, and about information.")
    {
    }
}
