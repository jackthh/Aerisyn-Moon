Status: resolved

# 02: ScriptableObject authoring for Tutorial definitions

**What to build:** Thin Authoring layer that projects ScriptableObject Tutorial/Step/Choreography data into the same Core Tutorial definition the code-first builders already produce. One-way SO → definition is enough. No second runtime model; no custom per-beat C# subclasses for list-shaped tutorials.

**Blocked by:** 01 Soft Runner vertical slice, 03 Hard Gate signals, 04 Sequential Cue Choreography, 05 Concurrent Cue Choreography

**Status:** resolved

- [x] Authoring stays out of Core presentation/UI; SO maps Enforcement, Report match, Cue Choreography, and order 1:1 into Core definitions
- [x] Same Runner accepts definitions from builders and from SO projection
- [x] Samples or DevHost show both code-built and SO-built Tutorials on one Runner
- [x] No Idle-Axolotl-style one-class-per-substep authoring requirement

## Seams (TDD)

- **Authoring projection seam:** serializable Step / Cue-group authoring data → same `TutorialDefinition` shape as `TutorialBuilder` (Enforcement, Report match, Choreography groups, order).
- **Runner public seam:** Start / Report / CueDone / Completion with a definition produced by projection (no second runtime model).
- **Sample seam:** one Runner runs a code-built Tutorial then an SO-projected Tutorial (Samples~ facade may CreateInstance when no asset is assigned).

## Answer

Thin ScriptableObject authoring under `Packages/com.aerisyn.tutorial/Runtime/Authoring/`:

- `AuthoredCueGroup` / `AuthoredStep` — list-shaped serializable data (no per-beat subclasses)
- `TutorialAuthoringProjection.Project` — one-way Authoring → Core `TutorialDefinition`
- `TutorialAsset` — Odin SO (`Assets → Create → Aerisyn → Tutorial → Tutorial`) calling the same projection
- Sample: `RunnerSmokeDriver` builds both code and authored demos; `RunScriptedCodeThenAuthoring` + Sample context menus on one Runner
- Fixtures: `AuthoringProjectionTests` + extended `RunnerSmokeTests` (full suite via `dotnet test`)

## Comments

- 2026-09-25: Parked during `/grill-with-docs` as Q6 = 2b (code-first first).
- 2026-09-25: `/to-tickets` set concrete blockers 01, 03, 04, 05; Status ready-for-agent (grab only after those resolve).
- 2026-09-26: Claimed for ScriptableObject authoring implementation.
- 2026-09-26: Resolved — SO authoring projects into Core definitions; sample shows code + SO on one Runner.
