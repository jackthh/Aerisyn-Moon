# Changelog

All notable changes to `com.aerisyn.dataconfigsheet` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.3.0] - Unreleased

Prefactor toward **Pull → ScriptableObject** (ADR 0010). Luban bake removed from the product path.

### Changed

- User-facing Bake naming → **Pull** (`PullConfig`, Pull menus).
- `PullConfig` holds spreadsheet id, OAuth settings, one shared output folder, and an explicit Config Type list (no per-tab paths; no assembly auto-scan).
- README / DevHost guide the Pull → SO path only; Luban teaching surfaces removed or demoted.

### Added

- `ConfigTypeAsset` base for hand-written Config Types.
- Pull runner/menu shell (validates config; fails clearly until Vertical Nest + asset write land).

### Removed

- Luban CLI runner, tab→CSV export product path, BakeConfig Luban fields.
- DevHost Luban project, generated `cfg.*` / JSON, Luban smoke, and `com.code-philosophy.luban` DevHost dependency.

### Notes

- OAuth Sign In remains the auth happy path.
- Historical 0.3.0 Luban spike notes remain under superseded ADR 0009.

## [0.2.0] - 2026-09-23

First release. BakingSheet-based editor bake: Google Sheet → optional CSV cache → ScriptableObjects.

### Added

- Package scaffold (`package.json` 0.2.0, Runtime + Editor asmdefs).
- Dependency on upstream `com.cathei.bakingsheet` **v4.1.3** (git URL; not vendored).
- **Requires [Odin Inspector](https://odininspector.com/)** (repo-wide Aerisyn-Moon assumption).
- **OAuth browser sign-in** as default Google auth.
- **Editable single-file bake output** via game-owned `BakedSheetContainerAsset`.
- Sample `Samples~/BasicBake` + DevHost demo.
