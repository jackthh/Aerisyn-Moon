Status: ready-for-agent

# 05: Concurrent Cue Choreography

**What to build:** Choreography can fire concurrent Cues (no Cue Done required) and mix sequential await with concurrent forget on the same Step, matching the UniTask await/Forget mental model for presentation intents.

**Blocked by:** 04 Sequential Cue Choreography

**Status:** ready-for-agent

- [ ] Concurrent Cues on Step enter are all emitted without waiting for Cue Done
- [ ] A Step can mix sequential and concurrent Cue groups in one Choreography
- [ ] Report success remains independent of unfinished concurrent Cues
- [ ] Tests cover concurrent and mixed Choreography at the Runner seam

## Comments

- Blocked by sequential Cue plumbing in 04.
