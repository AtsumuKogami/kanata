using System.Collections.ObjectModel;

namespace Kanata.Hub.ViewModels;

/// <summary>
/// Provides state for the Hub command console overlay.
/// </summary>
public sealed class CommandConsoleViewModel : ViewModelBase
{
    private bool isOpen;
    private string query = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandConsoleViewModel"/> class.
    /// </summary>
    public CommandConsoleViewModel()
    {
        Suggestions = new ObservableCollection<string>
        {
            "kanata package list",
            "kanata package inspect <id>",
            "kanata tool list",
            "kanata tool inspect <id>",
            "kanata create <name>",
        };
    }

    /// <summary>
    /// Gets or sets a value indicating whether the console overlay is visible.
    /// </summary>
    public bool IsOpen
    {
        get => isOpen;
        set => SetProperty(ref isOpen, value);
    }

    /// <summary>
    /// Gets or sets the current console query.
    /// </summary>
    public string Query
    {
        get => query;
        set => SetProperty(ref query, value);
    }

    /// <summary>
    /// Gets command hint suggestions shown in the overlay.
    /// </summary>
    public ObservableCollection<string> Suggestions { get; }

    /// <summary>
    /// Toggles the console overlay visibility.
    /// </summary>
    public void Toggle()
    {
        IsOpen = !IsOpen;
    }

    /// <summary>
    /// Closes the console overlay.
    /// </summary>
    public void Close()
    {
        IsOpen = false;
    }
}
