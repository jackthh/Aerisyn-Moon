Status: ready-for-agent

# 04: Sequential Cue Choreography

**What to build:** When a Step becomes active, its sequential Choreography emits Cues one at a time and waits for Cue Done before the next. Step success remains Report-driven: a matching Report completes the Step even if Choreography is unfinished. Cue ids stay opaque for the game to present.

**Blocked by:** 01 Soft Runner vertical slice

**Status:** ready-for-agent

- [ ] Step definitions can include sequential Choreography of opaque Cue ids
- [ ] On Step enter, Runner emits the first Cue and waits for Cue Done before emitting the next
- [ ] Matching Report still completes the Step if sequential Cues are not finished
- [ ] Tests cover await semantics and Report-vs-Choreography independence at the Runner seam

## Comments

- Parallelizable with 03 after 01. Concurrent Cues are ticket 05.
