# Aerisyn Quests (`com.aerisyn.quests`)

A game-agnostic quest tracker. Gameplay reports facts; every open Quest that listens moves; the game
grants rewards when a claim succeeds. The package never touches currency, UI, or disk.

**Unity:** 2022.3+ · **Dependencies:** none · **Runtime assembly:** `Aerisyn.Quests` (pure C#)

## Install

Package Manager → **+ → Add package from git URL…**

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
    new QuestDefinition(1, Objective.Any((int)Kind.ServeCustomer), Accumulation.Sum, 10),

    // Stall 2 reaches level 25 (report the level, keep the max).
    new QuestDefinition(2, new Objective((int)Kind.UpgradeStall, 2), Accumulation.HighWater, 25),
}, savedSnapshots /* ProgressSnapshot[] from your save, or null */);

// UI listens once.
tracker.ProgressChanged     += id => RefreshRow(id);
tracker.StepBecameClaimable += (id, step) => ShowClaimButton(id, step);
tracker.BoardChanged        += board => Save(board, tracker.Export(board));

// Gameplay reports facts. No modes, no flags.
tracker.Report((int)Kind.ServeCustomer, param: 2, value: 1);
tracker.Report((int)Kind.UpgradeStall,  param: 2, value: 25);

// Claim button: a true return is the signal to grant.
if (tracker.TryClaim(new QuestId(stage, 1), step: 0))
    Grant(myRewardTable[(stage, 1, 0)]);

// Stage over.
tracker.CloseBoard(stage);
```

## Authoring in the Inspector (optional)

`Assets → Create → Aerisyn → Quests → Quest` and `Quest Board`. The inspectors show validation
errors (bad thresholds, duplicate local ids). Call `board.BuildDefinitions()` and hand the result to
`OpenBoard`. CSV-driven games can skip these assets and build `QuestDefinition` directly.

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
| `Runtime/Authoring` | `QuestAsset`, `QuestBoardAsset` ScriptableObjects |
| `Editor/` | Validation inspectors |
| `Samples~/BasicBoard` | Runnable demo with a temporary JSON store |
