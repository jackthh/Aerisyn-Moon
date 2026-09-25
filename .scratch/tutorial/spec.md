Status: needs-triage

# Spec: Tutorial package (`com.aerisyn.tutorial`)

Feature slug: `tutorial`  
Context: Tutorial (`com.aerisyn.tutorial`)  
Package: [`Packages/com.aerisyn.tutorial`](../../Packages/com.aerisyn.tutorial)

## Problem Statement

Games need a game-agnostic way to run Soft/Hard per-Step tutorials: Steps succeed on Reports, Hard Steps emit Gate signals for the game to lock input/UI, Hints stay presentational in the game. Authors are usually developers; Inspector SO authoring should follow once the core runner is solid.

## Solution (direction from grill; not implementation yet)

- Pure C# Core: Tutorial definition (ordered Steps with Enforcement) + runner
- Code-first builders for v1
- Game owns presentation and Hard locking (Hint + Gate signals)
- No Quests package dependency in v1
- SO authoring parked: [issues/02-scriptableobject-authoring.md](./issues/02-scriptableobject-authoring.md)

## Out of scope (v1 core)

- ScriptableObject authoring (explicit follow-up ticket 02)
- Dependency on Quests or Data Config
- Package-owned input freeze / mask UI

## Next steps

1. Finish grill frontier (persist, start/stop, SubStep, catch-up, concurrency)
2. Capture ADRs under `docs/adr/` when decisions land
3. `/to-spec` + `/to-tickets` for Core runner; keep 02 blocked until Core resolves

## Comments

- Workspace scaffold created earlier; domain grill in progress on `cursor/tutorial-domain-grill-1949`.
- 2026-09-25: Q6 = 2b (code-first first, SO later). Q7 = separate from Quests in v1.
