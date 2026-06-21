# Kanata build workflow v1

Kanata uses the project file as an intent description and resolves the concrete build for a selected target.

## Commands

```powershell
kanata create MyGame
cd MyGame
kanata validate
kanata restore
kanata generate
kanata build
kanata play
```

`create` creates a game project and immediately validates the generated `.kanata` file.

`validate` reads the project file and checks the project structure.

`restore` validates the project, resolves the required component graph, builds missing local component artifacts, and writes `Kanata.lock.json`.

`generate` runs restore and writes generated build files into `Generated/Build`.

`build` runs restore, generates build files, and calls `dotnet build` for the selected target host project.

`play` runs restore, generates build files, and calls `dotnet run` for the selected target host project.

## Version source

The generated `kanataVersion` field is taken from the current Kanata toolchain version. In the monorepo this version is defined in `Directory.Build.props` and is shared by `Kanata.Core`, `Kanata.ProjectSystem`, and `Kanata.Build`.

Component versions are resolved from `.kcomponent` manifests. The first bundled components use `$kanata`, which means they inherit the current Kanata toolchain version.

## Component build modes

During engine development, components are restored from the local Kanata source repository and built into the local component cache.

For normal game development, resolved components should eventually come from prebuilt packages or binaries, not from rebuilding the whole engine repository every time.

The intended modes are:

- `source` for engine development and local component development;
- `package` for installed Kanata SDK components;
- `binary` for already compiled third-party components.

The first resolver implementation supports bundled source components and records their resolved artifacts in `Kanata.lock.json`.
