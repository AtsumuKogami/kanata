# Kanata Project Format v1

Kanata game projects are described by a `.kanata` file. The file uses JSONC: normal JSON syntax plus comments and trailing commas.

## Minimal example

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

## Required fields

- `format` must be `kanata.project`.
- `schemaVersion` identifies the project file schema. Version `1` is the initial format.
- `id` is a stable lowercase technical identifier.
- `name` is the human-readable project name.
- `kanataVersion` records the SDK version expected by the project.
- `paths` declares standard project folders.
- `source` declares C# source projects used by the generated build.
- `features` declares high-level project features.
- `targets` declares build targets.
- `start.scene` points to the startup scene.

## Current CLI commands

```bash
kanata new game MyGame
kanata validate MyGame/MyGame.kanata
kanata build desktop Debug MyGame/MyGame.kanata
```

During build, Kanata generates an MSBuild props file under `Generated/Build` and passes it to the target host project through the `KanataGeneratedProps` property.
