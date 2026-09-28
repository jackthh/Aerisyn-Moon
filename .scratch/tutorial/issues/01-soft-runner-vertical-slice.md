Status: resolved

# 01: Soft Runner vertical slice

**What to build:** A developer can build a Soft-only Tutorial definition in code, Start it on a pure C# Runner (at most one active), Report matching facts to advance Steps, receive Step and Tutorial Completion signals, Stop to abandon, and export/apply a Progress Snapshot (Tutorial identity + Step index) so the game can persist or resume. Unmatched Reports are ignored. No Hard Gates and no Cues in this ticket.

**Blocked by:** None (can start immediately).

**Status:** resolved

- [x] Code-first builders (or equivalent) produce a Tutorial definition of ordered Soft Steps with Report match data
- [x] Runner Start/Stop works; a second Start while active is rejected clearly
- [x] Matching Report completes the active Soft Step, emits Step Completion, and advances; last Step emits Tutorial Completion
- [x] Unmatched Reports do not advance or corrupt the active Step
- [x] Progress Snapshot export/apply restores enough state to resume at a Step index under game save policy
- [x] Automated tests cover the above only through the Runner public seam (no UI / Play Mode required)
- [x] CONTEXT vocabulary used in public names (Tutorial, Step, Soft, Report, Runner, Completion, Progress Snapshot)

## Comments

- First Core tracer bullet from `/to-tickets` on the ready-for-agent Tutorial spec.

## Answer

Soft Runner seam shipped under `Packages/com.aerisyn.tutorial/Runtime/Core/`:

- `TutorialBuilder.SoftStep` → `TutorialDefinition` of Soft Steps with `ReportMatch`
- `TutorialRunner`: Start (optional Progress Snapshot resume), Stop, Report; throws on second Start while active
- Events: `StepCompleted`, `TutorialCompleted`
- `ExportSnapshot` / Start-with-snapshot for Tutorial id + Step index
- Fixtures: `Tests~/SoftRunner.Tests` (6 tests, `dotnet test`)
