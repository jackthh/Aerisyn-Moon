# Runner Smoke sample

Minimal Soft / Hard Tutorial over a pure C# `TutorialRunner`, with stub Cue / Gate / Completion
logging. No Idle-Axolotl UI, no package-owned input freeze. Shows both **code-first** and
**ScriptableObject** authoring on one Runner.

## Try it

1. Import the sample from Package Manager (**Aerisyn Tutorial → Samples → Runner Smoke**).
2. Add `SampleRunnerSmoke` to any GameObject and enter Play mode.
3. Watch the Console for the first Soft Cue (`highlight.menu`).
4. Right-click the component header and use:
   - **Cue Done / highlight.menu**
   - **Report / Soft open menu** (advances to Hard Step; Gate Started + Concurrent Cues)
   - **Cue Done / highlight.button** (optional; Report still completes unfinished Cues)
   - **Report / Hard upgrade sword** → Tutorial Completion
5. Or use **Run scripted smoke (full path)** for Start → Cue Done → Reports → Completion in one click.
6. **Start from TutorialAsset (SO)** / **Run scripted smoke (code then SO on one Runner)** for the
   Authoring projection path. Optionally assign a `TutorialAsset` on the component; when empty the
   sample CreateInstance-populates the same demo Steps.

## What it shows

| Step | Enforcement | Report | Choreography |
|---|---|---|---|
| coach | Soft | kind 11 (any param) | Sequential: highlight.menu → text.coach |
| upgrade | Hard | kind 10, param 1 | Concurrent glow.slot + sfx.chime, then Sequential highlight.button |

Pure driver (no MonoBehaviour): `RunnerSmokeDriver.RunScripted()` and
`RunScriptedCodeThenAuthoring()` — covered by `Tests~/SoftRunner.Tests/RunnerSmokeTests.cs`.

Create assets via **Assets → Create → Aerisyn → Tutorial → Tutorial**, then assign on the sample
or call `TutorialAsset.Build()` and hand the definition to `TutorialRunner.Start`.

## Not for production

`SampleRunnerSmoke` is a logging facade only. Real games bind Cue ids to highlight/scale/hide,
apply Gate locks in their own input stack, and grant rewards on Tutorial Completion.
