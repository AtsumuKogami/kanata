# Kanata component resolver and lock file v1

This document describes the first implementation of component restore in Kanata.

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

For v1, the target component selector always requests `kanata.core` and the backend declared by the selected target.

## Resolve

The resolver loads bundled `.kcomponent` manifests from the local Kanata repository, then expands dependencies in dependency order.

For the default desktop target, the resolved graph is currently:

```text
kanata.core
kanata.backend.monogame
```

`kanata.backend.monogame` depends on `kanata.core`, so `kanata.core` appears first in the lock file and generated references.

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

Current dev locks contain absolute assembly paths into the local Kanata component cache. Because of this, generated game projects ignore `Kanata.lock.json` for now. When package restore is added, the lock file will move toward portable package identities and will become suitable for source control.
