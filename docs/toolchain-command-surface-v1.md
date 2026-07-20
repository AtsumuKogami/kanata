# Kanata toolchain command surface v1

Status: current architecture target for CLI/GUI command sharing.
Scope: shared command execution model used by `Kanata.Cli` and `Kanata.Hub`.

## Goal

Kanata toolchain operations must be executed through shared application commands.

    CLI and GUI must not implement separate package/tool logic.

The shared layer is:

    src/Tools/Kanata.Toolchain

It depends on low-level service modules such as `Kanata.Packaging` and exposes structured command results for user-facing shells.

## Roles

| Layer | Responsibility |
| --- | --- |
| `Kanata.Packaging` | Low-level KPKG reader/writer/verifier/installer/store APIs. |
| `Kanata.Toolchain` | Application-level command execution and structured results. |
| `Kanata.Cli` | Text renderer and process entrypoint for `kanata`. |
| `Kanata.Hub` | GUI renderer for the same command results. |

## Command result model

Shared commands return structured results:

    ToolchainCommandResult<T>
      IsSuccess
      ExitCode
      Value
      Messages

The command result is not terminal-specific and not GUI-specific.

CLI renders it as text and exit codes.
GUI renders it as panels, status badges, lists, and dialogs.

## Current shared package commands

`Kanata.Toolchain.Packages.PackageCommands` provides:

| Command | Purpose |
| --- | --- |
| `OpenPackage` | Read `.kpkg` metadata without full payload verification. |
| `VerifyPackage` | Verify `.kpkg` package structure and hashes. |
| `PackPackage` | Pack a staging directory into `.kpkg`. |
| `InstallPackage` | Install a `.kpkg` into the local package store. |
| `ListInstalledPackages` | Read the local installed package registry. |
| `InspectInstalledPackages` | Inspect installed package usability. |

## Current shared tool commands

`Kanata.Toolchain.Tools.ToolCommands` provides:

| Command | Purpose |
| --- | --- |
| `ListTools` | Read installed tool package descriptors, commands, and surfaces. |
| `InspectTool` | Inspect one installed tool package. |

## Rendering rule

Application commands must not write directly to `Console` and must not create GUI controls.

Allowed:

    command -> structured result -> CLI renderer
    command -> structured result -> GUI renderer

Not allowed:

    command -> Console.WriteLine
    command -> Avalonia controls
    CLI implementation duplicated in GUI

## Future extensions

The same model should be used for project and build operations when they are prepared for tool packages:

    kanata.project -> create/new/validate
    kanata.build   -> restore/generate/build/play/engine

These commands should become process-routed tool commands later. The shared result model should remain renderer-independent.
