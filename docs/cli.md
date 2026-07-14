# Kanata CLI

Status: current implementation notes with planned package commands  
Scope: global commands, project commands, engine commands, and package command direction

## Global commands

Global commands can be executed from any directory.

```powershell
kanata create MyGame
kanata new MyGame
kanata new game MyGame
kanata engine build Debug
kanata engine status Debug
kanata version
```

`create` and `new` create a project in the current directory unless `--output` is provided.

## Project commands

Project commands must be executed from the project root directory that contains exactly one `.kanata` file.

```powershell
kanata validate
kanata restore
kanata generate
kanata build
kanata play
```

The default target is `desktop` and the default configuration is `Debug`.

Explicit target and configuration are supported:

```powershell
kanata restore desktop Release
kanata generate desktop Release
kanata build desktop Release
kanata play desktop Debug
```

Project commands do not search parent directories. This keeps command behavior predictable and makes the project root explicit.

## Restore and lock file

`kanata restore` validates the project, resolves the required component graph for the selected target, builds missing local component artifacts, and writes `Kanata.lock.json`.

```powershell
kanata restore
```

`generate`, `build`, and `play` call restore automatically before doing their own work:

```text
validate -> restore -> generate -> build/play
```

The current dev lock file contains machine-local paths to components restored from the local Kanata source repository.

Generated game projects ignore `Kanata.lock.json` for now. Later package restore will make the lock file portable and suitable for source control.

## Engine component cache

`kanata engine build Debug` builds bundled source components into `.kanata/cache/components`.

Game builds call component preparation automatically through restore:

```powershell
kanata build
```

To force component rebuilds during a project build:

```powershell
kanata build --force-engine
```

## Planned package commands

The package commands are planned as the first CLI surface for `.kpkg`.

Initial scope:

```powershell
kanata package info <file.kpkg>
kanata package verify <file.kpkg>
kanata package pack <source>
kanata package install <file.kpkg>
```

Expected behavior:

| Command | Reads payload | Installs package | Writes registry | Purpose |
|---|---:|---:|---:|---|
| `package info` | no | no | no | Print package and installable metadata. |
| `package verify` | yes | no | no | Validate hashes, block ranges, descriptors, and file table. |
| `package pack` | yes | no | no | Create a `.kpkg` from source manifests and artifacts. |
| `package install` | yes | yes | yes | Verify and install into the local Kanata package store. |

Package install must not execute package code.

Tool commands provided by installed tool components are future environment state, not game project commands by default.
