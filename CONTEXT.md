# Quests

Domain language for `com.aerisyn.quests`: authored goals a player can pursue, how progress is tracked, and how claimable rewards are signaled to the game.

## Language

**Quest**:
An authored definition of a goal: what to listen for, how progress accumulates, ordered steps with thresholds and rewards, and how claiming works.
_Avoid_: Mission (unless a game renames it in UI), task as a synonym for the definition itself

**Progress**:
One player's durable state on a single Quest: current value, which steps are claimed, and how many times it has been claimed.
_Avoid_: PlayerQuestData, quest row

**Progress Snapshot**:
The serializable shape of Progress that a game (or a later save package) stores and restores. Quests does not own the official save pipeline.
_Avoid_: Save file, PlayerPrefs as the official contract

**Board**:
A named set of Quests that open and close together (stage, daily, event season). The game owns boards; the package indexes their Quests while a board is open.
_Avoid_: QuestGroup, QuestGroupType as the everyday name

**Objective**:
The fact a Quest listens for (kind + optional param), reported by gameplay.
_Avoid_: QuestType as the name for the whole Quest; targetId as jargon for the param
