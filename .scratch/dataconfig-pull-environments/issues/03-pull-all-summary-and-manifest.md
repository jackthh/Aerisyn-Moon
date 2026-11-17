# 03: Pull All summary + Config Manifest

**What to build:** Pull All runs every Pull Config without stopping at the first failure, then prints one summary of passed and failed jobs. On overall success (or after each successful aggregate run per spec), write Config Manifest (environment, timestamp, per-feature sheet id/title/Drive revision fields, hash per Baked Asset). Completeness helper fails when required Config Types are missing or environment is wrong. Opt-in Production build gate helper allowed.

**Blocked by:** 02

**Status:** ready-for-agent

- [ ] Pull All continues across configs and returns a combined summary (no early silent return after first failure)
- [ ] Config Manifest asset written with env, time, per-feature metadata, per-asset hashes
- [ ] Drive metadata fetch uses existing `drive.readonly` + `supportsAllDrives=true` behind an injectible seam
- [ ] Completeness check compares manifest to a required Config Type list
- [ ] Optional helper can fail builds when active environment is not Production
- [ ] Pure tests for summary aggregation and completeness; no live Google
- [ ] CONTEXT.md updated (Environment Key, Feature Key, Environment Profile, Title Check, Config Manifest)
- [ ] Package version → 0.6.0; README + CHANGELOG document the feature

## Comments
