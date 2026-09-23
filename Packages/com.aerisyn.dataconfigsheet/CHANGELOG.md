# Changelog

All notable changes to `com.aerisyn.dataconfigsheet` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2026-09-23

First release. BakingSheet-based editor bake: Google Sheet → optional CSV cache → ScriptableObjects.

### Added

- Package scaffold (`package.json` 0.2.0, Runtime + Editor asmdefs).
- Dependency on upstream `com.cathei.bakingsheet` **v4.1.3** (git URL; not vendored).
- **Requires [Odin Inspector](https://odininspector.com/)** (repo-wide Aerisyn-Moon assumption). `BakeConfig` uses Odin drawers; `Aerisyn.DataConfigSheet` references `Sirenix.OdinInspector.Attributes`.
- `BakeConfig` asset: spreadsheet id(s), credential path, SO output path, optional CSV cache path, `SheetContainerFactory` reference.
- `SheetContainerFactory` abstract ScriptableObject so game-owned schema wires into the package menu.
- Editor menus: bake selected BakeConfig / bake all BakeConfigs (Google → `ScriptableObjectSheetExporter`).
- Sample `Samples~/BasicBake`: demo `Items` sheet, `DemoSheetContainer`, `DemoSheetContainerFactory`.
- README: auth (service account Viewer), one-way policy, install Git URL, BakingSheet + Odin prerequisites.

### Security

- Credential JSON paths are documented as gitignored; credentials must never be committed.
