# Kanata CLI

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
kanata generate
kanata build
kanata play
```

The default target is `desktop` and the default configuration is `Debug`.

Explicit target and configuration are supported:

```powershell
kanata generate desktop Release
kanata build desktop Release
kanata play desktop Debug
```

Project commands do not search parent directories. This keeps the command behavior predictable and makes the project root explicit.

## Engine component cache

`kanata engine build Debug` builds bundled source components into `.kanata/cache/components`.

Game builds call engine component preparation automatically:

```powershell
kanata build
```

To force component rebuilds during a project build:

```powershell
kanata build --force-engine
```
