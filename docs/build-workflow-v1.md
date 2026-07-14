# Kanata build workflow v1

Status: current implementation notes  
Scope: project validation, restore, generation, build, and play commands

## Purpose

Kanata uses the project file as an intent description and resolves the concrete build for a selected target.

A game project does not directly decide which assemblies must be referenced. It describes what it wants to use, and Kanata resolves the required component graph.

## Main commands

```powershell
kanata create MyGame
cd MyGame

kanata validate
kanata restore
kanata generate
kanata build
kanata play
```

Command behavior:

| Command | Behavior |
|---|---|
| `create` | Creates a game project and validates the generated `.kanata` file. |
| `validate` | Reads the project file and checks the project structure. |
| `restore` | Validates the project, resolves the component graph, builds missing local component artifacts, and writes `Kanata.lock.json`. |
| `generate` | Runs restore and writes generated build files into `Generated/Build`. |
| `build` | Runs restore, generates build files, and calls `dotnet build` for the selected target host project. |
| `play` | Runs restore, generates build files, and calls `dotnet run` for the selected target host project. |

The default target is `desktop`. The default configuration is `Debug`.

Explicit target and configuration are supported:

```powershell
kanata restore desktop Release
kanata generate desktop Release
kanata build desktop Release
kanata play desktop Debug
```

## Current command pipeline

```text
validate
  -> restore
    -> generate
      -> build/play
```

In practice:

```text
generate = validate + restore + write generated build files
build    = validate + restore + write generated build files + dotnet build
play     = validate + restore + write generated build files + dotnet run
```

## Version source

The generated `kanataVersion` field is taken from the current Kanata toolchain version.

In the monorepo this version is defined in `Directory.Build.props` and is shared by the main Kanata projects.

Component versions are resolved from `.kcomponent` manifests. The first bundled components may use `$kanata`, which means they inherit the current Kanata toolchain version.

## Component build modes

During engine development, components are restored from the local Kanata source repository and built into the local component cache.

For normal game development, resolved components should eventually come from installed packages or binaries, not from rebuilding the whole engine repository every time.

Intended modes:

| Mode | Purpose |
|---|---|
| `source` | Engine development and local component development. |
| `package` | Installed Kanata SDK components from `.kpkg`. |
| `binary` | Already compiled third-party components. |

The first resolver implementation supports bundled source components and records their resolved artifacts in `Kanata.lock.json`.

## Future package mode

Package mode is expected to resolve components from installed `.kpkg` packages.

The package system should prepare installed package metadata and payload files before the game build pipeline runs.

Game build should consume resolved runtime/backend components. It should not execute package installation and should not read `.kpkg` files directly during normal build execution.

Tool components installed from `.kpkg` belong to the Kanata development environment. They should not be added to the game build/runtime graph unless a future explicit rule says otherwise.
