# Kanata build workflow v1

Kanata uses the project file as an intent description and resolves the concrete build for a selected target.

## Commands

```powershell
kanata create MyGame
cd MyGame
kanata validate
kanata generate
kanata build
kanata play
```

`create` creates a game project and immediately validates the generated `.kanata` file.

`validate` reads the project file and checks the project structure.

`generate` validates the project and writes generated build files into `Generated/Build`.

`build` validates, generates, and calls `dotnet build` for the selected target host project.

`play` validates, generates, and calls `dotnet run` for the selected target host project.

## Version source

The generated `kanataVersion` field is taken from the current Kanata toolchain version. In the monorepo this version is defined in `Directory.Build.props` and is shared by `Kanata.Core`, `Kanata.ProjectSystem`, and `Kanata.Build`.

Later, component versions will be resolved separately and written into a lock file, for example `Kanata.lock.json`.

## Component build modes

During engine development, components can be referenced as local projects.

For normal game development, resolved components should come from prebuilt packages or binaries, not from rebuilding the whole engine repository every time.

The intended modes are:

- `source` for engine development and local component development;
- `package` for installed Kanata SDK components;
- `binary` for already compiled third-party components.

The first builder implementation only creates a technical project build and does not yet resolve component packages.
