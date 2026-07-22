using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Kanata.Hub.ViewModels;

namespace Kanata.Hub;

/// <summary>
/// Represents the main Kanata Hub shell window.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        KeyDown += OnKeyDown;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel)
        {
            return;
        }

        if (e.Key == Key.K && e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            viewModel.Console.Toggle();
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape && viewModel.Console.IsOpen)
        {
            viewModel.Console.Close();
            e.Handled = true;
        }
    }
}
