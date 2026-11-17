# 01: Data Config Settings core

**What to build:** One Data Config Settings asset holds Environment Keys (seed Dev, Test, Production), empty Feature Keys, active Environment Key, title-prefix overrides, Feature×Env spreadsheet bindings, Config Manifest path and required Config Types, Google auth fields, and the Pull All folder path. Pure tests lock key membership, spreadsheet id resolve from settings-shaped data, folder-scope Pull Config path filtering, and blank-path failure. Old Environment Profile / key-list types may still exist unused.

**Blocked by:** None (can start immediately). Prerequisite: 0.6.0 pull-environments merged on the integration line you build from.

**Status:** resolved

- [x] Data Config Settings exists as the single settings ScriptableObject with the fields above
- [x] First-create seeding: Environment Keys Dev/Test/Production; Feature Keys empty
- [x] Resolve(active env × Feature Key) returns spreadsheet id or fails clearly (pure tests)
- [x] Folder-scope rule: given folder path + asset paths, returns in-scope paths in stable order (pure tests)
- [x] Blank or whitespace Pull All folder path fails with a clear validation error (pure tests)
- [x] Inspectors/pickers for keys bind to the settings lists (no free-typed env/feature strings on bindings)

## Comments

## Answer

Added `DataConfigSettings` (`Packages/com.aerisyn.dataconfigsheet/Runtime/DataConfigSettings.cs`): one ScriptableObject with Environment Keys (seeded Dev/Test/Production via `KeyListRules`), empty Feature Keys, active key, bindings, title-prefix overrides, manifest path + required types, auth fields, and `PullAllFolderPath`. Fixed path const `DefaultAssetPath`. Methods: `ResolveSpreadsheetId(featureKey)` (wraps `SpreadsheetIdResolver`), `SelectPullConfigPaths(paths)` (wraps the new pure `Runtime/Parsing/PullConfigFolderScope.cs`: recursive, ordinal-sorted, blank folder throws "Pull All folder is blank..."), `ToBindings`, `ToTitlePrefixOverrides`.
- New `IDataConfigKeySource` (implemented by `DataConfigSettings` and the old `EnvironmentProfile`); the binding/title-prefix row pickers now cast the Inspector root to it, so keys bind to the settings lists.
- Tests: `Tests~/VerticalNest.Tests/PullConfigFolderScopeTests.cs`. Key membership / resolve seams were already pure and shape-agnostic (`KeyListRulesTests`, `SpreadsheetIdResolverTests`), so they cover settings data unchanged. Run with `DOTNET_ROLL_FORWARD=Major dotnet test` (only .NET 10 runtime installed locally).
- Not done (ticket 02+): auto-create of the asset at `DefaultAssetPath`, the window, Pull/PullConfig rewiring, removal of old types.
