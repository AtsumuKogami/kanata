# Kanata Hub UI slice v1

Status: first project-first UI implementation slice.
Scope: home-first shell and minimal user-facing page structure.

## Goal

This UI slice moves `Kanata.Hub` away from the technical package viewer checkpoint and toward the product information architecture:

    Home
    Templates
    Library
    Engine
    Settings
    Environment

The first visible layer must be minimal and user-friendly.

## Home

Home combines the previous Projects concept with first-run project creation.

Home contains:

- recent projects;
- create project action;
- open project action;
- browse templates action;
- basic new project setup.

The user should not see packages, hashes, package store internals, resolver dumps, or environment internals on the first screen.

## Basic new project setup

The basic setup captures intent, not package ids:

- project name;
- location;
- start mode;
- target platforms;
- game type;
- base modules.

Kanata will later translate this into a component recipe and resolver plan.

## Navigation

Primary navigation:

- Home;
- Templates;
- Library;
- Engine.

Secondary navigation:

- Settings;
- Environment.

Environment remains visible but quiet because it is maintenance, not the user's main task.

## Layering rule

Layer 1: user task.

    Create/open project, use template, browse components.

Layer 2: configuration.

    Component versions, platform variants, resolver plan, project locks.

Layer 3: technical details.

    Package ids, manifests, hashes, install paths, raw logs.

## Non-goals

This slice does not implement:

- real project persistence;
- actual project creation;
- template resolver integration;
- component installation;
- package explorer features;
- package store dump on the main screen.

Those should be wired through `Kanata.Toolchain` in later slices.
