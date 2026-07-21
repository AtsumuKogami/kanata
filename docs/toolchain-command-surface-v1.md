# Toolchain Command Surface V1

Kanata toolchain operations are exposed through a shared command surface so CLI and GUI render the same operation results.

## Rule

```text
Kanata.Packaging -> Kanata.Toolchain -> CLI / GUI renderers
```

CLI must not implement package behavior separately from Hub. Hub must not implement package behavior separately from CLI.

## Current command layer

`Kanata.Toolchain` currently exposes structured package and tool operations:

- open package metadata;
- verify package;
- pack package staging directory;
- install package;
- list installed packages;
- inspect installed packages;
- list installed tool packages;
- inspect installed tool package.

Results are represented as typed payloads plus structured messages:

- `ToolchainCommandResult`;
- `ToolchainCommandResult<T>`;
- `ToolchainMessage`;
- `ToolchainMessageSeverity`.

## Renderer responsibilities

`Kanata.Cli` renders command results as terminal text and exit codes.

`Kanata.Hub` renders command results as GUI state: cards, lists, details panes, status text and operation log.

## Package summaries

The shared package summary includes:

- package id;
- version;
- display name;
- description;
- installables;
- payload files;
- block table;
- package length.

This allows GUI and CLI surfaces to inspect `.kpkg` packages without duplicating binary package parsing.

## Boundaries

The command surface does not yet provide:

- external tool command execution;
- project creation flow;
- build/play flow;
- terminal embedding;
- remote registry operations.

Those should be added as new toolchain commands before they are exposed through CLI or Hub.
