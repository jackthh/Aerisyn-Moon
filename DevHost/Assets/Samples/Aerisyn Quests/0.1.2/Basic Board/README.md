# Basic Board sample

One `QuestTracker`, one Board, five Quests that cover every accumulation and claim mode.

## Try it

1. Import the sample from Package Manager (**Aerisyn Quests → Samples → Basic Board**).
2. Add `SampleStageBoard` to any GameObject and enter Play mode.
3. Right-click the component header and use the **Report / …** and **Claim all** context menu items.
4. Watch the Console. Exit and re-enter Play mode: progress is restored from a temp JSON file.

## What it shows

| Quest | Objective | Accumulation | Claim |
|---|---|---|---|
| 1 | ServeCustomer, any stall | Sum to 10 | once |
| 2 | EarnCash | Sum, steps 100 / 1 000 / 10 000 | once per step |
| 3 | UpgradeStall, stall 1 | HighWater to 25 | once |
| 4 | UnlockStall, stall 2 | Flag | once |
| 5 | ServeCustomer, any stall | Sum to 3 | repeat with reset, 3 times |

Reporting one served customer moves quest 1 and quest 5 at the same time: that is the shared bus.

## Not for production

`TempJsonProgressStore` exists only so the demo survives a restart. Real games pass `ProgressSnapshot[]`
from `QuestTracker.Export` into their own save pipeline. See ADR-0001 in the repo root.
