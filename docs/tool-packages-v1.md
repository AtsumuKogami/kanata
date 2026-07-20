# Kanata tool packages v1

Status: current tool package contract target.  
Scope: installed tool descriptors, CLI command surfaces, optional GUI surfaces, and local registry inspection.

## Role

A Kanata tool package provides development-time functionality for the Kanata toolchain.

```text
kind: tool
```

Tool packages are not part of the game build graph or runtime graph unless a future specialized tool explicitly declares otherwise.

```text
gameParticipation.build = false
gameParticipation.runtime = false
```

## Required CLI surface

Every usable tool package must declare at least one CLI command in its `.ktool` descriptor.

```json
{
  "commands": [
    {
      "name": "example-engineer",
      "description": "Runs the example Engineer tool.",
      "aliases": [],
      "entryPoint": {
        "kind": "dotnet-assembly",
        "path": "artifacts/tools/example.engineer/example.engineer.dll"
      },
      "arguments": [],
      "launchMode": "process",
      "required": true
    }
  ]
}
```

The CLI surface is mandatory because `Kanata.Cli` and automation must be able to use the tool without loading tool assemblies directly.

## Optional GUI surfaces

A tool package may declare human-facing UI surfaces.

```json
{
  "surfaces": [
    {
      "id": "package-explorer",
      "kind": "gui",
      "title": "Kanata Package Explorer",
      "description": "Opens, verifies, installs and inspects .kpkg packages.",
      "optional": true,
      "entryPoint": {
        "kind": "dotnet-wpf-app",
        "path": "artifacts/tools/kanata.package.explorer/gui/Kanata.PackageExplorer.exe"
      },
      "platforms": ["windows"]
    }
  ]
}
```

Current recognized surface kinds:

| Kind | Meaning |
|---|---|
| `gui` | Standalone desktop GUI surface. |
| `hub-panel` | Future panel hosted by Kanata Hub. Not implemented yet. |

The first implementation only records and inspects surfaces. It does not launch GUI surfaces yet.

## Process boundary

Tool commands should be launched as separate processes.

Supported initial entrypoint kinds:

| Kind | Meaning |
|---|---|
| `dotnet-assembly` | Launch through `dotnet <assembly>`. |
| `native-executable` | Launch executable directly. |
| `dotnet-wpf-app` | GUI application entrypoint for Windows WPF surfaces. |

`Kanata.Cli` must not load installed tool assemblies into its own process for normal command execution.

## Built-in command priority

Built-in bootstrap commands take priority over installed tool commands.

Reserved built-in groups:

```text
package
tool
version
help
doctor
```

Installed tool packages must not override these groups.

## Local tool registry view

`kanata tool list` reports installed tool packages.

```powershell
kanata tool list
```

`kanata tool inspect <tool-id>` reports detailed tool descriptor state.

```powershell
kanata tool inspect example.engineer
```

The registry view is computed from the installed package registry and the installed `.ktool` descriptors. It currently checks:

```text
- tool descriptor exists
- commands[] exists
- command entrypoints resolve to installed files
- surfaces[] entrypoints resolve to installed files when declared
- duplicate command names across installed tools
- duplicate aliases across installed tools
```

## Package Explorer direction

Kanata is moving toward GUI coverage for the whole toolchain, including package management.

The first GUI checkpoint should be a tool package:

```text
kanata.package.explorer
```

It should provide:

```text
required CLI surface
optional GUI surface
```

The GUI must use the same package services as `Kanata.Cli` instead of implementing separate package logic.
