# Tutorial

Domain language for `com.aerisyn.tutorial`: tools for game developers and designers to author in-game tutorials that players run through.

## Language

**Tutorial**:
An authored, ordered sequence of Steps that teaches a player a gameplay verb or system.
_Avoid_: Quest as a synonym (Quests is a separate package); Mission unless a game renames it in UI

**Step**:
One beat in a Tutorial. Each Step has an Enforcement of Soft or Hard.
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
Transient coaching content associated with an active Step. How it looks (UI, VFX, audio) stays in the game; this package does not own presentation.
_Avoid_: Toast as the package contract name

**Report**:
Gameplay notifying the tutorial runtime that something happened (opaque kind plus optional param), so an active Step can succeed. Same idea as Quests Reports; this package does not own the verb catalog.
_Avoid_: Unity scene watches or button RectTransform refs as the package’s success contract

**Gate signal**:
A runtime notice that a Hard Step started or ended so the game can lock or unlock input/UI. The package does not freeze input itself.
_Avoid_: Package-owned InputManager / Canvas block as the core API

## Relationships

- **Tutorial ↔ Quests**: none yet; Reports may look like Quests Objectives later, but this package must not depend on Quests until that is ADR'd.
- **Authors ↔ Players**: developers are the primary authors of Tutorials; designers may get a lighter authoring path later. Players experience Tutorials at runtime.
- **Tutorial ↔ Game presentation**: Hints and Hard Gate signals are consumed by the game; presentation and input lock stay in the game.
