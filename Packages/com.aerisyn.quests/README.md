# Aerisyn Quests (`com.aerisyn.quests`)

A game-agnostic quest tracker. Gameplay reports facts; every open Quest that listens moves; the game
grants rewards when a claim succeeds. The package never touches currency, UI, or disk.

**Unity:** 2022.3+ · **Requires:** [Odin Inspector](https://odininspector.com/) (Sirenix) · **Runtime assembly:** `Aerisyn.Quests` · **Version:** `0.1.3`

## Requirements

| Dependency | Why |
|---|---|
| **Unity 2022.3+** | Minimum editor / player target |
| **Odin Inspector (Sirenix)** | Needed so this package compiles. Authoring ScriptableObjects use Odin attributes in the same assembly as the Tracker. |

Install Odin from the Unity Asset Store (or your usual Sirenix workflow) into the **consuming project**. Odin is not on UPM and is **not** shipped in this repository (install per machine / per project).

Tracker and definition types do not call Odin at runtime. You can build `QuestDefinition`s in code or from CSV and never use the ScriptableObject assets; Odin still has to be present for the assembly to build.

## Install

1. Install **Odin Inspector** into your Unity project.
2. Package Manager → **+ → Add package from git URL…**

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.quests
```

## Concepts

| Term | Meaning |
|---|---|
| **Quest** | Authored definition: Objective, Accumulation, Step thresholds, ClaimPolicy |
| **Board** | Game-owned set of Quests that open and close together (stage, daily, event) |
| **Objective** | `int kind` (your enum cast to int) + `int param`, or match-any-param |
| **Accumulation** | `Sum` (count), `HighWater` (keep max), `Flag` (set once) |
| **Step** | One threshold with its own claim; a Quest has 1..64 |
| **ClaimPolicy** | `OncePerStep` or `RepeatWithReset` (up to a repeat limit) |
| **ProgressSnapshot** | The four numbers you save per Quest |

Full glossary: [`CONTEXT.md`](../../CONTEXT.md). Design decisions: [`docs/adr/`](../../docs/adr/).

## Quick start

```csharp
using Aerisyn.Quests;

enum Kind { ServeCustomer = 1, UpgradeStall = 2 }   // the game owns its verbs

var tracker = new QuestTracker();
var stage = new BoardId("stage.1.3");

tracker.OpenBoard(stage, new[]
{
    // Serve 10 customers at any stall, claim once.
    new QuestDefinition(1, Objective.AnyParam((int)Kind.ServeCustomer), Accumulation.Sum, 10),

    // Stall 2 reaches level 25 (report the level, keep the max).
    new QuestDefinition(2, new Objective((int)Kind.UpgradeStall, 2), Accumulation.HighWater, 25),
}, savedSnapshots /* ProgressSnapshot[] from your save, or null */);

// UI listens once.
tracker.ProgressChanged     += questId => RefreshRow(questId);
tracker.StepBecameClaimable += (questId, stepIndex) => ShowClaimButton(questId, stepIndex);
tracker.BoardChanged        += boardId => Save(boardId, tracker.ExportBoard(boardId));

// Gameplay reports facts. No modes, no flags.
tracker.Report((int)Kind.ServeCustomer, objectiveParam: 2, reportedValue: 1);
tracker.Report((int)Kind.UpgradeStall,  objectiveParam: 2, reportedValue: 25);

// Claim button: a true return is the signal to grant.
if (tracker.TryClaim(new QuestId(stage, 1), stepIndex: 0))
    Grant(myRewardTable[(stage, 1, 0)]);

// Stage over.
tracker.CloseBoard(stage);
```

## Authoring ScriptableObjects

`Assets → Create → Aerisyn → Quests → Quest` and `Quest Board`. Layout and validation use **Odin**
(conditional fields, list drawers, InfoBoxes). Identity on a board asset is `BoardId` (use `BoardId.Value` for the raw string). Call `board.BuildDefinitions()` and hand the result to `OpenBoard`. Prefer code or CSV? Build `QuestDefinition` directly and skip these assets.

## Rules worth knowing

- A Quest stops accumulating once every unclaimed Step is reached, and resumes only if a later Step is still ahead. A single-step Quest freezes at its threshold until claimed.
- `Flag` Quests must have exactly one Step with threshold 1.
- Local ids must be unique per Board; `OpenBoard` throws on duplicates and leaves nothing half-open.
- Restoring a snapshot clamps unknown Step bits and negative values; unknown local ids are ignored.
- Events fire after a whole `Report` has been applied, so handlers see a consistent Tracker.

## Out of scope (by design)

Reward payloads, quest text and icons, go-to navigation, analytics, save files. See ADR-0001, ADR-0002.

## Layout

| Folder | Purpose |
|---|---|
| `Runtime/Core` | Tracker, definitions, snapshots, views (no UnityEngine) |
| `Runtime/Authoring` | `QuestAsset`, `QuestBoardAsset` ScriptableObjects (Odin) |
| `Samples~/BasicBoard` | Runnable demo with a temporary JSON store |
