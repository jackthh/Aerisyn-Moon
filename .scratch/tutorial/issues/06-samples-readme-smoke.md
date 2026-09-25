Status: ready-for-agent

# 06: Samples and README smoke

**What to build:** A minimal Samples~ and/or DevHost smoke path plus README that shows installing and driving the Runner seam (Start, Report, Cue Done stubs, Gate/Completion logging) without porting Idle-Axolotl UI. Consumers can copy a working Soft/Hard + Cue demo.

**Blocked by:** 01 Soft Runner vertical slice, 03 Hard Gate signals, 04 Sequential Cue Choreography, 05 Concurrent Cue Choreography

**Status:** ready-for-agent

- [ ] Sample demonstrates Start → Cue/Gate/Completion signals → Report / Cue Done → Tutorial Completion with stub presentation
- [ ] README documents install, CONTEXT vocabulary, and the Runner seam (no Quests dependency; SO authoring pointed at later)
- [ ] Sample does not introduce a Core MonoBehaviour API; facades stay in Samples~ if needed
- [ ] Package version/CHANGELOG reflect the first usable Runner if not already bumped by Core tickets

## Comments

- Last Core demo ticket before SO (02).
