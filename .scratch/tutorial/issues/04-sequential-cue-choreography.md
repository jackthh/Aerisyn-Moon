Status: resolved

# 04: Sequential Cue Choreography

**What to build:** When a Step becomes active, its sequential Choreography emits Cues one at a time and waits for Cue Done before the next. Step success remains Report-driven: a matching Report completes the Step even if Choreography is unfinished. Cue ids stay opaque for the game to present.

**Blocked by:** 01 Soft Runner vertical slice

**Status:** resolved

- [x] Step definitions can include sequential Choreography of opaque Cue ids
- [x] On Step enter, Runner emits the first Cue and waits for Cue Done before emitting the next
- [x] Matching Report still completes the Step if sequential Cues are not finished
- [x] Tests cover await semantics and Report-vs-Choreography independence at the Runner seam

## Comments

- Parallelizable with 03 after 01. Concurrent Cues are ticket 05.

## Answer

Sequential Cue Choreography on the Runner seam under `Packages/com.aerisyn.tutorial/Runtime/Core/`:

- `ChoreographyDefinition.Sequential(params string[] cueIds)` → opaque Cue ids on `StepDefinition`
- `TutorialBuilder.SoftStep` / `HardStep` optional Choreography argument
- `TutorialRunner.Cue` event + `CueDone(cueId)` await between sequential Cues
- Matching Report abandons unfinished Choreography and still completes the Step
- Fixtures: `Tests~/SoftRunner.Tests/SequentialCueTests.cs` (6 Cue tests; full suite 18 via `dotnet test`)
