# Changelog

All notable changes to `com.aerisyn.tutorial` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.4.0] - 2026-09-25

### Added

- Soft Runner vertical slice: `TutorialRunner`, `TutorialBuilder` / Soft Steps, `ReportMatch`,
  Step + Tutorial Completion events, Progress Snapshot export/apply.
- Hard Gate signals: `TutorialBuilder.HardStep`, `GatePhase`, Runner `Gate` event
  (Started on Hard enter, Ended on Completion or Stop). Soft Steps emit no Gate.
- Sequential Cue Choreography: `ChoreographyDefinition.Sequential`, Runner `Cue` /
  `CueDone` (await between Cues). Matching Report completes the Step if Cues are unfinished.
- Concurrent / mixed Cue Choreography: `ChoreographyDefinition.Concurrent` (fire-and-forget, no Cue Done)
  and `Mix` of Sequential + Concurrent groups on one Step.
- Sample **Runner Smoke** (`Samples~/RunnerSmoke`): Soft/Hard + Cue demo with Console stubs;
  pure `RunnerSmokeDriver` + Samples~ MonoBehaviour facade (no Core MonoBehaviour API).
- Fixture suite `Tests~/SoftRunner.Tests` (Start/Stop, Report advance, unmatched ignore,
  single-active rejection, snapshot resume, Hard Gate enter/leave, Soft/Hard mix,
  sequential Cue await, concurrent fire-and-forget, mixed groups, Report-vs-Choreography independence,
  Runner Smoke scripted path).

### Changed

- Version cadence alignment with Aerisyn Moon **0.4.0** (Quests + Data Config Sheet).
- README / package marker updated for the Soft Runner API (no longer scaffold-only).

## [0.0.1] - 2026-09-24

### Added

- Package scaffold: `package.json`, Runtime assembly `Aerisyn.Tutorial`, `TutorialPackage` marker, README, CONTEXT stub.
