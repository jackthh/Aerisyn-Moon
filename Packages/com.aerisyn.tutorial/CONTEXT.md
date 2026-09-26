# Tutorial

Domain language for `com.aerisyn.tutorial`: tools for game developers and designers to author in-game tutorials that players run through.

## Language

**Tutorial**:
An authored, ordered sequence of Steps that teaches a player a gameplay verb or system. At most one Tutorial is active at a time in v1.
_Avoid_: Quest as a synonym (Quests is a separate package); Mission unless a game renames it in UI

**Step**:
One beat in a Tutorial. Each Step has an Enforcement of Soft or Hard, optional Choreography of Cues, and a Report that marks success.
_Avoid_: Lesson, stage (unless a game renames it in UI)

**Soft**:
Enforcement on a Step: coaching that never blocks play. The player can keep doing other things while the Step is active. A reminder-only flow is just a Tutorial whose Steps are Soft, not a second Soft catalog.
_Avoid_: Optional, skippable, weak (those are separate skip/completion policies, not Soft); Soft-track / TutorialSoftId as a parallel id system (Idle-Axolotl pattern we are not copying)

**Hard**:
Enforcement on a Step: play is gated until the Step succeeds. The player cannot usefully continue past the gate until the success condition is met.
_Avoid_: Mandatory, required, strong (use Hard); skip rules are separate from Enforcement; Hard-track as a separate catalog name (use Hard Enforcement on Steps inside a Tutorial)

**Enforcement**:
The Soft-or-Hard mode of a single Step. Soft and Hard can mix inside one Tutorial.
_Avoid_: Soft Tutorial / Hard Tutorial as the everyday name for a whole Tutorial (Enforcement is per Step); a second Soft/Hard “track” alongside Enforcement

**Hint**:
_Avoid as the main term_: prefer Cue. If kept, only as optional text/content id on a Cue, not as the presentation engine.

**Cue**:
One opaque presentation intent while a Step is active (highlight a control, scale an object, hide an item, show text). How it looks stays in the game; the package only names the intent.
_Avoid_: SubStep as the name for presentation (that word implies nested progress); hard-coded UI ops inside the Core package

**Choreography**:
How Cues run for a Step: sequential (await each Cue Done), concurrent (fire-and-forget), or a mix. Independent of Soft/Hard Enforcement and of Report success unless the author ties them.
_Avoid_: Treating Choreography completion as the only way a Step succeeds (success is still a Report unless you explicitly design otherwise)

**Cue Done**:
Game notice that a sequential Cue finished, so Choreography can advance. Concurrent Cues do not need this.
_Avoid_: Frame timers inside Core as the only sequencing mechanism

**Report**:
Gameplay notifying the tutorial runtime that something happened (opaque kind plus optional param), so an active Step can succeed. Same idea as Quests Reports; this package does not own the verb catalog.
_Avoid_: Unity scene watches or button RectTransform refs as the package’s success contract

**Gate signal**:
A runtime notice that a Hard Step started or ended so the game can lock or unlock input/UI. The package does not freeze input itself.
_Avoid_: Package-owned InputManager / Canvas block as the core API

**Completion signal**:
Notice that a Step or Tutorial finished. The game grants rewards at the listen site; reward contents stay outside this package. Catch-up (“already did this”) and whether a reward still applies are game policy, not Core auto-skip.
_Avoid_: Currency, items, or loot tables inside the Tutorial package; silent Core auto-complete that skips Completion signals

**Progress Snapshot**:
The serializable shape of a player’s place in Tutorials (at least which Tutorial and which Step). The package does not own disk I/O; the game chooses whether to resume mid-Tutorial or only store whole-Tutorial completion.
_Avoid_: Save file, PlayerPrefs as the official contract

**Runner**:
The in-memory engine that holds at most one active Tutorial, advances Steps on Reports, schedules Choreography (Cue / Cue Done), and emits Gate and Completion signals. Pure C# in v1; the game Starts and Stops Tutorials explicitly. Not a MonoBehaviour.
_Avoid_: TutorialManager as the product name; package auto-start from world rules in v1

## Relationships

- **Tutorial ↔ Quests**: separate for v1. No package dependency. Reports may look like Quests Objectives, but each context owns its own ids until a shared-bus ADR exists. Rewards follow the same idea as Quests: package signals completion; game grants.
- **Authors ↔ Players**: developers are the usual authors. Authoring is **code-first builders** or a thin **ScriptableObject** (`TutorialAsset`) that projects into the same Tutorial definition for a pure C# Runner. Players experience Tutorials at runtime.
- **Tutorial ↔ Game presentation**: Cues / Choreography / Gate signals are consumed by the game; highlight/scale/hide implementations stay in the game.
- **Catch-up ↔ Rewards**: game decides whether to Start a Tutorial when progress already matches the teaching goal, and whether Completion still pays a reward. Core does not auto-skip Steps in v1.
- **Game ↔ Runner**: game Calls Start/Stop; feeds Report and Cue Done; listens for Gate, Cue, and Completion signals; owns save I/O via Progress Snapshot.
