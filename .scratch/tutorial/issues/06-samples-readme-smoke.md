Status: resolved

# 06: Samples and README smoke

**What to build:** A minimal Samples~ and/or DevHost smoke path plus README that shows installing and driving the Runner seam (Start, Report, Cue Done stubs, Gate/Completion logging) without porting Idle-Axolotl UI. Consumers can copy a working Soft/Hard + Cue demo.

**Blocked by:** 01 Soft Runner vertical slice, 03 Hard Gate signals, 04 Sequential Cue Choreography, 05 Concurrent Cue Choreography

**Status:** resolved

- [x] Sample demonstrates Start → Cue/Gate/Completion signals → Report / Cue Done → Tutorial Completion with stub presentation
- [x] README documents install, CONTEXT vocabulary, and the Runner seam (no Quests dependency; SO authoring pointed at later)
- [x] Sample does not introduce a Core MonoBehaviour API; facades stay in Samples~ if needed
- [x] Package version/CHANGELOG reflect the first usable Runner if not already bumped by Core tickets

## Comments

- Last Core demo ticket before SO (02).

## Seams (TDD)

- **Runner public seam** via a pure C# sample driver (`RunnerSmokeDriver`): scripted Start → Cue Done → Report → Completion.
- MonoBehaviour facade is Samples~ only (not Core); not covered by EditMode fixtures.

## Answer

Runner Smoke sample under `Packages/com.aerisyn.tutorial/Samples~/RunnerSmoke/`:

- `RunnerSmokeDriver` (pure C#): builds Soft/Hard + Cue demo, `AttachLogging`, `RunScripted`
- `SampleRunnerSmoke` MonoBehaviour facade: Console stubs + context menus (Cue Done / Report / Stop / scripted smoke)
- Package Manager sample entry; README vocabulary + sample section; CHANGELOG note
- Fixture: `Tests~/SoftRunner.Tests/RunnerSmokeTests.cs` (full suite via `dotnet test`)
- Version remains `0.4.0` (already bumped by Core tickets)
