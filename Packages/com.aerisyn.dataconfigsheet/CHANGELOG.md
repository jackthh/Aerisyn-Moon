# Changelog

All notable changes to `com.aerisyn.dataconfigsheet` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.5.0] - 2026-10-06

**Local Only** fields and **After Pull** (ADR 0013). Designers can keep fewer Source Sheet columns than the C# nest shape; games refill sheet-unowned derivatives on every Pull.

### Added

- `[LocalOnly]` on Config Type fields (scalars and primitive arrays/lists, any nest level): not part of the Source Sheet contract; not required for Header Row match; never bound from cells.
- Warn-and-ignore when the sheet still has a column matching a Local Only field name or Column Alias (warning text can include the alias); Pull continues and does not hard-fail for that alone.
- Header Emitter / Copy Header Row omit Local Only names and aliases; guidance states how many Local Only fields were omitted.
- Virtual `ConfigTypeAsset.OnAfterPull()` (default no-op): after every included tab parses successfully and before any Baked Asset create/save. Override to fill Local Only derivatives. A throw fails the whole Pull with no writes.
- README example: sheet-owned `turnSpeed` plus Local Only accel/decel filled in After Pull.

### Notes

- Pull stays **clean**: re-Pull rebuilds rows; prior Inspector Local Only values are not merged (refill in After Pull).
- **Ignore Marker** (`!!!`) remains for extra sheet note columns; Local Only is for extra fields on the Config Type / Baked Asset.
- Live Google Pull and inject `PullFromGrids` share the same After Pull timing.

## [0.4.0] - 2026-09-25

Repo-scale cadence with Quests and Tutorial. Pull UX and DevHost cleanup on top of the 0.3.0 Pull → ScriptableObject path (ADR 0010).

### Changed

- Pull Config Config Types are owned candidates with per-type **Include In Pull** (bare checkbox; new entries default included). Live and inject Pull only process included types; all-unticked fails clearly with no writes.
- New Pull Config defaults: empty OAuth client secrets, service-account path, and output folder; OAuth user token path remains under `UserSettings`.
- Live Pull and Sign In fail clearly when OAuth client secrets (or other required credential paths for the auth mode) are empty.
- Inspector **Pull** button on Pull Config; menu Selected/All Pull share one workflow helper into the runner.
- Docs and Weapons Pull sample use sample-local credential/output path examples (no `Assets/AerisynDataConfig` convention).
- Clean break: no silent migrate from the old bare Config Types type array; re-add types on existing Pull Configs by hand.

### Removed

- DevHost `Assets/AerisynDataConfig` scaffold (Credentials/Baked placeholders, unused Demo scene, local README).
- DevHost Weapons Pull sample `PullConfig.asset` (create locally; credentials and spreadsheet ids stay out of git).

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
