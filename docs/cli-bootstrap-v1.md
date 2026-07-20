# Kanata CLI bootstrap host v1

Status: current architecture target for the installed `kanata` entrypoint.
Scope: bootstrap responsibilities, built-in modules, packageable tool boundaries, GUI direction, and command dispatch direction.

## Role

`Kanata.Cli` is the installed command-line entrypoint for Kanata.

    Kanata.Cli = bootstrap host + built-in package manager + command router foundation

The executable command name is:

    kanata

`Kanata.Cli` is not the build tool itself. It owns the stable entrypoint and dispatches commands to built-in modules or tool-provided command surfaces.

## Built-in responsibilities

The following functionality is part of the installed Kanata CLI distribution:

| Area | Responsibility |
| --- | --- |
| CLI entrypoint | Provide the `kanata` command. |
| Help/version | Keep basic CLI UX available even if packages are missing. |
| Packaging | Read, verify, pack, install, list, and inspect `.kpkg` packages. |
| Package store | Manage the local Kanata package store layout. |
| Installed registry | Track installed package records. |
| Tool visibility | List and inspect installed tool packages, commands, and optional UI surfaces. |
| Command routing foundation | Reserve the place where future package-provided commands will be dispatched. |

Packaging is built-in because Kanata must be able to install or repair tool packages without depending on a package manager that is itself installed as a package.

## Shared command execution

Package and tool commands are executed through:

    src/Tools/Kanata.Toolchain

The shared layer returns structured results. CLI and GUI surfaces render those results differently:

    Kanata.Toolchain -> Kanata.Cli text renderer
    Kanata.Toolchain -> Kanata.Hub GUI renderer

This prevents package/tool behavior from diverging between CLI and GUI.

## GUI direction

`Kanata.Hub` is the GUI surface for the local toolchain.

Current scope:

    Kanata.Hub V0 = GUI shell + Packages/Tools pages

Hub is not a bootstrap replacement. If Hub breaks, `kanata package` and `kanata tool` must remain usable from the CLI.

## Packageable tools

These parts should be packageable tool components:

| Package | Kind | Commands / surfaces |
| --- | --- | --- |
| `kanata.project` | `tool` | `create`, `new`, `validate` |
| `kanata.build` | `tool` | `restore`, `generate`, `build`, `play`, `engine` |
| `kanata.package.explorer` or Hub package surface | `tool` | optional GUI/package surface |
| `kanata.engineer` | `tool` | `engineer`, future docs commands |

## Current compatibility routing

The following commands are currently still routed directly from `Kanata.Cli` to existing command implementations:

    create
    new
    validate
    restore
    generate
    build
    play
    engine

This is a compatibility bridge until those functions are supplied by installed tool packages.

## Command priority

When dynamic tool routing is added, command lookup should use this priority:

1. Built-in bootstrap commands.
2. Installed tool commands.
3. Unknown-command error with suggestions.

Installed packages must not override built-in bootstrap commands such as `package`, `tool`, `version`, or future `doctor`.

## Process boundary

Future external tool commands should be launched as processes instead of loading tool assemblies into the `Kanata.Cli` process.

Allowed command entry point kinds for early versions:

    dotnet-assembly
    native-executable

Script entry points can be added later after platform and security rules are defined.
