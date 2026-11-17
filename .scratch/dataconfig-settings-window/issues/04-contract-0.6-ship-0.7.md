# 04: Contract 0.6.0 multi-asset model + ship 0.7.0

**What to build:** Remove the separate Environment Key list, Feature Key list, and Environment Profile CreateAssetMenu (and any multi-profile Pull All rule left from 0.6.0). Package version 0.7.0 with README, CHANGELOG, and samples teaching Data Config Settings and the window. Hard break: recreate in the window; no auto-migrator.

**Blocked by:** 03

**Status:** resolved

- [x] Environment Key list / Feature Key list / Environment Profile are no longer createable product assets
- [x] Multi-profile Pull All rule removed or moot with no remaining callers
- [x] README documents the window, fixed settings path, folder-scoped Pull All, and shared auth
- [x] CHANGELOG 0.7.0 states the hard break and “recreate in the window”
- [x] Samples no longer teach Environment Profile or separate key-list assets
- [x] package version is 0.7.0; CONTEXT language already matches (verify, fix gaps only)

## Comments

## Answer

Contracted to the single-settings model for 0.7.0.
- Deleted `EnvironmentProfile`, `EnvironmentKeyList`, `FeatureKeyList`, `IDataConfigKeySource` (only one implementer left; the binding row Odin pickers now cast the Inspector root to `DataConfigSettings`) and `PullAllProfileRule`, with their `.meta` files. Removed the 3 `PullAllProfileRule` tests; the other 148 tests pass (`DOTNET_ROLL_FORWARD=Major dotnet test`).
- Repo-wide grep shows no remaining references in code, asmdefs, samples or docs except CONTEXT `_Avoid_` lines and CHANGELOG history. Comments retargeted to Data Config Settings.
- package.json 0.7.0; README (window, fixed path, folder-scoped Pull All, shared auth, upgrade note); CHANGELOG 0.7.0 hard break "recreate in the window", no migrator; Weapons Pull sample README uses the window + fixed settings path.
- CONTEXT.md verified, no changes. Old package SPEC.md/TICKETS.md have no environment/profile content, left alone.
- Not compiled in Unity; hand-reviewed.
