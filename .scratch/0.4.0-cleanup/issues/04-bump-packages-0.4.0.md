# 04: Bump all packages to 0.4.0 + CHANGELOGs

**What to build:** Quests, Data Config Sheet, and Tutorial all ship as 0.4.0 for coordinated repo cadence while remaining separately installable. Each CHANGELOG honestly describes what changed (Data Config Sheet: cleanup and Pull UX from prior tickets; Quests/Tutorial: cadence bump unless they needed doc alignment). Sample import version folders / docs that embed the old version are updated to match.

**Blocked by:** 01 (Include In Pull + empty Pull Config defaults), 02 (Shared pull helper + Inspector Pull button), 03 (Delete AerisynDataConfig scaffold + docs / gitignore).

**Status:** resolved

- [x] com.aerisyn.quests, com.aerisyn.dataconfigsheet, and com.aerisyn.tutorial package versions are 0.4.0
- [x] Each package CHANGELOG has a 0.4.0 section accurate to what that package changed
- [x] Packages remain separately installable (no merge / no shared forced dependency)
- [x] DevHost sample paths or docs that key off prior package versions are updated for 0.4.0 where required
- [x] Package ids and displayNames unchanged (renames deferred)

## Answer

Bumped all three `package.json` versions to `0.4.0` (ids/displayNames unchanged; no cross-package deps). CHANGELOGs: Data Config Sheet documents Include In Pull, empty defaults, Inspector Pull, AerisynDataConfig removal; Quests/Tutorial are cadence-only. Renamed DevHost sample import folders to `…/0.4.0/…` and updated root + package README version pins and sample-local path examples.
