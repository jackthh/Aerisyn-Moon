# Changelog

All notable changes to `com.aerisyn.quests` are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

- `QuestBoardAsset` exposes a single identity, `BoardId`. `BoardName` is now an `[Obsolete]` alias (use `BoardId.Value` for the raw string) and is removed in 0.2.0.

## [0.1.1] - 2026-09-23

Readability pass: clearer names and summary comments. No behavior changes. Old public names still
compile as `[Obsolete]` aliases and are removed in 0.2.0.

### Changed

- `Objective.Any` renamed to `Objective.AnyParam`.
- `QuestId.Board` renamed to `QuestId.BoardId`.
- `QuestTracker.TryGet` renamed to `TryGetQuest`; `QuestTracker.Export` renamed to `ExportBoard`.
- `QuestView.Value` renamed to `ProgressValue`, `ClaimCount` to `CompletedCycles` (it counts full claim cycles, not Step claims), `CurrentStep` to `FirstUnclaimedStepIndex`, `GetThreshold` to `GetStepThreshold`.
- `QuestBoardAsset.BoardIdValue` renamed to `BoardName`; `QuestBoardAsset.Id` renamed to `BoardId`.
- Parameter names spell out intent (`stepIndex`, `objectiveKind`, `objectiveParam`, `reportedValue`, `savedSnapshots`). Callers using named arguments need to update them.
- Serialized authoring fields renamed (`_objectiveKind`, `_objectiveParam`, `_stepThresholds`, `_boardName`) with `[FormerlySerializedAs]`, so existing assets keep their data.
- Summary comments on every public type and member, plus step outlines on `OpenBoard`, `Report`, and `TryClaim`.

### Unchanged on purpose

- `ProgressSnapshot` field names (`Value`, `ClaimCount`, ...) stay as-is because they are the save format. Their docs now point at the clearer `QuestView` names.

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
- Committed Unity `.meta` files for every package asset and folder (required for Git Package Manager installs).

### Notes

- Progress values are `long`. Wider numeric types are deferred.
- Reward contents, descriptions, icons, navigation, and analytics stay in the game.
