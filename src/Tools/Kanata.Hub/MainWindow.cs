using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Kanata.Packaging;
using Kanata.Toolchain.Commands;
using Kanata.Toolchain.Packages;
using Kanata.Toolchain.Tools;

namespace Kanata.Hub;

/// <summary>
/// Main Kanata Hub window.
/// </summary>
public sealed class MainWindow : Window
{
    private readonly TextBox packagePathBox = new();
    private readonly TextBox packageDetailsBox = new();
    private readonly TextBox installedPackagesBox = new();
    private readonly TextBox toolsBox = new();
    private readonly TextBlock statusBlock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        Title = "Kanata Hub";
        Width = 1120;
        Height = 720;
        MinWidth = 900;
        MinHeight = 560;
        Content = CreateContent();

        RefreshStore();
    }

    private Control CreateContent()
    {
        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("220,*"),
            RowDefinitions = new RowDefinitions("*,Auto"),
            Margin = new Thickness(14),
        };

        var sidebar = CreateSidebar();
        Grid.SetColumn(sidebar, 0);
        Grid.SetRow(sidebar, 0);
        root.Children.Add(sidebar);

        var tabs = new TabControl
        {
            Margin = new Thickness(14, 0, 0, 0),
            ItemsSource = new[]
            {
                new TabItem { Header = "Packages", Content = CreatePackagesPage() },
                new TabItem { Header = "Tools", Content = CreateToolsPage() },
                new TabItem { Header = "Projects", Content = CreatePlaceholder("Project management will be added after project/build tools are packaged.") },
                new TabItem { Header = "Build", Content = CreatePlaceholder("Build and play controls will be added after kanata.build becomes a tool package.") },
            },
        };
        Grid.SetColumn(tabs, 1);
        Grid.SetRow(tabs, 0);
        root.Children.Add(tabs);

        statusBlock.Text = "Ready.";
        statusBlock.Margin = new Thickness(0, 10, 0, 0);
        statusBlock.Foreground = Brushes.Gray;
        Grid.SetColumnSpan(statusBlock, 2);
        Grid.SetRow(statusBlock, 1);
        root.Children.Add(statusBlock);

        return root;
    }

    private static Control CreateSidebar()
    {
        return new StackPanel
        {
            Spacing = 10,
            Children =
            {
                new TextBlock
                {
                    Text = "Kanata Hub",
                    FontSize = 26,
                    FontWeight = FontWeight.SemiBold,
                },
                new TextBlock
                {
                    Text = "V0 focuses on local package and toolchain inspection.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = Brushes.Gray,
                },
                CreateStatusBadge("Packages", "active"),
                CreateStatusBadge("Tools", "active"),
                CreateStatusBadge("Projects", "later"),
                CreateStatusBadge("Build", "later"),
            },
        };
    }

    private static Control CreateStatusBadge(string title, string status)
    {
        return new Border
        {
            BorderBrush = Brushes.DimGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(10, 8),
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock { Text = title, FontWeight = FontWeight.SemiBold },
                    new TextBlock { Text = status, Foreground = Brushes.Gray },
                },
            },
        };
    }

    private Control CreatePackagesPage()
    {
        packagePathBox.PlaceholderText = "Path to .kpkg";

        var browseButton = new Button { Content = "Browse" };
        browseButton.Click += async (_, _) => await BrowsePackageAsync().ConfigureAwait(true);

        var openButton = new Button { Content = "Open" };
        openButton.Click += (_, _) => OpenPackage();

        var verifyButton = new Button { Content = "Verify" };
        verifyButton.Click += (_, _) => VerifyPackage();

        var installButton = new Button { Content = "Install --force" };
        installButton.Click += (_, _) => InstallPackage();

        var refreshButton = new Button { Content = "Refresh store" };
        refreshButton.Click += (_, _) => RefreshStore();

        packageDetailsBox.AcceptsReturn = true;
        packageDetailsBox.IsReadOnly = true;
        packageDetailsBox.TextWrapping = TextWrapping.NoWrap;

        installedPackagesBox.AcceptsReturn = true;
        installedPackagesBox.IsReadOnly = true;
        installedPackagesBox.TextWrapping = TextWrapping.NoWrap;

        var pathRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto,Auto,Auto,Auto,Auto"),
            ColumnSpacing = 8,
            Children = { packagePathBox, browseButton, openButton, verifyButton, installButton, refreshButton },
        };
        Grid.SetColumn(browseButton, 1);
        Grid.SetColumn(openButton, 2);
        Grid.SetColumn(verifyButton, 3);
        Grid.SetColumn(installButton, 4);
        Grid.SetColumn(refreshButton, 5);

        var detailsGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 12,
            Children =
            {
                CreatePanel("Opened package", packageDetailsBox),
                CreatePanel("Installed packages", installedPackagesBox),
            },
        };
        Grid.SetColumn(detailsGrid.Children[1], 1);

        return new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            RowSpacing = 12,
            Children = { pathRow, detailsGrid },
        };
    }

    private Control CreateToolsPage()
    {
        var refreshButton = new Button
        {
            Content = "Refresh tools",
            HorizontalAlignment = HorizontalAlignment.Left,
        };
        refreshButton.Click += (_, _) => RefreshTools();

        toolsBox.AcceptsReturn = true;
        toolsBox.IsReadOnly = true;
        toolsBox.TextWrapping = TextWrapping.NoWrap;

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            RowSpacing = 12,
            Children =
            {
                refreshButton,
                CreatePanel("Installed tool packages", toolsBox),
            },
        };
        Grid.SetRow(grid.Children[1], 1);
        return grid;
    }

    private static Control CreatePlaceholder(string text)
    {
        return new Border
        {
            BorderBrush = Brushes.DimGray,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16),
            Child = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                Foreground = Brushes.Gray,
            },
        };
    }

    private static Control CreatePanel(string title, Control body)
    {
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            RowSpacing = 8,
        };

        grid.Children.Add(new TextBlock
        {
            Text = title,
            FontWeight = FontWeight.SemiBold,
            FontSize = 16,
        });

        var bodyHost = body is TextBox
            ? new ScrollViewer
            {
                Content = body,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            }
            : body;

        Grid.SetRow(bodyHost, 1);
        grid.Children.Add(bodyHost);
        return grid;
    }

    private async Task BrowsePackageAsync()
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open Kanata package",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Kanata packages") { Patterns = new[] { "*.kpkg" } },
                FilePickerFileTypes.All,
            },
        }).ConfigureAwait(true);

        var file = files.FirstOrDefault();
        if (file is not null)
        {
            packagePathBox.Text = file.Path.LocalPath;
            OpenPackage();
        }
    }

    private void OpenPackage()
    {
        var packagePath = packagePathBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(packagePath))
        {
            SetStatus("Package path is empty.", isError: true);
            return;
        }

        var result = PackageCommands.OpenPackage(packagePath);
        if (!result.IsSuccess || result.Value is null)
        {
            packageDetailsBox.Text = FormatMessages(result.Messages);
            SetStatus("Could not open package.", isError: true);
            return;
        }

        packageDetailsBox.Text = FormatPackageSummary(result.Value);
        SetStatus($"Opened package {result.Value.PackageId} {result.Value.Version}.");
    }

    private void VerifyPackage()
    {
        var packagePath = packagePathBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(packagePath))
        {
            SetStatus("Package path is empty.", isError: true);
            return;
        }

        var result = PackageCommands.VerifyPackage(packagePath);
        if (result.Value is null)
        {
            packageDetailsBox.Text = FormatMessages(result.Messages);
            SetStatus("Verification failed before package metadata was loaded.", isError: true);
            return;
        }

        var builder = new StringBuilder();
        builder.AppendLine(result.Value.IsValid ? "Package is valid." : "Package is invalid.");
        foreach (var error in result.Value.Errors)
        {
            builder.AppendLine($" - {error}");
        }

        if (result.Value.Package is not null)
        {
            builder.AppendLine();
            builder.Append(FormatPackageSummary(result.Value.Package));
        }

        packageDetailsBox.Text = builder.ToString();
        SetStatus(result.Value.IsValid ? "Package is valid." : "Package is invalid.", !result.Value.IsValid);
    }

    private void InstallPackage()
    {
        var packagePath = packagePathBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(packagePath))
        {
            SetStatus("Package path is empty.", isError: true);
            return;
        }

        var result = PackageCommands.InstallPackage(packagePath, overwrite: true);
        if (!result.IsSuccess || result.Value is null)
        {
            packageDetailsBox.Text = FormatMessages(result.Messages);
            SetStatus("Package install failed.", isError: true);
            return;
        }

        packageDetailsBox.Text = FormatInstallSummary(result.Value);
        RefreshStore();
        SetStatus($"Installed package {result.Value.PackageId} {result.Value.Version}.");
    }

    private void RefreshStore()
    {
        var result = PackageCommands.ListInstalledPackages();
        installedPackagesBox.Text = result.Value is null
            ? FormatMessages(result.Messages)
            : FormatInstalledPackages(result.Value);

        RefreshTools();
    }

    private void RefreshTools()
    {
        var result = ToolCommands.ListTools();
        toolsBox.Text = result.Value is null
            ? FormatMessages(result.Messages)
            : FormatTools(result.Value);
    }

    private void SetStatus(string text, bool isError = false)
    {
        statusBlock.Text = text;
        statusBlock.Foreground = isError ? Brushes.IndianRed : Brushes.Gray;
    }

    private static string FormatPackageSummary(PackageSummary package)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Package: {package.PackageId}");
        builder.AppendLine($"Version: {package.Version}");
        if (!string.IsNullOrWhiteSpace(package.DisplayName))
        {
            builder.AppendLine($"Display name: {package.DisplayName}");
        }

        if (!string.IsNullOrWhiteSpace(package.Description))
        {
            builder.AppendLine($"Description: {package.Description}");
        }

        builder.AppendLine($"Size: {package.PackageLength} bytes");
        builder.AppendLine();
        builder.AppendLine("Installables:");
        foreach (var installable in package.Installables)
        {
            builder.AppendLine($" - {installable.Id} {installable.Version} {installable.Kind}");
            if (installable.Provides.Count > 0)
            {
                builder.AppendLine($"   Provides: {string.Join(", ", installable.Provides)}");
            }

            if (installable.Dependencies.Count > 0)
            {
                builder.AppendLine($"   Dependencies: {string.Join(", ", installable.Dependencies)}");
            }
        }

        builder.AppendLine();
        builder.AppendLine("Blocks:");
        foreach (var block in package.Blocks)
        {
            builder.AppendLine($" - #{block.BlockId} {block.Type} offset={block.Offset} length={block.StoredLength}");
        }

        return builder.ToString();
    }

    private static string FormatInstallSummary(PackageInstallSummary install)
    {
        return $"Package installed: {install.PackageId} {install.Version}{Environment.NewLine}"
            + $"Hash: {install.PackageSha256}{Environment.NewLine}"
            + $"Path: {install.InstalledPath}{Environment.NewLine}"
            + $"Installables: {install.InstallableCount}{Environment.NewLine}"
            + $"Files: {install.FileCount}{Environment.NewLine}";
    }

    private static string FormatInstalledPackages(InstalledPackageListResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Package store: {result.StoreRoot}");
        if (result.Packages.Count == 0)
        {
            builder.AppendLine("Installed packages: none");
            return builder.ToString();
        }

        builder.AppendLine("Installed packages:");
        foreach (var package in result.Packages)
        {
            builder.AppendLine($" - {package.PackageId} {package.Version}");
            builder.AppendLine($"   Hash: {package.PackageSha256}");
            builder.AppendLine($"   Path: {package.InstalledPath}");
            foreach (var installable in package.Installables)
            {
                builder.AppendLine($"    - {installable.Id} {installable.Version} {installable.Kind}");
            }
        }

        return builder.ToString();
    }

    private static string FormatTools(KpkgToolRegistryDocument registry)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Package store: {registry.StoreRoot}");
        if (registry.Tools.Count == 0)
        {
            builder.AppendLine("Installed tool packages: none");
            return builder.ToString();
        }

        builder.AppendLine("Installed tool packages:");
        foreach (var tool in registry.Tools)
        {
            builder.AppendLine($" - {tool.Id} {tool.Version}");
            builder.AppendLine($"   Status: {(tool.IsUsable ? "usable" : "not usable")}");
            builder.AppendLine($"   Package: {tool.PackageId} {tool.PackageVersion}");
            if (tool.Commands.Count == 0)
            {
                builder.AppendLine("   Commands: none");
            }
            else
            {
                builder.AppendLine($"   Commands: {string.Join(", ", tool.Commands.Select(command => command.Name))}");
            }

            if (tool.Surfaces.Count == 0)
            {
                builder.AppendLine("   Surfaces: none");
            }
            else
            {
                builder.AppendLine("   Surfaces:");
                foreach (var surface in tool.Surfaces)
                {
                    builder.AppendLine($"    - {surface.Id} {surface.Kind} {(surface.EntryPointExists ? "found" : "missing")}");
                }
            }

            foreach (var problem in tool.Problems)
            {
                builder.AppendLine($"   Problem: {problem}");
            }
        }

        foreach (var problem in registry.Problems)
        {
            builder.AppendLine($"Registry problem: {problem}");
        }

        return builder.ToString();
    }

    private static string FormatMessages(IReadOnlyList<ToolchainMessage> messages)
    {
        if (messages.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(Environment.NewLine, messages.Select(message => $"{message.Severity}: {message.Text}"));
    }
}
