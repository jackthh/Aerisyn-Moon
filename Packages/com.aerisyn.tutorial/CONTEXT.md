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
Enforcement on a Step: coaching that never blocks play. The player can keep doing other things while the Step is active.
_Avoid_: Optional, skippable, weak (those are separate skip/completion policies, not Soft)

**Hard**:
Enforcement on a Step: play is gated until the Step succeeds. The player cannot usefully continue past the gate until the success condition is met.
_Avoid_: Mandatory, required, strong (use Hard); skip rules are separate from Enforcement

**Enforcement**:
The Soft-or-Hard mode of a single Step. Soft and Hard can mix inside one Tutorial.
_Avoid_: Soft Tutorial / Hard Tutorial as the everyday name for a whole Tutorial (Enforcement is per Step)

**Hint**:
Transient coaching content associated with an active Step. How it looks (UI, VFX, audio) stays in the game; this package does not own presentation.
_Avoid_: Toast as the package contract name

## Relationships

- **Tutorial ↔ Quests**: none yet; a later design may share goal/progress ideas with Quests, but this package must not depend on Quests until that decision is ADR'd.
- **Authors ↔ Players**: developers and designers author Tutorials; players experience them at runtime.
