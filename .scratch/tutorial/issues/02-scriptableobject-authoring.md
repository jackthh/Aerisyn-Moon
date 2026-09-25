Status: needs-triage

# 02: ScriptableObject authoring for Tutorial definitions

**What to build:** Thin Authoring layer (`TutorialAsset` / step list SO + Odin drawers as needed) that projects into the same Core `TutorialDefinition` the code-first builders already produce. Round-trip or one-way SO → definition is fine; do not invent a second runtime model.

**Why later:** Grill chose **2b**. Prove pure C# definition + runner + Reports + Gate signals first. SO is flexibility and Inspector iteration for the same data, not a second system.

**Blocked by:** Core Tutorial definition + runner tickets (numbers TBD after `/to-tickets`). Do not start until those are `resolved` and the definition shape has stopped thrashing.

**Status:** needs-triage

- [ ] Authoring asmdef (or package Authoring folder) without pulling presentation/UI into Core
- [ ] SO fields map 1:1 to Core definition (Enforcement, Report match, Hint id, order)
- [ ] Samples or DevHost show: build via code *and* load via SO into the same runner
- [ ] No custom per-step C# subclasses required for list-shaped tutorials

## Comments

- 2026-09-25: Parked during `/grill-with-docs`. User chose code-first v1, SO next once core works.
