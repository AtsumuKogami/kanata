# Kanata CLI

Status: current CLI feature map.  
Scope: installed `kanata` entrypoint, built-in package commands, tool package visibility, and currently direct project/build commands.

## Entrypoint

`Kanata.Cli` is the command-line bootstrap host for Kanata.

The installed command name is:

```powershell
kanata
```

`Kanata.Cli` owns the stable entrypoint. Other tools may provide commands later, but the user-facing command remains `kanata`.

## Built-in bootstrap commands

These commands are part of the bootstrap host and must be available without installing additional tool packages:

```powershell
kanata package info <file.kpkg>
kanata package verify <file.kpkg> [--fast]
kanata package pack <source-folder> -o <output.kpkg> [--force]
kanata package install <file.kpkg> [--force]
kanata package list
kanata package inspect [package-or-installable-id]
kanata tool list
kanata tool inspect <tool-id>
kanata version
```

`package` commands are built into the Kanata CLI distribution because the package manager cannot depend on being installed through the package manager.

`tool list` and `tool inspect` read the local installed package registry, resolve installed `.ktool` descriptors, and report CLI commands plus optional UI surfaces declared by installed tool packages.

## Current direct commands

The following commands are currently routed directly by `Kanata.Cli` to existing command implementations:

```powershell
kanata create <name> [--output <path>] [--id <id>] [--force]
kanata new <template> [--output <path>] [--id <id>] [--force]
kanata new game <name> [--output <path>] [--id <id>] [--force]
kanata validate
kanata restore [target] [configuration] [--force-engine]
kanata generate [target] [configuration] [--force-engine]
kanata build [target] [configuration] [--force-engine]
kanata play [target] [configuration] [--force-engine]
kanata engine build [configuration] [--force]
kanata engine status [configuration]
```

These commands are candidates for future tool packages:

| Command group | Future package | Notes |
|---|---|---|
| `create`, `new`, `validate` | `kanata.project` | Project creation and validation surface. |
| `restore`, `generate`, `build`, `play`, `engine` | `kanata.build` | Build pipeline and project execution surface. |

Until dynamic tool routing is implemented, `Kanata.Cli` calls the existing command implementations directly.

## Package commands

| Command | Reads payload | Installs package | Writes registry | Purpose |
|---|---:|---:|---:|---|
| `package info` | no | no | no | Print package and installable metadata. |
| `package verify` | yes | no | no | Validate hashes, block ranges, descriptors, and file table. |
| `package pack` | yes | no | no | Create a `.kpkg` from source manifests and artifacts. |
| `package install` | yes | yes | yes | Verify and install into the local Kanata package store. |
| `package list` | no | no | no | Print installed package registry records. |
| `package inspect` | no | no | no | Inspect installed package usability, artifacts, dependencies, descriptors, command entrypoints, and UI surfaces. |

Package install must not execute package code.

## Local package store

By default packages are installed to:

```text
%USERPROFILE%\.kanata\packages\
```

The store can be overridden with:

```powershell
$env:KANATA_PACKAGE_STORE = "D:\Dev\KanataStore"
```

## Examples

```powershell
kanata package list
kanata package inspect kanata.backend.monogame
kanata tool list
kanata tool inspect example.engineer
kanata create MyGame
cd MyGame
kanata validate
kanata build
kanata play
```
