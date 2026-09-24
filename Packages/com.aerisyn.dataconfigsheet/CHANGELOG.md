# Changelog

All notable changes to `com.aerisyn.dataconfigsheet` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.3.0] - 2026-09-24

**Pull → ScriptableObject** product path (ADR 0010). Replaces the abandoned Luban/BakingSheet spike documented historically in ADR 0009.

### Changed

- User-facing Bake naming → **Pull** (`PullConfig`, Pull menus).
- `PullConfig` holds spreadsheet id, OAuth settings, one shared output folder, and an explicit Config Type list (no per-tab paths; no assembly auto-scan).
- Config Types inspector list uses Odin `ValueDropdown` (stores `System.Type`; does not instantiate Config Types).
- README / DevHost / samples teach the Pull → SO path only.

### Added

- `ConfigTypeAsset` base for hand-written Config Types.
- Type-driven **Vertical Nest** parse over in-memory grids (Preamble, Field Headers, Column Aliases, `!!!` Ignore Marker, struct + primitive nests).
- Optional `[ColumnAlias]` / `[SheetTab]` attributes; Header Emitter menu copies a pasteable Header Row.
- Live Google Pull: OAuth → Sheets API cell grids → parse → Baked Assets (no CSV bridge; commas in cells are safe).
- Fixture tests under `Tests~/VerticalNest.Tests` (weapons → upgrades → bonus stats, aliases, Header Emitter, Pull report).
- `PullFromGrids` inject path: create missing Baked Assets or overwrite data in place (stable GUID).
- `PullReport`: success summary (created/updated assets) and failures with A1 sheet coordinates; menus log and show the report.
- Sample `Samples~/WeaponsPull`: nested Config Type template + Pull Config happy-path docs and acceptance checklist.

### Fixed

- Editor asmdef references `Sirenix.Serialization` under `overrideReferences` so `SerializedScriptableObject` Config Types compile against Editor code.

### Removed

- Luban CLI runner, tab→CSV export product path, BakeConfig Luban fields.
- DevHost Luban project, generated `cfg.*` / JSON, Luban smoke, and `com.code-philosophy.luban` DevHost dependency.
- Sample `Samples~/BasicBake` (BakingSheet-era demo).

### Notes

- OAuth Sign In remains the auth happy path.
- Abandoned Luban spike design notes: superseded ADR 0009 (not the shipping 0.3.0 product).

## [0.2.0] - 2026-09-23

First release. BakingSheet-based editor bake: Google Sheet → optional CSV cache → ScriptableObjects.

### Added

- Package scaffold (`package.json` 0.2.0, Runtime + Editor asmdefs).
- Dependency on upstream `com.cathei.bakingsheet` **v4.1.3** (git URL; not vendored).
- **Requires [Odin Inspector](https://odininspector.com/)** (repo-wide Aerisyn-Moon assumption).
- **OAuth browser sign-in** as default Google auth.
- **Editable single-file bake output** via game-owned `BakedSheetContainerAsset`.
- Sample `Samples~/BasicBake` + DevHost demo.
