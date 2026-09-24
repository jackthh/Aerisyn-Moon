Status: ready-for-agent

# Spec: Data Config Pull → ScriptableObject

Feature slug: `dataconfig-pull-so`  
Context: Data Config (`com.aerisyn.dataconfigsheet`)  
ADR: 0010 (supersedes 0009 Luban path)

## Problem Statement

Installing and running the Data Config package today is too hard for a reusable template. The 0.3.0 Luban bake path forces developers to maintain sheet headers, Luban XML, tab→CSV maps, a CLI toolchain, and fragile nested CSV encodings. Nested real data (weapons → upgrade levels → bonus stats) does not feel like the Idle product flow people already trust: hand-written types, Google Sheets, Pull, ScriptableObject assets. Naive CSV bridges also break when cell values contain commas. The package must become an install-and-run template other projects can adopt with little per-sheet wiring, not merely a runnable spike.

## Solution

Replace the Luban-centric bake with a one-way **Pull**: OAuth into Google, read Source Sheet tabs into an in-memory cell grid, parse with a type-driven **Vertical Nest** engine shaped by hand-written Odin **Config Types**, and create or overwrite **Baked Assets** in one shared output folder on the **Pull Config**. Designers keep Preamble notes and `!!!` Ignore Marker columns; developers own schema in C# (Field Headers, optional Column Alias, optional tab-override). Nested lists author as blank parent cells. No required CSV bridge. Strip obsolete Luban/BakingSheet teaching surfaces so newcomers see one workflow.

## User Stories

1. As a package consumer, I want a single documented Pull workflow, so that I can install the package and use it without learning Luban.
2. As a package consumer, I want nested config (parent → child → grandchild lists) to round-trip from Google Sheets into ScriptableObjects, so that real game tables like weapons and upgrades work.
3. As a developer, I want to hand-write Config Types as Odin SerializedScriptableObject subclasses, so that I control naming, nesting, and types in C#.
4. As a developer, I want Pull to create missing Baked Assets automatically, so that I do not hand-author empty `.asset` files.
5. As a developer, I want re-Pull to overwrite data in place, so that asset GUIDs and references stay stable.
6. As a developer, I want one output folder on the Pull Config, so that I do not set a path per tab.
7. As a developer, I want an explicit list of Config Types on the Pull Config, so that I know exactly what Pull will touch.
8. As a developer, I want the Google tab title to match the Config Type name by default, so that wiring a new table is mostly “add type + matching tab”.
9. As a developer, I want an optional tab-override attribute when the tab title must differ, so that awkward designer tab names still work.
10. As a developer, I want Field Headers to map to C# fields, so that the sheet contract stays predictable.
11. As a developer, I want optional Column Alias attributes, so that designers can see friendly header names without renaming fields.
12. As a designer, I want Vertical Nest blank-parent authoring, so that I can enter nested lists without compact cell encodings.
13. As a designer, I want Preamble rows above the Header Row for instructions, so that sheets stay navigable for humans.
14. As a designer, I want `!!!` above a column to mark notes-only columns, so that cloud notes are not fetched into runtime data.
15. As a designer, I want commas inside cells to be safe, so that notes and text fields do not break Pull.
16. As a developer, I want Pull to find the Header Row by matching field names and aliases, so that Preamble height can vary.
17. As a developer, I want OAuth Sign In as the happy path, so that a new machine can authorize without a service-account ritual.
18. As a developer, I want a Header Emitter from a Config Type, so that I can paste correct headers quickly.
19. As a developer, I want a Pull report with sheet cell coordinates on failure, so that I can fix bad rows fast.
20. As a package maintainer, I want Luban and obsolete bake demos removed or demoted, so that newcomers are not taught two pipelines.
21. As a package maintainer, I want menus and config assets named Pull (not Bake) where user-facing, so that language matches CONTEXT.md.
22. As a runtime gameplay programmer, I want Baked Assets as the runtime final, so that I load ScriptableObjects like other Unity config.
23. As a runtime gameplay programmer, I want one Baked Asset per tab (full list inside), so that Addressables and references stay simple.
24. As a CI engineer, I want the design to allow a later service-account auth mode, so that headless Pull can return without redesigning the core.
25. As a developer adding a second table, I want only a new Config Type, a Pull Config list entry, and a matching tab (+ headers), so that per-sheet cost stays small.
26. As a developer, I want nest shape to come from the Config Type (type-driven parse), so that I do not maintain Luban XML or `##var`/`##type` as schema.
27. As a developer, I want primitive arrays (e.g. bonus stats) to author as vertical values in one column, so that list-of-int nests work like Idle.
28. As a developer, I want struct arrays to expand to child field columns, so that nested objects do not need collection-name columns.
29. As a QA engineer, I want fixture tests on the parse seam, so that nest/`!!!`/Preamble regressions fail in CI-style EditMode or pure tests.
30. As a package consumer, I want DevHost (or Samples) to demonstrate weapons-style nested Pull, so that I can copy a working template.
31. As a package consumer, I want the README to describe only the Pull → SO path as the product, so that I am not misled by Luban leftovers.
32. As a designer, I want Ignore Marker syntax to be exactly `!!!`, so that it matches the Idle convention we keep.
33. As a developer, I want Pull to skip Ignore Marker columns entirely, so that note text never lands in Baked Assets.
34. As a developer, I want failed Pulls to leave a clear report rather than a silent partial write, so that I trust asset contents.
35. As a multi-project studio, I want this package to stay game-agnostic, so that level/reward/mission/localization tables can all use the same template patterns.
36. As a developer, I want Google to remain the value source of truth, so that designers do not edit generated assets as the official store.
37. As a developer, I want one-way Pull only, so that temporary Inspector tweaks never push back to Google by accident.
38. As an agent implementing tickets, I want domain terms (Pull, Config Type, Baked Asset, Vertical Nest, etc.) used consistently, so that tickets and code share language with CONTEXT.md.

