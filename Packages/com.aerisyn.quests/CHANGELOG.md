# Changelog

All notable changes to `com.aerisyn.quests` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-09-22

First usable release. Replaces the earlier unreleased scaffold at the same version.

### Added

- `QuestTracker`: pure C# engine. Open/close Boards, `Report(kind, param, value)`, `TryClaim`, `Export`, read views.
- `QuestDefinition` with `Objective` (opaque int kind + param, optional match-any), `Accumulation` (Sum, HighWater, Flag), ascending Step thresholds, and `ClaimPolicy` (OncePerStep, RepeatWithReset).
- `BoardId` / `QuestId` (board-scoped identity; duplicate local ids rejected on open).
- `ProgressSnapshot`: the only durable shape. No file or PlayerPrefs I/O in Runtime.
- `QuestView`: derived Quest and Step states for UI.
- Events: `ProgressChanged`, `StepBecameClaimable`, `StepClaimed`, `BoardChanged`.
- Authoring ScriptableObjects `QuestAsset` and `QuestBoardAsset` with validation, plus Editor inspectors that surface errors.
- Sample **Basic Board** with a temporary JSON store (sample only).

### Notes

- Progress values are `long`. Wider numeric types are deferred.
- Reward contents, descriptions, icons, navigation, and analytics stay in the game.
