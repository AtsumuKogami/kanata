# Kanata Hub v0

Status: current GUI checkpoint.
Scope: Avalonia GUI shell for the local Kanata toolchain.

## Current implementation status

`Kanata.Hub` is the current Avalonia GUI surface for Kanata toolchain management.

The current V0 implementation started as a package/tool inspection surface:

```text
Kanata Hub V0 = GUI shell + Packages/Tools pages
```

This was useful as a technical checkpoint, but it is not the final product structure.

## Product direction

Hub should move toward the product model defined in:

```text
docs/hub-information-architecture-v1.md
```

The target direction is:

```text
Kanata Hub is project-first.
Packages are infrastructure.
Components are library items.
Templates are recipes.
Environment is maintenance.
```

## Target navigation

The next Hub rewrite should use this top-level structure:

```text
Home
Templates
Library
Engine
Settings
Environment
```

`Home` combines the old Projects landing screen with project creation:

```text
Home
├─ Recent Projects
├─ Create Project
├─ Open Project
└─ Basic Template Builder
```

## Technology

Hub V0 uses Avalonia.

The project is:

```text
src/Tools/Kanata.Hub
```

Current package references:

```text
Avalonia
Avalonia.Desktop
Avalonia.Themes.Fluent
```

## Dependency direction

Hub depends on `Kanata.Toolchain`.
Hub does not depend on `Kanata.Cli`.
Hub does not depend on `Kanata.Build`.

```text
Kanata.Packaging -> Kanata.Toolchain -> Kanata.Hub
                                  |
                                  -> Kanata.Cli
```

This keeps CLI and GUI behavior aligned while allowing different renderers.

## Rewrite rule

If a feature needs package, project, template, resolver, or toolchain logic, implement it first in `Kanata.Toolchain` or the appropriate lower-level service module, then render it in CLI or Hub.

Do not parse CLI stdout from Hub.
Do not duplicate package/project logic in Avalonia code-behind.

