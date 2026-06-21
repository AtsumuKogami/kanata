# Kanata project format v1

Kanata project files use the `.kanata` extension and JSONC syntax. JSONC means regular JSON with comments and trailing commas allowed.

## Minimal project file

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

`schemaVersion` is the version of the `.kanata` file format.

`projectVersion` is the version of the game project.

`kanataVersion` is the Kanata SDK version requested by the project.

## Validation

The first ProjectSystem implementation validates structure, required fields, referenced directories, source projects, target host projects, and the startup scene.

Run validation with:

```bash
kanata validate path/to/MyGame.kanata
```

or from a project directory:

```bash
kanata validate
```
