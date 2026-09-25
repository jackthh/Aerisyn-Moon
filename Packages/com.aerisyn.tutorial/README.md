# Aerisyn Tutorial (`com.aerisyn.tutorial`)

Pure C# **Tutorial Runner** for Soft (and later Hard) player tutorials. Authors build
Tutorial definitions in code; the game Starts / Stops, feeds Reports, and listens for
Completion signals. Presentation, rewards, and disk save stay in the game
(**Progress Snapshot** is the only save-shaped export).

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

Soft Runner vertical slice (ticket 01):

- Code-first `TutorialBuilder` → Soft Steps with `ReportMatch`
- `TutorialRunner`: Start / Stop / Report, single-active enforcement
- Step and Tutorial Completion events
- Progress Snapshot export / apply on Start for mid-Tutorial resume
- Fixture tests: `Tests~/SoftRunner.Tests` (`dotnet test`)

Still out of this cut: Hard Gate signals, Cue Choreography, SO authoring, Samples~.

## Quick start

```csharp
var runner = new TutorialRunner();
runner.StepCompleted += (tutorialId, stepIndex, stepId) => { /* UI / analytics */ };
runner.TutorialCompleted += tutorialId => { /* grant rewards */ };

var tutorial = new TutorialBuilder("onboarding.sword")
    .SoftStep("upgrade", new ReportMatch(kind: 10, param: 1))
    .SoftStep("equip", ReportMatch.AnyParam(kind: 11))
    .Build();

runner.Start(tutorial);
runner.Report(10, 1); // completes Soft Step 0, advances
runner.Report(11, 0); // completes last Soft Step → TutorialCompleted
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
