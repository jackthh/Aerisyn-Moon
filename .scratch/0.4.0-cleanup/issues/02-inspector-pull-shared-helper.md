# 02: Shared pull helper + Inspector Pull button

**What to build:** From a Pull Config’s Inspector, the user can click Pull and get the same workflow as the menu (Selected Config): one shared editor helper into the pull runner, including Include In Pull filtering from ticket 01. Menu items “Pull From Google (Selected Config)” and “Pull From Google (All Configs)” keep working and call that same helper. All Configs honors each Pull Config’s own ticks. No Sign In / Sign Out controls are added to the Inspector.

**Blocked by:** 01 (Include In Pull + empty Pull Config defaults).

**Status:** resolved

- [x] Pull Config Inspector exposes a Pull action (no Sign In / Sign Out buttons)
- [x] Inspector Pull and both menu Pull commands share one helper path into the runner
- [x] Selected Config menu behavior matches Inspector Pull for the same asset
- [x] All Configs menu still runs every Pull Config and respects each config’s Include In Pull flags
- [x] Manual smoke: Inspector Pull and menu Pull produce the same success/failure outcomes for the same config

## Answer

Shipped `DataConfigPullWorkflow` as the single Pull-with-UI path into `DataConfigPullRunner`. Menu Selected/All and the Odin Inspector `Pull` button (editor-registered on `PullConfig`) all call it. Sign In / Sign Out remain menu-only.
