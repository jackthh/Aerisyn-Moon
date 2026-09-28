Status: resolved

# 03: Hard Gate signals

**What to build:** Hard Steps emit Gate signals so the game can lock and unlock input/UI. Soft Steps still do not require Gate locks. Leaving a Hard Step (Completion or Stop) ends the Gate. Soft/Hard can mix in one Tutorial.

**Blocked by:** 01 Soft Runner vertical slice

**Status:** resolved

- [x] Entering a Hard Step emits Gate started; leaving it (Step Completion or Stop) emits Gate ended
- [x] Soft Steps do not emit Gate lock semantics
- [x] A Tutorial can mix Soft and Hard Steps; Gates only track the active Hard Step
- [x] Tests assert Gate behavior only through the Runner seam

## Comments

- Parallelizable with 04 after 01.

## Answer

Hard Gate signals on the Soft Runner seam under `Packages/com.aerisyn.tutorial/Runtime/Core/`:

- `TutorialBuilder.HardStep` → Hard Enforcement Steps with the same `ReportMatch` shape
- `GatePhase` (`Started` / `Ended`) + `TutorialRunner.Gate` event
- Enter Hard (Start or advance) → Gate Started; leave (Report Completion or Stop) → Gate Ended
- Soft Steps emit no Gate; Soft/Hard mix only tracks the active Hard Step
- Fixtures: `Tests~/SoftRunner.Tests/HardGateTests.cs` (6 Gate tests; full suite 12 via `dotnet test`)
