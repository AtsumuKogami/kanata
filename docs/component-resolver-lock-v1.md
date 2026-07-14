# Kanata component resolver and lock file v1

Status: current implementation notes with package direction  
Scope: component selection, restore, and lock file behavior

## Purpose

This document describes the first implementation of component restore in Kanata.

The resolver turns project intent into a concrete component graph for a selected target.

## Input

A project describes intent in the `.kanata` file:

```jsonc
{
  "features": ["ui", "input", "assets", "local-session"],
  "targets": {
    "desktop": {
      "backend": "kanata.backend.monogame"
    }
  }
}
```

For v1, the target component selector always requests:

```text
kanata.core
<backend declared by selected target>
```

For the default desktop target, the resolved graph is currently:

```text
kanata.core
kanata.backend.monogame
```

`kanata.backend.monogame` depends on `kanata.core`, so `kanata.core` appears first in the lock file and generated references.

## Resolve

The resolver loads bundled `.kcomponent` manifests from the local Kanata repository, then expands dependencies in dependency order.

Current resolver input is source-oriented.

Future package restore should allow the resolver to load installed package metadata produced by `.kpkg` installation.

## Restore

Restore does the following:

```text
validate project
resolve components
build missing local component artifacts
write Kanata.lock.json
```

Commands that need build artifacts call restore automatically:

```text
generate = validate + restore + write props
build    = validate + restore + write props + dotnet build
play     = validate + restore + write props + dotnet run
```

## Lock file

`Kanata.lock.json` records the resolved result for the selected target and configuration.

Current dev locks contain absolute assembly paths into the local Kanata component cache. Because of this, generated game projects ignore `Kanata.lock.json` for now.

When package restore is added, the lock file should move toward portable package identities and installed package artifact references.

## Tool components

Tool components are part of the Kanata development environment.

They are not game runtime dependencies and should not be included in the game build/runtime graph by default.

A tool package may provide commands and capabilities, but active capability binding is future installed environment or user configuration state.

Example future binding:

```text
kanata.engineering -> kanata.engineer
```

This binding must not be stored inside the `.kpkg` package itself.