## Implementation Decisions

- Follow ADR 0010: Pull → in-memory grid → type-driven Vertical Nest → Baked Assets; Luban path is removed from the product surface.
- Keep Google as source of truth for values and OAuth as the documented auth happy path (ADR 0005 / 0007 spirit).
- Config Types are hand-written Odin `SerializedScriptableObject` subclasses plus nested serializable types; the package does not generate those types; Pull only creates/fills assets.
- Pull Config holds spreadsheet id, OAuth settings, one shared output folder, and an explicit Config Type list (no assembly auto-scan; no per-tab output paths).
- Tab resolution: exact Config Type name, or optional tab-override attribute on the type when titles must differ (no suffix guessing).
- Baked Asset cardinality: one asset per matching tab; file name derived from the type/tab convention; all assets for a Pull Config share its output folder; re-Pull overwrites in place (stable GUID).
- Sheet contract: Preamble ignored; Header Row discovered by matching Field Headers and Column Aliases; Ignore Marker is exactly `!!!` on the marker row above the header; Vertical Nest uses blank parent cells; Field Headers are canonical; Column Alias is optional.
- Transport: Sheets API cell values in memory; CSV is not required for a correct Pull (fixes Idle comma breakage). Optional debug dumps must not be the parse path.
- Parser: owned Aerisyn type-driven Vertical Nest implementation with fixture tests; do not vendor/clone ZBase CsvReader wholesale.
- v1 extras: Header Emitter (from Config Type) and Pull report (failures with sheet coordinates when possible). No dry-run mode in v1.
- User-facing Bake naming migrates to Pull where it teaches the workflow.
- Cleanup removes or replaces Luban runner/config fields and DevHost/sample material that teaches Luban or BakingSheet as the primary path; retain Google OAuth/Sheets access code where it still serves Pull.
- Package remains a reusable template: game-specific tables live in the consuming project as Config Types + sheets.

## Testing Decisions

- Good tests assert external behavior of the parse seam only: given a cell grid + Config Type shape, assert the object graph or structured errors (including coordinates). Do not assert private helpers, reflection call order, or Google HTTP details in the main suite.
- **Primary seam (one):** Vertical Nest parse over an in-memory grid. This is the highest valuable seam: it locks Preamble, Header Row match, `!!!`, aliases, and nest rules without Unity AssetDatabase or live Google.
- Adapters (Sheets fetch, asset create/overwrite) stay thin; covered by DevHost/sample smoke and manual Pull, not by expanding the automated seam surface in v1.
- Prior art: almost none in-repo for Data Config today (DevHost Luban smoke only). Prefer new fixture tests at the parse seam over EditorTest sprawl. Quests package tests (if any) are unrelated.
- Fixture set must include a weapons → upgrade levels → bonus stats grid with Preamble and `!!!` note columns, matching the locked sheet behavior.

## Out of Scope

- Two-way sync to Google
- Luban / BakingSheet as supported product paths
- Required CSV (or xlsx) bridge for Pull correctness
- Cloning Idle/ZBase CsvReader as the parser product
- Assembly-wide auto-discovery of Config Types
- Dry-run Pull
- One Baked Asset per top-level row
- Runtime Google download in player builds
- Suffix-guessing tab names
- Making Column Alias or tab-override mandatory
- Quests package integration (boards/quests as sheet tables) in this feature
- Changing Odin from being a shared Aerisyn prerequisite

## Further Notes

- Domain vocabulary: `Packages/com.aerisyn.dataconfigsheet/CONTEXT.md` and root `CONTEXT-MAP.md`.
- Earlier draft notes: package `SPEC.md` / `TICKETS.md` were grilling artifacts; this tracker spec is the agent-ready source for `/to-tickets` and `/implement`.
- Design PR context: https://github.com/jackthh/Aerisyn-Moon/pull/9
- After this spec: run `/to-tickets` to produce tracer-bullet issues under `.scratch/dataconfig-pull-so/issues/` with blocking edges.

## Comments

- Published from `/to-spec` after grill-with-docs lock and ADR 0010. Test seam proposed in-thread: single pure Vertical Nest parse seam; adapters out of main suite.
