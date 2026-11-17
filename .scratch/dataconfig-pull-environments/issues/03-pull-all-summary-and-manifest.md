# 03: Pull All summary + Config Manifest

**What to build:** Pull All runs every Pull Config without stopping at the first failure, then prints one summary of passed and failed jobs. On overall success (or after each successful aggregate run per spec), write Config Manifest (environment, timestamp, per-feature sheet id/title/Drive revision fields, hash per Baked Asset). Completeness helper fails when required Config Types are missing or environment is wrong. Opt-in Production build gate helper allowed.

**Blocked by:** 02

**Status:** claimed

- [x] Pull All continues across configs and returns a combined summary (no early silent return after first failure)
- [x] Config Manifest asset written with env, time, per-feature metadata, per-asset hashes
- [x] Drive metadata fetch uses existing `drive.readonly` + `supportsAllDrives=true` behind an injectible seam
- [x] Completeness check compares manifest to a required Config Type list
- [x] Optional helper can fail builds when active environment is not Production
- [x] Pure tests for summary aggregation and completeness; no live Google
- [x] CONTEXT.md updated (Environment Key, Feature Key, Environment Profile, Title Check, Config Manifest)
- [x] Package version → 0.6.0; README + CHANGELOG document the feature

## Comments

- Pure seams in `Runtime/Parsing`: `PullAllRunner` + `PullAllSummary` (run all, exceptions become failed results, empty run is not success), `ConfigManifestSnapshot.FromJobs`, `ManifestCompleteness.Check`, `SheetRevisionLookup.TryFetchAsync` (Drive fetch seam, failure → empty fields), `ManifestContentHash`, `ProductionBuildGate`. 29 new NUnit tests; suite 122/122.
- Editor wiring: `DataConfigPullWorkflow.PullAllWithUiAsync` (stable path order, one summary dialog, manifest per Environment Profile with a manifest path, only when every job passed, then completeness), `DataConfigPullRunner.PullWithSourceAsync`, `GoogleDriveRevisionFetcher` (`files.get`, `supportsAllDrives=true`), `ConfigManifestStore`, `DataConfigBuildGate.Require` (opt-in; package registers nothing).
- Environment Profile gained `Manifest Asset Path` and `Required Config Types`.
- Not done: manifest after a single Pull (spec says optional).
- Editor/Runtime Unity code was not compiled in a Unity Editor here (no Unity on this host); only `Runtime/Parsing` compiles under `dotnet test`. Open Unity once and Pull All against a real profile before release.
