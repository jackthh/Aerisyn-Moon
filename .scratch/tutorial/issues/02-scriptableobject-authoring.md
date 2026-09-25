Status: ready-for-agent

# 02: ScriptableObject authoring for Tutorial definitions

**What to build:** Thin Authoring layer that projects ScriptableObject Tutorial/Step/Choreography data into the same Core Tutorial definition the code-first builders already produce. One-way SO → definition is enough. No second runtime model; no custom per-beat C# subclasses for list-shaped tutorials.

**Blocked by:** 01 Soft Runner vertical slice, 03 Hard Gate signals, 04 Sequential Cue Choreography, 05 Concurrent Cue Choreography

**Status:** ready-for-agent

- [ ] Authoring stays out of Core presentation/UI; SO maps Enforcement, Report match, Cue Choreography, and order 1:1 into Core definitions
- [ ] Same Runner accepts definitions from builders and from SO projection
- [ ] Samples or DevHost show both code-built and SO-built Tutorials on one Runner
- [ ] No Idle-Axolotl-style one-class-per-substep authoring requirement

## Comments

- 2026-09-25: Parked during `/grill-with-docs` as Q6 = 2b (code-first first).
- 2026-09-25: `/to-tickets` set concrete blockers 01, 03, 04, 05; Status ready-for-agent (grab only after those resolve).
