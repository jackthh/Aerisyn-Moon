# Changelog

All notable changes to `com.aerisyn.tutorial` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.4.0] - 2026-09-25

### Added

- Soft Runner vertical slice: `TutorialRunner`, `TutorialBuilder` / Soft Steps, `ReportMatch`,
  Step + Tutorial Completion events, Progress Snapshot export/apply.
- Fixture suite `Tests~/SoftRunner.Tests` (Start/Stop, Report advance, unmatched ignore,
  single-active rejection, snapshot resume).

### Changed

- Version cadence alignment with Aerisyn Moon **0.4.0** (Quests + Data Config Sheet).
- README / package marker updated for the Soft Runner API (no longer scaffold-only).

## [0.0.1] - 2026-09-24

### Added

- Package scaffold: `package.json`, Runtime assembly `Aerisyn.Tutorial`, `TutorialPackage` marker, README, CONTEXT stub.
