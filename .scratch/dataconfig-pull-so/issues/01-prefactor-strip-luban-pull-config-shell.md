# 01: Prefactor — Strip Luban; expose Pull Config shell

**What to build:** The package no longer teaches or runs Luban. User-facing Bake naming becomes Pull. A Pull Config holds spreadsheet id, OAuth settings, one shared output folder, and an explicit Config Type list. OAuth Sign In still works. Pull may stub or fail clearly until later tickets land. DevHost/sample material that teaches Luban as the product path is removed or demoted.

**Blocked by:** None (can start immediately).

**Status:** resolved

- [x] Luban CLI bake is gone from the product path (no runner/menus that invoke it as the workflow)
- [x] User-facing Bake → Pull naming matches CONTEXT vocabulary where it teaches the workflow
- [x] Pull Config exposes spreadsheet, OAuth, one output folder, and an explicit Config Type list (no per-tab output paths; no assembly auto-scan)
- [x] OAuth Sign In still works as the auth happy path
- [x] Newcomers reading README/DevHost are not guided into a Luban-first pipeline

## Answer

Prefactor complete on branch `cursor/prefactor-strip-luban-pull-config-2f39`:

- Removed `BakeConfig`, Luban runner, CSV export product path, and Bake menus.
- Added `PullConfig` (spreadsheet, OAuth, shared output folder, explicit `ConfigTypeAsset` type list), `ConfigTypeAsset` base, Pull menus/runner shell that validates then fails clearly.
- Retargeted OAuth Sign In/Out to `PullConfig`.
- Rewrote package + DevHost READMEs for Pull → SO; stripped DevHost Luban/Gen/GeneratedData/smoke and `com.code-philosophy.luban`.
