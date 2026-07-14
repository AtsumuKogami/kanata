# Kanata project format v1

Status: current implementation notes  
Scope: `.kanata` project file

## Purpose

A Kanata project file uses JSONC syntax and the `.kanata` extension.

The project file describes game intent. It does not directly list all resolved runtime/backend assemblies.

## Minimal file

```jsonc
{
  "$schema": "https://schemas.kanata.dev/project/v1/kanata.project.schema.json",
  "format": "kanata.project",
  "schemaVersion": 1,
  "id": "my-game",
  "name": "My Game",
  "projectVersion": "0.1.0",
  "kanataVersion": "0.1.0",
  "paths": {
    "content": "Content",
    "source": "Source",
    "generated": "Generated",
    "settings": "ProjectSettings"
  },
  "source": {
    "shared": "Source/Shared/MyGame.Shared.csproj",
    "logic": "Source/Logic/MyGame.Logic.csproj",
    "view": "Source/View/MyGame.View.csproj"
  },
  "features": [
    "ui",
    "input",
    "assets",
    "local-session"
  ],
  "targets": {
    "desktop": {
      "platform": "desktop",
      "backend": "kanata.backend.monogame",
      "hostProject": "Platforms/Desktop/MyGame.Desktop.csproj",
      "session": "local"
    }
  },
  "start": {
    "scene": "Content/Scenes/MainMenu.kscene"
  }
}
```

## Version fields

| Field | Meaning |
|---|---|
| `schemaVersion` | Version of the project file format. |
| `projectVersion` | Version of the game project. |
| `kanataVersion` | Kanata toolchain version used when the project was created. |

In the current monorepo, `kanataVersion` is shared with `Kanata.Core` through `Directory.Build.props`.

## Builder behavior

The builder validates the project before `generate`, `build`, and `play`.

Generated files are written under `Generated/Build` and can be deleted safely.

## Relation to component packages

The `.kanata` project file describes intent and target selection.

It should not directly store installed package payload paths.

Future package restore should resolve package identities and installed component artifacts into the lock file.
