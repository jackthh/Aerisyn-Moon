# Tutorial

Domain language for `com.aerisyn.tutorial`: in-game tutorial / onboarding guidance for players.

## Status

**Scaffold only.** Terms below are placeholders so the context map has a landing page.
Grill and replace them via `/domain-modeling` before shipping API names.

## Language

**Tutorial**:
A guided sequence that teaches a player a gameplay verb or system. Exact shape (steps, hints, gates) is TBD.
_Avoid_: Quest as a synonym (Quests is a separate package); Mission unless a game renames it in UI

**Hint**:
Transient coaching presented while a Tutorial is active. Presentation ownership (UI, VFX) stays in the game.
_Avoid_: Toast as the package contract name

## Relationships

- **Tutorial ↔ Quests**: none yet; a later design may drive tutorial steps from quest-like goals, but this package must not depend on Quests until that decision is ADR'd.
