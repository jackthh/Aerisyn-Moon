Status: resolved

# 05: Concurrent Cue Choreography

**What to build:** Choreography can fire concurrent Cues (no Cue Done required) and mix sequential await with concurrent forget on the same Step, matching the UniTask await/Forget mental model for presentation intents.

**Blocked by:** 04 Sequential Cue Choreography

**Status:** resolved

- [x] Concurrent Cues on Step enter are all emitted without waiting for Cue Done
- [x] A Step can mix sequential and concurrent Cue groups in one Choreography
- [x] Report success remains independent of unfinished concurrent Cues
- [x] Tests cover concurrent and mixed Choreography at the Runner seam

## Comments

- Blocked by sequential Cue plumbing in 04.

## Seams (TDD)

- **Runner public seam only:** Start / Report / CueDone + Cue / StepCompleted / TutorialCompleted events.
- Definitions via `TutorialBuilder` + `ChoreographyDefinition` factories (Concurrent, Sequential, Mix).

## Answer

Concurrent / mixed Cue Choreography on the Runner seam under `Packages/com.aerisyn.tutorial/Runtime/Core/`:

- `CueGroupKind` + `CueGroup` → Sequential or Concurrent scheduling units
- `ChoreographyDefinition.Concurrent` / `Mix` (alongside existing `Sequential`)
- Runner walks groups in order: Concurrent Forget emits all immediately; Sequential awaits Cue Done before the next group
- Matching Report still abandons unfinished Choreography and completes the Step
- Fixtures: `Tests~/SoftRunner.Tests/ConcurrentCueTests.cs` (7 Cue tests; full suite via `dotnet test`)
