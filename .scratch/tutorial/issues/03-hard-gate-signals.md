Status: ready-for-agent

# 03: Hard Gate signals

**What to build:** Hard Steps emit Gate signals so the game can lock and unlock input/UI. Soft Steps still do not require Gate locks. Leaving a Hard Step (Completion or Stop) ends the Gate. Soft/Hard can mix in one Tutorial.

**Blocked by:** 01 Soft Runner vertical slice

**Status:** ready-for-agent

- [ ] Entering a Hard Step emits Gate started; leaving it (Step Completion or Stop) emits Gate ended
- [ ] Soft Steps do not emit Gate lock semantics
- [ ] A Tutorial can mix Soft and Hard Steps; Gates only track the active Hard Step
- [ ] Tests assert Gate behavior only through the Runner seam

## Comments

- Parallelizable with 04 after 01.
