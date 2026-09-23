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
- **OAuth browser sign-in** as default Google auth (`GoogleAuthMode.OAuthUser`): Sign In / Sign Out menus, `authorized_user` token under `UserSettings/` for BakingSheet `FromJson`. Email-shared private sheets (Idle-like). Service account remains optional for CI.
- **Editable single-file bake output** via game-owned `BakedSheetContainerAsset` (serializable lists). BakingSheet `ScriptableObjectSheetExporter` is not used (it produced read-only row sub-assets).
- `BakeConfig` asset: spreadsheet id(s), auth mode + paths, optional CSV cache, `SheetContainerFactory` + `Baked Output` references.
- `SheetContainerFactory` / `BakedSheetContainerAsset` so game-owned schema and SO shape wire into the package menu.
- Editor menus: Sign In / Sign Out / bake selected / bake all.
- Sample `Samples~/BasicBake` + DevHost demo: factory + `*BakedDataAsset` with editable Items list.
- README: OAuth setup, editable SO policy, BakingSheet + Odin prerequisites.

### Security

- Credential JSON paths are documented as gitignored; credentials must never be committed.
