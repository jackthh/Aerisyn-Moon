# 02: Pull / Pull All read settings

**What to build:** Pull Config keeps Feature Key, output folder, Config Types, Include In Pull, and Skip Title Check only. It no longer stores an Environment Profile reference or auth fields. Single Pull and Pull All load Data Config Settings for spreadsheet id and Google auth. Pull All discovers Pull Configs only under the settings folder path (recursive); a blank path fails with no whole-project fallback. Title Check, Pull All summary (continue on failure), and Config Manifest completeness keep 0.6.0 behavior against settings data.

**Blocked by:** 01

**Status:** resolved

- [x] Pull Config has no Environment Profile reference and no auth fields
- [x] Feature Key picker reads Feature Keys from Data Config Settings
- [x] Single Pull resolves spreadsheet id and credentials from Data Config Settings; missing settings fails clearly
- [x] Pull All uses folder-scoped discovery only; blank path fails; empty folder is not success
- [x] Title Check / Skip Title Check still work with prefixes from Data Config Settings
- [x] Pull All summary and manifest completeness (including mixed-run missing types) still pass pure/regression tests
- [x] Build gate helpers accept Data Config Settings (Production vs completeness stay separate)

## Comments

## Answer

Pull and Pull All now read `DataConfigSettings` (loaded via new `Editor/DataConfigSettingsLoader.cs` from `DefaultAssetPath`; missing asset throws a clear `InvalidOperationException`).
- `Runtime/PullConfig.cs`: Environment Profile reference, auth fields and Sign In/Out buttons removed; Feature Key picker reads `DataConfigSettings.FeatureKeys` (editor-only `AssetDatabase` load at the fixed path). Pull button stays.
- `Editor/DataConfigPullRunner.cs`: `PullAsync` / `PullWithSourceAsync` have `(config)` (loads settings) and `(config, settings)` overloads; spreadsheet id, active key, title prefixes and auth come from settings. `ValidateGoogle(settings)`.
- `Editor/GoogleOAuthSession.cs`: all APIs take `DataConfigSettings` (`SignInAsync`, `SignOut`, `IsSignedIn`, `CreateSheetsServiceAsync`, `CreateDriveServiceAsync`).
- `Editor/DataConfigPullConfigDiscovery.cs`: `FindPaths` / `Find(settings)` wrap `SelectPullConfigPaths` plus new pure `PullConfigFolderScope.RequireAny` (empty folder throws "No Pull Configs found under ...").
- `Editor/DataConfigPullWorkflow.cs`: `PullAllWithUiAsync()` loads settings, discovers by folder, no profile rule (`PullAllProfileRule` is now unused, removal is ticket 04). Setup failures return a failed `PullAllSummary` carrying the message (`Format` now lists findings on an empty run). `SignInWithUiAsync()` / `SignOutWithUi()` take no args and use settings. Manifest check uses `ConfigManifestStore.CheckSettings(settings)`.
- `Editor/DataConfigBuildGate.cs`: `Require` / `RequireCompleteManifest` take `DataConfigSettings`.
- Menu Sign In / Sign Out / Pull All keep working (no selection needed for auth).
- Pure messages retargeted from "Environment Profile" to "Data Config Settings". Tests added: `RequireAny` (PullConfigFolderScopeTests), empty-run findings (PullAllSummaryTests), resolver message. 146 pass.
- Old `EnvironmentProfile` / key-list types remain, unused by Pull.
- For ticket 03: window calls `DataConfigPullWorkflow.SignInWithUiAsync()`, `SignOutWithUi()`, `PullAllWithUiAsync()`; sign-in status via `GoogleOAuthSession.IsSignedIn(settings)`; create the asset at `DataConfigSettings.DefaultAssetPath` before those run. `PullConfig.RunEditorSignIn*` statics no longer exist.
