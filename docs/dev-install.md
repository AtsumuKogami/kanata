# Kanata development install

Kanata can be tested as an installed command line tool instead of being launched from the repository with `dotnet run`.

## Install development tool

From the repository root:

```powershell
.\scripts\install-kanata-dev.ps1
```

When PowerShell script execution is blocked, run it once with bypass:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\install-kanata-dev.ps1
```

The script:

1. builds the repository;
2. packs `Kanata.Build` as a .NET tool;
3. removes the previously installed global `Kanata.Build` tool;
4. clears the matching NuGet package cache entry;
5. installs the new tool as `kanata`;
6. stores the source repository root in `~/.kanata/dev-install.json`;
7. also writes `KANATA_REPOSITORY_ROOT` for compatibility;
8. runs smoke tests through the installed `kanata` command.

The config file is used by installed dev builds when a command such as `kanata build` is executed from a game project outside the Kanata repository.

## Fast install without smoke tests

```powershell
.\scripts\install-kanata-dev.ps1 -SkipSmokeTest
```

## Test installed tool again

```powershell
.\scripts\test-kanata-installed.ps1
```

## Uninstall development tool

```powershell
.\scripts\uninstall-kanata-dev.ps1
```

## Development workflow

Use the repository for implementation:

```powershell
dotnet build
```

Use the installed tool for end-to-end checks:

```powershell
.\scripts\install-kanata-dev.ps1
kanata create MyGame
cd MyGame
kanata validate
kanata build
kanata play
```

This gives two scenarios:

- repository mode for developing Kanata itself;
- installed tool mode for checking how users will run Kanata.
