# Aerisyn Tutorial (`com.aerisyn.tutorial`)

Pure C# **Tutorial Runner** for Soft and Hard player tutorials. Authors build
Tutorial definitions in code; the game Starts / Stops, feeds Report and Cue Done, and
listens for Cue, Gate, and Completion signals. Presentation, rewards, and disk save
stay in the game (**Progress Snapshot** is the only save-shaped export).

Domain language: [`CONTEXT.md`](CONTEXT.md). Design notes: [`.scratch/tutorial/spec.md`](../../.scratch/tutorial/spec.md).

**Unity:** 2022.3+ · **Requires:** [Odin Inspector](https://odininspector.com/) (Sirenix) · **Runtime assembly:** `Aerisyn.Tutorial` · **Version:** `0.4.0`

## Requirements

| Dependency | Why |
|---|---|
| **Unity 2022.3+** | Minimum editor / player target |
| **Odin Inspector (Sirenix)** | Shared Aerisyn prerequisite so packages compile against `Sirenix.OdinInspector.Attributes`. |

Install Odin from the Unity Asset Store (or your usual Sirenix workflow) into the **consuming project**. Odin is not on UPM and is **not** shipped in this repository. Runner logic does not call Odin at runtime.

## Install

1. Install **Odin Inspector** into your Unity project.
2. Package Manager → **+ → Add package from git URL…**

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.tutorial
```

## Status (0.4.0)

Soft Runner + Hard Gate + sequential Cue Choreography (tickets 01, 03, 04):

- Code-first `TutorialBuilder` → Soft / Hard Steps with `ReportMatch` and optional Choreography
- `TutorialRunner`: Start / Stop / Report / CueDone, single-active enforcement
- Gate Started / Ended events for Hard Steps (Soft emits none)
- Sequential Cues: emit one at a time, await Cue Done; Report still completes unfinished Choreography
- Step and Tutorial Completion events
- Progress Snapshot export / apply on Start for mid-Tutorial resume
- Fixture tests: `Tests~/SoftRunner.Tests` (`dotnet test`)

Still out of this cut: concurrent Cue Choreography, SO authoring, Samples~.

## Quick start

```csharp
var runner = new TutorialRunner();
runner.Cue += (tutorialId, stepIndex, stepId, cueId) =>
{
    // Present, then: runner.CueDone(cueId);
};
runner.Gate += (tutorialId, stepIndex, stepId, phase) =>
{
    // Hard only: lock input on Started, unlock on Ended
};
runner.StepCompleted += (tutorialId, stepIndex, stepId) => { /* UI / analytics */ };
runner.TutorialCompleted += tutorialId => { /* grant rewards */ };

var tutorial = new TutorialBuilder("onboarding.sword")
    .SoftStep(
        "coach",
        ReportMatch.AnyParam(kind: 11),
        ChoreographyDefinition.Sequential("highlight.menu", "text.coach"))
    .HardStep("upgrade", new ReportMatch(kind: 10, param: 1))
    .Build();

runner.Start(tutorial);       // emits first Cue
runner.CueDone("highlight.menu");
runner.Report(11, 0);         // Soft Step completes even if later Cues unfinished
runner.Report(10, 1);         // Hard Step → Gate Ended + TutorialCompleted
```

## Layout

```text
Packages/com.aerisyn.tutorial/
  package.json
  README.md
  CHANGELOG.md
  CONTEXT.md
  Runtime/
    Aerisyn.Tutorial.asmdef
    TutorialPackage.cs
    Core/                 # Runner + definitions (pure C#)
  Tests~/SoftRunner.Tests/
```
