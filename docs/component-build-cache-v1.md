# Kanata component build cache v1

Status: current implementation notes  
Scope: local source component build cache

## Purpose

Kanata can build bundled source components into versioned local artifacts.

The first cache implementation is local to the engine repository.

## Current layout

```text
.kanata/cache/components/
  <component-id>/
    <version>/
      <configuration>/
        lib/
          <target-framework>/
            <assembly>.dll
        kanata.component.json
        build-info.json
```

Examples:

```text
.kanata/cache/components/kanata.core/0.1.0/Debug/lib/net10.0/Kanata.Core.dll
.kanata/cache/components/kanata.backend.monogame/0.1.0/Debug/lib/net10.0/Kanata.Backend.MonoGame.dll
```

## Commands

```powershell
kanata engine build Debug
kanata engine build Release
kanata engine build Debug --force
```

Game target commands restore required engine components automatically:

```powershell
kanata restore desktop Debug
kanata generate desktop Debug
kanata build desktop Debug
kanata play desktop Debug
```

## Cache rule

For local source components, Kanata uses a source fingerprint.

The fingerprint includes:

- component id;
- resolved component version;
- build configuration;
- target framework;
- `.kcomponent` manifest;
- component `.csproj`;
- component `.cs` files;
- repository `Directory.Build.props`.

A source component is skipped when its cached artifact exists and its fingerprint did not change.

## Relation to package mode

For future downloaded binary or package components, the version and package content are expected to be immutable.

In package mode, Kanata should not rebuild the component from source. Instead it should use installed package metadata and payload files prepared by `.kpkg` installation.

The local source cache remains useful for Kanata engine development and local component development.
