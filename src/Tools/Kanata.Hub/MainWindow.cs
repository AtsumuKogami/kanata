using System.Text;
using Avalonia;
using Avalonia.Controls;
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
    private static readonly IBrush AppBackgroundBrush = Brush(10, 14, 28);
    private static readonly IBrush SidebarBrush = Brush(15, 23, 42);
    private static readonly IBrush PanelBrush = Brush(17, 25, 45);
    private static readonly IBrush PanelAltBrush = Brush(22, 33, 58);
    private static readonly IBrush InputBrush = Brush(11, 18, 32);
    private static readonly IBrush AccentBrush = Brush(96, 165, 250);
    private static readonly IBrush AccentSoftBrush = Brush(30, 64, 115);
    private static readonly IBrush TextBrush = Brush(226, 232, 240);
    private static readonly IBrush MutedTextBrush = Brush(148, 163, 184);
    private static readonly IBrush BorderBrush = Brush(51, 65, 85);
    private static readonly IBrush SuccessBrush = Brush(74, 222, 128);
    private static readonly IBrush ErrorBrush = Brush(248, 113, 113);
    private static readonly IBrush WarningBrush = Brush(251, 191, 36);

    private readonly TextBox packagePathBox = new();
    private readonly TextBox packageDetailsBox = CreateReadOnlyTextBox(wrap: true);
    private readonly TextBox fileDetailsBox = CreateReadOnlyTextBox(wrap: true);
    private readonly TextBox blockDetailsBox = CreateReadOnlyTextBox(wrap: true);
    private readonly TextBox installedDetailsBox = CreateReadOnlyTextBox(wrap: true);
    private readonly TextBox toolDetailsBox = CreateReadOnlyTextBox(wrap: true);
    private readonly TextBox operationLogBox = CreateReadOnlyTextBox(wrap: true);
    private readonly ListBox installedPackagesList = new();
    private readonly ListBox packageFilesList = new();
    private readonly ListBox packageBlocksList = new();
    private readonly ListBox toolList = new();
    private readonly ContentControl pageHost = new();
    private readonly TextBlock pageTitleBlock = new();
    private readonly TextBlock pageSubtitleBlock = new();
    private readonly TextBlock statusBlock = new();
    private readonly TextBlock openedPackageMetric = CreateMetricValue("—");
    private readonly TextBlock packageValidityMetric = CreateMetricValue("not checked");
    private readonly TextBlock installedPackagesMetric = CreateMetricValue("0");
    private readonly TextBlock installedToolsMetric = CreateMetricValue("0");
    private readonly TextBlock packageStoreBlock = new();

    private Button? packagesNavButton;
    private Button? toolsNavButton;
    private Button? projectsNavButton;
    private Button? buildNavButton;
    private Control? packagesPage;
    private Control? toolsPage;
    private Control? projectsPage;
    private Control? buildPage;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        Title = "Kanata Hub";
        Width = 1260;
        Height = 820;
        MinWidth = 1020;
        MinHeight = 680;
        Background = AppBackgroundBrush;
        Foreground = TextBrush;
        Content = CreateContent();

        ShowPackagesPage();
        RefreshStore();
        AddLog("Hub started.");
    }

    private Control CreateContent()
    {
        var root = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("260,*"),
            RowDefinitions = new RowDefinitions("*,Auto"),
            Margin = new Thickness(20),
        };

        var sidebar = CreateSidebar();
        Grid.SetColumn(sidebar, 0);
        Grid.SetRow(sidebar, 0);
        root.Children.Add(sidebar);

        var mainShell = CreateMainShell();
        Grid.SetColumn(mainShell, 1);
        Grid.SetRow(mainShell, 0);
        root.Children.Add(mainShell);

        var statusBar = CreateStatusBar();
        Grid.SetColumnSpan(statusBar, 2);
        Grid.SetRow(statusBar, 1);
        root.Children.Add(statusBar);

        return root;
    }

    private Control CreateSidebar()
    {
        packageStoreBlock.TextWrapping = TextWrapping.Wrap;
        packageStoreBlock.Foreground = MutedTextBrush;

        packagesNavButton = CreateNavButton("Packages", "Open, verify and install .kpkg files.");
        packagesNavButton.Click += (_, _) => ShowPackagesPage();

        toolsNavButton = CreateNavButton("Tools", "Installed CLI commands and GUI surfaces.");
        toolsNavButton.Click += (_, _) => ShowToolsPage();

        projectsNavButton = CreateNavButton("Projects", "Planned project workspace.");
        projectsNavButton.Click += (_, _) => ShowProjectsPage();

        buildNavButton = CreateNavButton("Build", "Planned build and play dashboard.");
        buildNavButton.Click += (_, _) => ShowBuildPage();

        var header = new StackPanel
        {
            Spacing = 4,
            Children =
            {
                new TextBlock
                {
                    Text = "KANATA",
                    Foreground = AccentBrush,
                    FontSize = 12,
                    FontWeight = FontWeight.SemiBold,
                },
                new TextBlock
                {
                    Text = "Hub",
                    FontSize = 34,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = TextBrush,
                },
                new TextBlock
                {
                    Text = "Local control surface for the modular toolchain.",
                    TextWrapping = TextWrapping.Wrap,
                    Foreground = MutedTextBrush,
                },
            },
        };

        return new Border
        {
            Background = SidebarBrush,
            BorderBrush = BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(20),
            Padding = new Thickness(16),
            Child = new Grid
            {
                RowDefinitions = new RowDefinitions("Auto,Auto,*,Auto"),
                RowSpacing = 18,
                Children =
                {
                    header,
                    new StackPanel
                    {
                        Spacing = 8,
                        Children =
                        {
                            packagesNavButton,
                            toolsNavButton,
                            projectsNavButton,
                            buildNavButton,
                        },
                    },
                    CreateSidebarCallout(),
                    CreateStoreBlock(),
                },
            },
        };
    }

    private Control CreateMainShell()
    {
        pageTitleBlock.FontSize = 28;
        pageTitleBlock.FontWeight = FontWeight.SemiBold;
        pageTitleBlock.Foreground = TextBrush;

        pageSubtitleBlock.Foreground = MutedTextBrush;
        pageSubtitleBlock.TextWrapping = TextWrapping.Wrap;

        var header = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto"),
            Margin = new Thickness(18, 0, 0, 18),
            Children =
            {
                new StackPanel
                {
                    Spacing = 4,
                    Children = { pageTitleBlock, pageSubtitleBlock },
                },
                CreatePill("Avalonia V0", AccentSoftBrush, AccentBrush),
            },
        };
        Grid.SetColumn(header.Children[1], 1);

        return new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            RowSpacing = 0,
            Margin = new Thickness(18, 0, 0, 0),
            Children = { header, pageHost },
        };
    }

    private Control CreateStatusBar()
    {
        statusBlock.Text = "Ready.";
        statusBlock.Foreground = MutedTextBrush;
        statusBlock.TextTrimming = TextTrimming.CharacterEllipsis;

        var bar = new Border
        {
            Background = PanelBrush,
            BorderBrush = BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(14, 10),
            Margin = new Thickness(0, 14, 0, 0),
            Child = statusBlock,
        };

        return bar;
    }

    private Control CreateSidebarCallout()
    {
        var callout = new Border
        {
            Background = AccentSoftBrush,
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(12),
            VerticalAlignment = VerticalAlignment.Top,
            Child = new StackPanel
            {
                Spacing = 6,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Checkpoint",
                        Foreground = AccentBrush,
                        FontWeight = FontWeight.SemiBold,
                    },
                    new TextBlock
                    {
                        Text = "V0 is focused on package and toolchain visibility. Projects and build stay locked until their tools are packaged.",
                        Foreground = TextBrush,
                        TextWrapping = TextWrapping.Wrap,
                    },
                },
            },
        };

        Grid.SetRow(callout, 2);
        return callout;
    }

    private Control CreateStoreBlock()
    {
        var block = CreatePanel("Package store", packageStoreBlock, compact: true);
        Grid.SetRow(block, 3);
        return block;
    }

    private Control CreatePackagesPage()
    {
        packagePathBox.PlaceholderText = "Path to .kpkg";
        StyleInput(packagePathBox);

        var browseButton = CreateButton("Browse", primary: true);
        browseButton.Click += async (_, _) => await BrowsePackageAsync().ConfigureAwait(true);

        var openButton = CreateButton("Open");
        openButton.Click += (_, _) => OpenPackage();

        var verifyButton = CreateButton("Verify");
        verifyButton.Click += (_, _) => VerifyPackage();

        var installButton = CreateButton("Install --force");
        installButton.Click += (_, _) => InstallPackage();

        var refreshButton = CreateButton("Refresh store");
        refreshButton.Click += (_, _) => RefreshStore();

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

        var actionCard = CreatePanel("Open package", pathRow, compact: true);

        var metrics = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*,*,*"),
            ColumnSpacing = 12,
            Children =
            {
                CreateMetricCard("Opened package", openedPackageMetric, "current .kpkg"),
                CreateMetricCard("Verification", packageValidityMetric, "last package check"),
                CreateMetricCard("Installed", installedPackagesMetric, "local packages"),
                CreateMetricCard("Tools", installedToolsMetric, "tool packages"),
            },
        };
        for (var index = 0; index < metrics.Children.Count; index++)
        {
            Grid.SetColumn(metrics.Children[index], index);
        }

        installedPackagesList.SelectionChanged += (_, _) => ShowSelectedInstalledPackage();
        packageFilesList.SelectionChanged += (_, _) => ShowSelectedPackageFile();
        packageBlocksList.SelectionChanged += (_, _) => ShowSelectedPackageBlock();
        StyleList(installedPackagesList);
        StyleList(packageFilesList);
        StyleList(packageBlocksList);

        var storePanel = new Grid
        {
            RowDefinitions = new RowDefinitions("*,240"),
            RowSpacing = 12,
            Children =
            {
                CreatePanel("Installed packages", installedPackagesList),
                CreatePanel("Installed details", installedDetailsBox),
            },
        };
        Grid.SetRow(storePanel.Children[1], 1);

        var openedPackageWorkspace = new Grid
        {
            RowDefinitions = new RowDefinitions("220,*,140"),
            RowSpacing = 12,
            Children =
            {
                CreatePanel("Opened package", packageDetailsBox),
                CreatePackageContentGrid(),
                CreatePanel("Operation log", operationLogBox, compact: true),
            },
        };
        Grid.SetRow(openedPackageWorkspace.Children[1], 1);
        Grid.SetRow(openedPackageWorkspace.Children[2], 2);

        var mainGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("360,*"),
            ColumnSpacing = 12,
            Children = { storePanel, openedPackageWorkspace },
        };
        Grid.SetColumn(openedPackageWorkspace, 1);

        return new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,*"),
            RowSpacing = 12,
            Children = { actionCard, metrics, mainGrid },
        };
    }

    private Control CreatePackageContentGrid()
    {
        var filesPanel = CreateListDetailsPanel("Payload files", packageFilesList, "File details", fileDetailsBox);
        var blocksPanel = CreateListDetailsPanel("Block table", packageBlocksList, "Block details", blockDetailsBox);

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 12,
            Children = { filesPanel, blocksPanel },
        };
        Grid.SetColumn(blocksPanel, 1);
        return grid;
    }

    private Control CreateToolsPage()
    {
        var refreshButton = CreateButton("Refresh tools", primary: true);
        refreshButton.HorizontalAlignment = HorizontalAlignment.Left;
        refreshButton.Click += (_, _) => RefreshTools();

        toolList.SelectionChanged += (_, _) => ShowSelectedTool();
        StyleList(toolList);

        var toolsMetric = CreateMetricCard("Installed tools", installedToolsMetric, "registered tool packages");
        var modelMetric = CreateMetricCard("Surface model", CreateMetricValue("commands + GUI"), "CLI required, GUI optional");

        var metrics = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,*"),
            ColumnSpacing = 12,
            Children = { toolsMetric, modelMetric },
        };
        Grid.SetColumn(modelMetric, 1);

        var contentGrid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("360,*"),
            ColumnSpacing = 12,
            Children =
            {
                CreatePanel("Installed tool packages", toolList),
                CreatePanel("Tool details", toolDetailsBox),
            },
        };
        Grid.SetColumn(contentGrid.Children[1], 1);

        return new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,Auto,*"),
            RowSpacing = 12,
            Children = { refreshButton, metrics, contentGrid },
        };
    }

    private void ShowPackagesPage()
    {
        pageTitleBlock.Text = "Packages";
        pageSubtitleBlock.Text = "Open .kpkg files, inspect the payload layout, verify integrity and manage the local package store.";
        packagesPage ??= CreatePackagesPage();
        pageHost.Content = packagesPage;
        UpdateNavigationState("Packages");
    }

    private void ShowToolsPage()
    {
        pageTitleBlock.Text = "Tools";
        pageSubtitleBlock.Text = "Inspect installed tool packages, their CLI commands and optional GUI surfaces.";
        toolsPage ??= CreateToolsPage();
        pageHost.Content = toolsPage;
        RefreshTools();
        UpdateNavigationState("Tools");
    }

    private void ShowProjectsPage()
    {
        pageTitleBlock.Text = "Projects";
        pageSubtitleBlock.Text = "Future workspace for kanata.project after project commands are packaged.";
        projectsPage ??= CreatePlaceholder("Project management will be added after project tools are packaged.", "Planned surface");
        pageHost.Content = projectsPage;
        UpdateNavigationState("Projects");
    }

    private void ShowBuildPage()
    {
        pageTitleBlock.Text = "Build";
        pageSubtitleBlock.Text = "Future build/play dashboard after kanata.build becomes a tool package.";
        buildPage ??= CreatePlaceholder("Build and play controls will be added after kanata.build becomes a tool package.", "Planned surface");
        pageHost.Content = buildPage;
        UpdateNavigationState("Build");
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
            ClearOpenedPackage();
            SetStatus("Could not open package.", isError: true);
            AddLog("Open failed: " + FormatInlineMessages(result.Messages));
            return;
        }

        ShowOpenedPackage(result.Value);
        packageValidityMetric.Text = "not checked";
        SetStatus($"Opened package {result.Value.PackageId} {result.Value.Version}.");
        AddLog($"Opened {result.Value.PackageId} {result.Value.Version}.");
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
            packageValidityMetric.Text = "failed";
            SetStatus("Verification failed before package metadata was loaded.", isError: true);
            AddLog("Verify failed: " + FormatInlineMessages(result.Messages));
            return;
        }

        if (result.Value.Package is not null)
        {
            ShowOpenedPackage(result.Value.Package);
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
        packageValidityMetric.Text = result.Value.IsValid ? "valid" : "invalid";
        SetStatus(result.Value.IsValid ? "Package is valid." : "Package is invalid.", !result.Value.IsValid);
        AddLog(result.Value.IsValid ? "Package verification passed." : "Package verification failed.");
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
            AddLog("Install failed: " + FormatInlineMessages(result.Messages));
            return;
        }

        packageDetailsBox.Text = FormatInstallSummary(result.Value);
        RefreshStore();
        SetStatus($"Installed package {result.Value.PackageId} {result.Value.Version}.");
        AddLog($"Installed {result.Value.PackageId} {result.Value.Version}.");
    }

    private void RefreshStore()
    {
        var result = PackageCommands.ListInstalledPackages();
        if (result.Value is null)
        {
            installedPackagesList.ItemsSource = Array.Empty<InstalledPackageDisplay>();
            installedPackagesMetric.Text = "0";
            packageStoreBlock.Text = FormatMessages(result.Messages);
            installedDetailsBox.Text = FormatMessages(result.Messages);
            SetStatus("Could not read package store.", isError: true);
            AddLog("Store refresh failed: " + FormatInlineMessages(result.Messages));
        }
        else
        {
            installedPackagesList.ItemsSource = result.Value.Packages
                .Select(package => new InstalledPackageDisplay(package))
                .ToArray();
            installedPackagesMetric.Text = result.Value.Packages.Count.ToString();
            packageStoreBlock.Text = result.Value.StoreRoot;
            installedDetailsBox.Text = FormatInstalledPackages(result.Value);
            SetStatus("Package store refreshed.");
        }

        RefreshTools();
    }

    private void RefreshTools()
    {
        var result = ToolCommands.ListTools();
        if (result.Value is null)
        {
            toolList.ItemsSource = Array.Empty<ToolDisplay>();
            installedToolsMetric.Text = "0";
            toolDetailsBox.Text = FormatMessages(result.Messages);
            AddLog("Tool refresh failed: " + FormatInlineMessages(result.Messages));
            return;
        }

        toolList.ItemsSource = result.Value.Tools
            .Select(tool => new ToolDisplay(tool))
            .ToArray();
        installedToolsMetric.Text = result.Value.Tools.Count.ToString();
        toolDetailsBox.Text = FormatTools(result.Value);
    }

    private void ShowOpenedPackage(PackageSummary package)
    {
        openedPackageMetric.Text = package.PackageId;
        packageDetailsBox.Text = FormatPackageSummary(package);
        packageFilesList.ItemsSource = package.Files.Select(file => new PackageFileDisplay(file)).ToArray();
        packageBlocksList.ItemsSource = package.Blocks.Select(block => new PackageBlockDisplay(block)).ToArray();
        fileDetailsBox.Text = package.Files.Count == 0 ? "No payload files." : "Select a payload file.";
        blockDetailsBox.Text = package.Blocks.Count == 0 ? "No blocks." : "Select a block.";
    }

    private void ClearOpenedPackage()
    {
        openedPackageMetric.Text = "—";
        packageValidityMetric.Text = "not checked";
        packageFilesList.ItemsSource = Array.Empty<PackageFileDisplay>();
        packageBlocksList.ItemsSource = Array.Empty<PackageBlockDisplay>();
        fileDetailsBox.Text = string.Empty;
        blockDetailsBox.Text = string.Empty;
    }

    private void ShowSelectedPackageFile()
    {
        if (packageFilesList.SelectedItem is not PackageFileDisplay selected)
        {
            return;
        }

        fileDetailsBox.Text = FormatPackageFile(selected.File);
    }

    private void ShowSelectedPackageBlock()
    {
        if (packageBlocksList.SelectedItem is not PackageBlockDisplay selected)
        {
            return;
        }

        blockDetailsBox.Text = FormatPackageBlock(selected.Block);
    }

    private void ShowSelectedInstalledPackage()
    {
        if (installedPackagesList.SelectedItem is not InstalledPackageDisplay selected)
        {
            return;
        }

        var result = PackageCommands.InspectInstalledPackages(selected.Package.PackageId);
        installedDetailsBox.Text = result.Value is null
            ? FormatMessages(result.Messages)
            : FormatInspection(result.Value);
    }

    private void ShowSelectedTool()
    {
        if (toolList.SelectedItem is not ToolDisplay selected)
        {
            return;
        }

        var result = ToolCommands.InspectTool(selected.Tool.Id);
        toolDetailsBox.Text = result.Value is null
            ? FormatMessages(result.Messages)
            : FormatTools(result.Value);
    }

    private void SetStatus(string text, bool isError = false)
    {
        statusBlock.Text = text;
        statusBlock.Foreground = isError ? ErrorBrush : MutedTextBrush;
    }

    private void AddLog(string text)
    {
        var line = $"[{DateTimeOffset.Now:HH:mm:ss}] {text}";
        operationLogBox.Text = string.IsNullOrWhiteSpace(operationLogBox.Text)
            ? line
            : line + Environment.NewLine + operationLogBox.Text;
    }

    private void UpdateNavigationState(string activePage)
    {
        SetNavState(packagesNavButton, activePage == "Packages");
        SetNavState(toolsNavButton, activePage == "Tools");
        SetNavState(projectsNavButton, activePage == "Projects");
        SetNavState(buildNavButton, activePage == "Build");
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

        builder.AppendLine($"Size: {FormatBytes(package.PackageLength)}");
        builder.AppendLine($"Installables: {package.Installables.Count}");
        builder.AppendLine($"Payload files: {package.Files.Count}");
        builder.AppendLine($"Blocks: {package.Blocks.Count}");
        builder.AppendLine();
        builder.AppendLine("Installables:");
        foreach (var installable in package.Installables)
        {
            builder.AppendLine($" - {installable.Id} {installable.Version} {installable.Kind}");
            if (!string.IsNullOrWhiteSpace(installable.Description))
            {
                builder.AppendLine($"   {installable.Description}");
            }

            if (installable.Provides.Count > 0)
            {
                builder.AppendLine($"   Provides: {string.Join(", ", installable.Provides)}");
            }

            if (installable.Dependencies.Count > 0)
            {
                builder.AppendLine($"   Dependencies: {string.Join(", ", installable.Dependencies)}");
            }
        }

        return builder.ToString();
    }

    private static string FormatPackageFile(PackageFileSummary file)
    {
        return $"Path: {file.Path}{Environment.NewLine}"
            + $"Length: {FormatBytes(file.Length)}{Environment.NewLine}"
            + $"Stored length: {FormatBytes(file.StoredLength)}{Environment.NewLine}"
            + $"Compression: {file.Compression}{Environment.NewLine}"
            + $"Payload block: {file.PayloadBlockId}{Environment.NewLine}"
            + $"SHA-256: {file.Sha256}{Environment.NewLine}";
    }

    private static string FormatPackageBlock(PackageBlockSummary block)
    {
        return $"Block: #{block.BlockId}{Environment.NewLine}"
            + $"Type: {block.Type}{Environment.NewLine}"
            + $"Offset: {block.Offset}{Environment.NewLine}"
            + $"Stored length: {FormatBytes(block.StoredLength)}{Environment.NewLine}"
            + $"Uncompressed length: {FormatBytes(block.UncompressedLength)}{Environment.NewLine}";
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

    private static string FormatInspection(KpkgPackageInspectionResult result)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Package store: {result.StoreRoot}");
        if (result.Packages.Count == 0)
        {
            builder.AppendLine("Installed packages: none");
            return builder.ToString();
        }

        builder.AppendLine("Installed package inspection:");
        foreach (var package in result.Packages)
        {
            builder.AppendLine($" - {package.PackageId} {package.Version}");
            builder.AppendLine($"   Status: {(package.IsUsable ? "usable" : "not usable")}");
            builder.AppendLine($"   Hash: {package.PackageSha256}");
            builder.AppendLine($"   Path: {package.InstalledPath}");
            foreach (var installable in package.Installables)
            {
                builder.AppendLine($"    - {installable.Id} {installable.Version} {installable.Kind}");
                builder.AppendLine($"      Status: {(installable.IsUsable ? "usable" : "not usable")}");
                builder.AppendLine($"      Descriptor: {installable.DescriptorPath}");
                if (installable.Dependencies.Count > 0)
                {
                    builder.AppendLine("      Dependencies:");
                    foreach (var dependency in installable.Dependencies)
                    {
                        builder.AppendLine($"       - {dependency.Id}: {(dependency.IsInstalled ? "installed" : "missing")}");
                    }
                }

                if (installable.Artifacts.Count > 0)
                {
                    builder.AppendLine("      Artifacts:");
                    foreach (var artifact in installable.Artifacts)
                    {
                        builder.AppendLine($"       - {artifact.Role} {artifact.PackagePath}: {(artifact.Exists ? "found" : "missing")}");
                    }
                }

                foreach (var problem in installable.Problems)
                {
                    builder.AppendLine($"      Problem: {problem}");
                }
            }

            foreach (var problem in package.Problems)
            {
                builder.AppendLine($"   Problem: {problem}");
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
            builder.AppendLine($"   Descriptor: {tool.DescriptorPath}");
            if (tool.Provides.Count > 0)
            {
                builder.AppendLine($"   Provides: {string.Join(", ", tool.Provides)}");
            }

            if (tool.Commands.Count == 0)
            {
                builder.AppendLine("   Commands: none");
            }
            else
            {
                builder.AppendLine("   Commands:");
                foreach (var command in tool.Commands)
                {
                    builder.AppendLine($"    - {command.Name}: {(command.EntryPointExists ? "found" : "missing")}");
                    builder.AppendLine($"      Entry: {command.EntryPointKind} {command.EntryPointPackagePath}");
                    if (command.Aliases.Count > 0)
                    {
                        builder.AppendLine($"      Aliases: {string.Join(", ", command.Aliases)}");
                    }
                }
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
                    builder.AppendLine($"    - {surface.Id} {surface.Kind}: {(surface.EntryPointExists ? "found" : "missing")}");
                    builder.AppendLine($"      Title: {surface.Title}");
                    builder.AppendLine($"      Optional: {surface.Optional}");
                    if (surface.Platforms.Count > 0)
                    {
                        builder.AppendLine($"      Platforms: {string.Join(", ", surface.Platforms)}");
                    }
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

    private static string FormatInlineMessages(IReadOnlyList<ToolchainMessage> messages)
    {
        if (messages.Count == 0)
        {
            return "no details";
        }

        return string.Join("; ", messages.Select(message => $"{message.Severity}: {message.Text}"));
    }

    private static string FormatBytes(ulong value)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        var size = (double)value;
        var unitIndex = 0;
        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return unitIndex == 0 ? $"{value} {units[unitIndex]}" : $"{size:0.##} {units[unitIndex]}";
    }

    private static Button CreateButton(string text, bool primary = false)
    {
        return new Button
        {
            Content = text,
            Padding = new Thickness(14, 8),
            Background = primary ? AccentBrush : PanelAltBrush,
            Foreground = primary ? AppBackgroundBrush : TextBrush,
            BorderBrush = primary ? AccentBrush : BorderBrush,
            BorderThickness = new Thickness(1),
        };
    }

    private static Button CreateNavButton(string title, string detail)
    {
        return new Button
        {
            Padding = new Thickness(12, 10),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = PanelBrush,
            BorderBrush = BorderBrush,
            BorderThickness = new Thickness(1),
            Foreground = TextBrush,
            Content = new StackPanel
            {
                Spacing = 2,
                Children =
                {
                    new TextBlock
                    {
                        Text = title,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = TextBrush,
                    },
                    new TextBlock
                    {
                        Text = detail,
                        Foreground = MutedTextBrush,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 12,
                    },
                },
            },
        };
    }

    private static void SetNavState(Button? button, bool isActive)
    {
        if (button is null)
        {
            return;
        }

        button.Background = isActive ? AccentSoftBrush : PanelBrush;
        button.BorderBrush = isActive ? AccentBrush : BorderBrush;
        button.Foreground = TextBrush;
    }

    private static TextBox CreateReadOnlyTextBox(bool wrap)
    {
        var box = new TextBox
        {
            AcceptsReturn = true,
            IsReadOnly = true,
            TextWrapping = wrap ? TextWrapping.Wrap : TextWrapping.NoWrap,
            Background = InputBrush,
            Foreground = TextBrush,
            BorderBrush = BorderBrush,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10),
        };

        return box;
    }

    private static TextBlock CreateMetricValue(string text)
    {
        return new TextBlock
        {
            Text = text,
            FontSize = 22,
            FontWeight = FontWeight.SemiBold,
            Foreground = TextBrush,
            TextTrimming = TextTrimming.CharacterEllipsis,
        };
    }

    private static Control CreateMetricCard(string label, TextBlock value, string detail)
    {
        return new Border
        {
            Background = PanelBrush,
            BorderBrush = BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Padding = new Thickness(14),
            Child = new StackPanel
            {
                Spacing = 5,
                Children =
                {
                    new TextBlock { Text = label, Foreground = MutedTextBrush, FontSize = 12 },
                    value,
                    new TextBlock { Text = detail, Foreground = MutedTextBrush, TextWrapping = TextWrapping.Wrap, FontSize = 12 },
                },
            },
        };
    }

    private static Control CreatePanel(string title, Control body, bool compact = false)
    {
        var bodyHost = body is TextBox or ListBox
            ? new ScrollViewer
            {
                Content = body,
                VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            }
            : body;

        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("Auto,*"),
            RowSpacing = compact ? 8 : 10,
            Children =
            {
                new TextBlock
                {
                    Text = title,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = TextBrush,
                    FontSize = compact ? 14 : 16,
                },
                bodyHost,
            },
        };
        Grid.SetRow(bodyHost, 1);

        return new Border
        {
            Background = PanelBrush,
            BorderBrush = BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(16),
            Padding = compact ? new Thickness(12) : new Thickness(14),
            Child = grid,
        };
    }

    private static Control CreateListDetailsPanel(string listTitle, ListBox list, string detailsTitle, TextBox details)
    {
        var grid = new Grid
        {
            RowDefinitions = new RowDefinitions("*,150"),
            RowSpacing = 10,
            Children =
            {
                CreatePanel(listTitle, list, compact: true),
                CreatePanel(detailsTitle, details, compact: true),
            },
        };
        Grid.SetRow(grid.Children[1], 1);
        return grid;
    }

    private static Control CreatePlaceholder(string text, string badge)
    {
        return new Border
        {
            Background = PanelBrush,
            BorderBrush = BorderBrush,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(20),
            Padding = new Thickness(28),
            Child = new StackPanel
            {
                Spacing = 14,
                VerticalAlignment = VerticalAlignment.Top,
                Children =
                {
                    CreatePill(badge, AccentSoftBrush, AccentBrush),
                    new TextBlock
                    {
                        Text = text,
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = TextBrush,
                        FontSize = 20,
                    },
                    new TextBlock
                    {
                        Text = "This page is intentionally locked in V0 to keep the checkpoint focused on packages and tools.",
                        TextWrapping = TextWrapping.Wrap,
                        Foreground = MutedTextBrush,
                    },
                },
            },
        };
    }

    private static Control CreatePill(string text, IBrush background, IBrush foreground)
    {
        return new Border
        {
            Background = background,
            CornerRadius = new CornerRadius(999),
            Padding = new Thickness(10, 5),
            HorizontalAlignment = HorizontalAlignment.Left,
            Child = new TextBlock
            {
                Text = text,
                Foreground = foreground,
                FontSize = 12,
                FontWeight = FontWeight.SemiBold,
            },
        };
    }

    private static void StyleInput(TextBox textBox)
    {
        textBox.Background = InputBrush;
        textBox.Foreground = TextBrush;
        textBox.BorderBrush = BorderBrush;
        textBox.BorderThickness = new Thickness(1);
        textBox.Padding = new Thickness(10, 8);
    }

    private static void StyleList(ListBox list)
    {
        list.Background = InputBrush;
        list.Foreground = TextBrush;
        list.BorderBrush = BorderBrush;
        list.BorderThickness = new Thickness(1);
    }

    private static IBrush Brush(byte r, byte g, byte b)
    {
        return new SolidColorBrush(Color.FromRgb(r, g, b));
    }

    private sealed record InstalledPackageDisplay(InstalledPackageSummary Package)
    {
        public override string ToString()
        {
            var kinds = Package.Installables.Count == 0
                ? "empty"
                : string.Join(", ", Package.Installables.Select(installable => installable.Kind).Distinct(StringComparer.Ordinal));
            return $"{Package.PackageId} {Package.Version}  ·  {kinds}";
        }
    }

    private sealed record PackageFileDisplay(PackageFileSummary File)
    {
        public override string ToString() => $"{File.Path}  ·  {FormatBytes(File.Length)}";
    }

    private sealed record PackageBlockDisplay(PackageBlockSummary Block)
    {
        public override string ToString() => $"#{Block.BlockId} {Block.Type}  ·  {FormatBytes(Block.StoredLength)}";
    }

    private sealed record ToolDisplay(KpkgInstalledToolRecord Tool)
    {
        public override string ToString()
        {
            var status = Tool.IsUsable ? "usable" : "not usable";
            return $"{Tool.Id} {Tool.Version}  ·  {status}";
        }
    }
}
