# Changelog

All notable changes to `com.aerisyn.dataconfigsheet` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.3.0] - 2026-09-24

Hard cut from BakingSheet to **Google → CSV → Luban**.

### Changed

- Converter stack is Luban CLI (`cs-simple-json` + JSON). BakingSheet dependency removed.
- `BakeConfig` now holds spreadsheet id, tab→CSV exports, Luban project/dll paths, and code/data output dirs.
- Runtime final is Luban `Tables` + JSON (editable `BakedSheetContainerAsset` paused; ADR 0008).
- Schema: Google `##var` / `##type` and/or Luban XML (`readSchemaFromFile`); no `SheetContainerFactory`.

### Added

- Google Sheets API CSV exporter (OAuth / service account).
- Editor bake runner invokes `dotnet Luban.dll` with `DOTNET_ROLL_FORWARD=Major`.
- ADR 0009; package `CONTEXT.md`; repo `CONTEXT-MAP.md`.

### Removed

- BakingSheet importers/exporters, `SheetContainerFactory`, `BakedSheetContainerAsset`, BasicBake sample.

## [0.2.0] - 2026-09-23

First release. BakingSheet-based editor bake: Google Sheet → optional CSV cache → ScriptableObjects.

### Added

- Package scaffold (`package.json` 0.2.0, Runtime + Editor asmdefs).
- Dependency on upstream `com.cathei.bakingsheet` **v4.1.3** (git URL; not vendored).
- **Requires [Odin Inspector](https://odininspector.com/)** (repo-wide Aerisyn-Moon assumption).
- **OAuth browser sign-in** as default Google auth.
- **Editable single-file bake output** via game-owned `BakedSheetContainerAsset`.
- Sample `Samples~/BasicBake` + DevHost demo.
