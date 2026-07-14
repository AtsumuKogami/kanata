# Kanata component manifest family v1

Status: draft  
Scope: source manifests, packaged descriptors, installable metadata

## Purpose

Kanata components are described by a small family of manifest formats.

The base contract is `kcomponent`. Specialized component types extend it:

```text
kcomponent
  ktool
  kbackend
  kruntime
  keditor
  kplugin
```

This is a schema-level inheritance model. It does not require C# inheritance.

## Base component contract

Every component-like installable has common metadata that can be read before installation.

Common fields:

| Field | Required | Description |
|---|---:|---|
| `format` | yes | Manifest format identifier. |
| `schemaVersion` | yes | Manifest schema version. |
| `id` | yes | Stable component id. |
| `version` | yes | Component version. |
| `kind` | yes | Component kind: `runtime`, `backend`, `tool`, `editor`, `plugin`. |
| `description` | no | Human-readable description. |
| `dependencies` | no | Component dependencies. |
| `provides` | no | Capabilities provided by the component. |
| `compatibility` | no | Kanata/platform/architecture compatibility. |
| `gameParticipation` | no | Whether the component participates in game build/runtime graphs. |

Current source `.kcomponent` manifests may also contain source-build fields:

| Field | Description |
|---|---|
| `project` | Path to source `.csproj`. |
| `assemblyName` | Assembly name. |
| `targetFramework` | Target framework moniker. |

These fields are source-oriented. Packaged descriptors may normalize or move them into source metadata.

## Specialized manifests

Specialized manifests extend the base component contract with kind-specific fields.

### `ktool`

A tool component is installed into the Kanata development environment.

Tools:

- are not game dependencies;
- do not participate in game runtime;
- do not participate in game build output;
- may provide CLI commands;
- may provide capabilities;
- may have official and user implementations.

Example:

```json
{
  "format": "kanata.tool",
  "schemaVersion": 1,
  "id": "kanata.engineer",
  "version": "0.1.0",
  "kind": "tool",
  "description": "Official engineering tool for Kanata.",
  "provides": [
    "kanata.engineering"
  ],
  "commands": [
    {
      "name": "kanata-engineer",
      "description": "Runs Kanata Engineer.",
      "entryPoint": {
        "kind": "dotnet-assembly",
        "path": "tools/kanata.engineer/Kanata.Engineer.dll"
      },
      "launchMode": "process"
    }
  ],
  "compatibility": {
    "kanataToolVersion": "[0.1.0,)",
    "platforms": ["windows"],
    "architectures": ["x64", "arm64"]
  },
  "gameParticipation": {
    "build": false,
    "runtime": false
  },
  "artifacts": [
    {
      "role": "tool-assembly",
      "path": "tools/kanata.engineer/Kanata.Engineer.dll"
    }
  ]
}
```

### `kbackend`

A backend component connects Kanata to a rendering or platform backend.

Example:

```json
{
  "format": "kanata.backend",
  "schemaVersion": 1,
  "id": "kanata.backend.monogame",
  "version": "0.1.0",
  "kind": "backend",
  "description": "MonoGame backend for Kanata.",
  "provides": [
    "kanata.backend.desktop"
  ],
  "backend": {
    "backendId": "monogame",
    "entryAssembly": "lib/net8.0/Kanata.Backend.MonoGame.dll",
    "entryType": "Kanata.Backend.MonoGame.MonoGameBackendModule",
    "supportedTargets": ["desktop"]
  },
  "compatibility": {
    "kanataToolVersion": "[0.1.0,)",
    "platforms": ["windows", "linux", "macos"],
    "architectures": ["x64", "arm64"]
  },
  "gameParticipation": {
    "build": true,
    "runtime": true
  },
  "artifacts": [
    {
      "role": "runtime-assembly",
      "path": "lib/net8.0/Kanata.Backend.MonoGame.dll"
    }
  ]
}
```

## Command vs capability

Command and capability are separate concepts.

| Concept | Stored in package | Meaning |
|---|---:|---|
| Command | yes | How to launch a tool. |
| Capability | yes | Replaceable role provided by a component. |
| Capability binding | no | Selected active provider in installed environment. |

Example:

```text
Component: fox.engineer
Kind: tool
Command: fox-engineer
Provides: kanata.engineering
```

The selected provider mapping is not stored in the package:

```text
kanata.engineering -> fox.engineer
```

This is future installed environment, resolver, or user configuration state.

## Game participation

Tool components must explicitly stay out of the game graph.

Recommended defaults:

| Kind | Game build graph | Game runtime graph | Dev environment |
|---|---:|---:|---:|
| `runtime` | yes | yes | optional |
| `backend` | yes | yes | optional |
| `tool` | no | no | yes |
| `editor` | no | no | yes |
| `plugin` | depends on plugin type | depends on plugin type | depends on plugin type |

## Source manifest vs packaged descriptor

A source manifest is written by a developer.

A packaged descriptor is written by `kanata package pack`.

During packaging:

```text
source manifest
  -> validate
  -> normalize
  -> packaged installable descriptor block
```

The packaged descriptor is the canonical metadata inside `.kpkg`.

Rules:

- source manifests may contain source-build paths;
- packaged descriptors must reference payload paths;
- packaged descriptors must not depend on the original source tree;
- packaged descriptors are not regular payload files;
- packaged descriptors are stored as descriptor blocks inside `.kpkg`.
