# Kanata Hub MVVM shell v1

Status: UI foundation.
Scope: Avalonia MVVM shell, gray/red-pink theme, right sidebar navigation, and first page placeholders.

## Goal

Kanata Hub should be composed from AXAML views and view models, not one large C#-generated `MainWindow`.

The first implementation target is the shell:

- main window;
- right sidebar navigation;
- left content area;
- Home as the first page;
- gray base palette with red/pink accents;
- command console overlay opened with `Ctrl+K`;
- no console window on Windows.

## Layout

The shell uses two main columns:

```text
Main content                                      Sidebar
┌──────────────────────────────────────────────┬──────────────┐
│ selected page                                │ Home         │
│                                              │ Templates    │
│ Home / Templates / Library / Engine / ...    │ Library      │
│                                              │ Engine       │
│                                              │              │
│                                              │ Settings     │
│                                              │ Environment  │
└──────────────────────────────────────────────┴──────────────┘
```

The sidebar is intentionally on the right and spans the full window height.

## Project structure

```text
src/Tools/Kanata.Hub/
├─ App.axaml
├─ App.cs
├─ MainWindow.axaml
├─ MainWindow.cs
├─ Program.cs
│
├─ Theme/
│  ├─ Palette.axaml
│  └─ Components.axaml
│
├─ ViewModels/
│  ├─ ViewModelBase.cs
│  ├─ PageViewModelBase.cs
│  ├─ MainWindowViewModel.cs
│  ├─ NavigationItemViewModel.cs
│  ├─ CommandConsoleViewModel.cs
│  ├─ HomePageViewModel.cs
│  ├─ TemplatesPageViewModel.cs
│  ├─ LibraryPageViewModel.cs
│  ├─ EnginePageViewModel.cs
│  ├─ SettingsPageViewModel.cs
│  └─ EnvironmentPageViewModel.cs
│
└─ Views/
   ├─ Pages/
   │  ├─ HomePageView.axaml
   │  ├─ TemplatesPageView.axaml
   │  ├─ LibraryPageView.axaml
   │  ├─ EnginePageView.axaml
   │  ├─ SettingsPageView.axaml
   │  └─ EnvironmentPageView.axaml
   │
   └─ Console/
      └─ CommandConsoleOverlay.axaml
```

## Theme

Theme tokens live in:

```text
Theme/Palette.axaml
```

Base direction:

```text
neutral gray base
red/pink accent
minimal semantic state colors
```

Change palette values there before editing individual controls.

Shared component styles live in:

```text
Theme/Components.axaml
```

## Current pages

### Home

First page. Currently a placeholder with greeting text and slots for:

- recent projects;
- create project;
- open project;
- basic template builder.

### Templates

Placeholder for template recipes and template builder flows.

### Library

Placeholder for components only: backends, runtime modules, gameplay modules, and asset packs.

### Engine

Placeholder for engine build and backend artifact maintenance.

### Settings

Placeholder for paths, package sources, updates, appearance, and about information.

### Environment

Quiet maintenance page for tools, critical components, version locks, and package store health.

## Command console

The command console overlay opens with:

```text
Ctrl+K
```

It currently shows command hints only:

```text
kanata package list
kanata package inspect <id>
kanata tool list
kanata tool inspect <id>
kanata create <name>
```

Execution is intentionally not implemented yet. Later it should go through `Kanata.Toolchain`, not through CLI stdout parsing.

## Current working scope

This foundation can already support:

- launching Hub without a console window on Windows;
- switching pages through the sidebar;
- a Home placeholder;
- AXAML-based theme changes;
- Ctrl+K command console hints;
- component-by-component UI generation.

Existing backend/toolchain code can already support package and tool operations through CLI and `Kanata.Toolchain`:

- package info;
- package verify;
- package pack;
- package install;
- package list;
- package inspect;
- tool list;
- tool inspect.

Those operations are not wired into this UI slice yet.

## Non-goals

This patch does not implement:

- real project creation;
- recent project persistence;
- real template builder logic;
- real component library data;
- environment health checks;
- command execution from the overlay;
- package explorer UI.
