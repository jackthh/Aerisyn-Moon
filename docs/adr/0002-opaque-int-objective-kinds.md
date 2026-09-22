# Opaque int Objective kinds

Objective kind is an opaque `int` (plus optional param). Games define their own enums and cast to int. The package does not ship a shared verb catalog.

**Why:** Keeps `com.aerisyn.quests` game-agnostic across Idle and future titles, while matching Idle's existing numeric QuestType wire shape. A package-owned enum would force every game onto one verb list and make additive versioning painful.
