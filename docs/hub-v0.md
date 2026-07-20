# Kanata Hub v0

Status: current GUI checkpoint.
Scope: Avalonia GUI shell for the local Kanata toolchain.

## Role

`Kanata.Hub` is the GUI surface for Kanata toolchain management.

V0 is intentionally narrow:

    Kanata Hub V0 = GUI shell + Packages/Tools pages

It does not replace `Kanata.Cli`. The CLI remains the bootstrap entrypoint and recovery path.

## Technology

Hub V0 uses Avalonia.

The project is:

    src/Tools/Kanata.Hub

Current package references:

    Avalonia
    Avalonia.Desktop
    Avalonia.Themes.Fluent

## Dependency direction

Hub depends on `Kanata.Toolchain`.
Hub does not depend on `Kanata.Cli`.
Hub does not depend on `Kanata.Build`.

    Kanata.Packaging -> Kanata.Toolchain -> Kanata.Hub
                                  |
                                  -> Kanata.Cli

This keeps CLI and GUI behavior aligned while allowing different renderers.

## V0 pages

### Packages

Current package features:

- open `.kpkg` metadata;
- verify `.kpkg` packages;
- install `.kpkg` packages with overwrite enabled;
- show installed packages from the local package store.

### Tools

Current tool features:

- list installed tool packages;
- show tool commands;
- show optional UI surfaces;
- show registry-level problems.

### Placeholders

These pages are placeholders in V0:

- Projects;
- Build.

They are visible only to reserve the product direction. They should not grow until `kanata.project` and `kanata.build` are prepared as tool packages.

## Non-goals for V0

V0 does not include:

- marketplace;
- remote registry;
- package update flow;
- uninstall/repair flow;
- project creation UI;
- build/play UI;
- embedded terminal;
- editor/runtime UI.

## Rule

If a feature needs package or toolchain logic, implement it first in `Kanata.Toolchain`, then render it in CLI or Hub.
