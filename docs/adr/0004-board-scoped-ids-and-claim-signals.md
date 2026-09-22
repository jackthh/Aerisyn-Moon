# Board-scoped Quest Ids and dual claim signals

A Quest Id is (Board id, local int) with uniqueness enforced per Board on open. The Tracker exposes progress/claimability events for UI and TryClaim success for granting rewards in the game; reward payloads stay outside the package.

**Why:** Matches Idle's per-board quest_id practice without packed global ids, and keeps grant logic at the claim call site while UI stays reactive.
