Status: ready-for-agent

# Spec: Local Only fields + After Pull (0.5.0)

Feature slug: `dataconfig-local-only`  
Package: `com.aerisyn.dataconfigsheet` (shipping ~0.4.0 → 0.5.0)  
Context: Data Config  
ADR: [0013](../../docs/adr/0013-dataconfigsheet-local-only-after-pull.md) (builds on ADR 0010 Pull → SO)

## Problem Statement

Pull today requires every serializable scalar and primitive-array field in the Config Type nest shape to appear on the Source Sheet Header Row (Field Header or Column Alias). When the sheet has fewer columns than the C# shape (e.g. designers maintain `turnSpeed` online while the Baked Asset also holds derived `accelerateTurnSpeed` / `decelerateTurnSpeed`), Header Row match fails and Pull aborts. Games need a way to mark sheet-unowned fields, keep Pull succeeding, and refill derivatives on every Pull without package-owned formulas.

## Solution

Add opt-in **Local Only** (`[LocalOnly]`) on Config Type fields at any nest level: not required for Header Row match, never bound from cells; if a matching column appears anyway, warn and ignore (alias text included in the warning). Header Emitter omits Local Only fields and notes how many were omitted. Pull stays **clean** (no merge of prior Inspector Local Only values). After all included tabs parse successfully and before any Baked Asset create/save, Pull calls virtual **`OnAfterPull()`** on each Config Type scratch so games fill Local Only derivatives; a throw fails the whole Pull with no writes. Ship as package **0.5.0** with docs aligned to CONTEXT and ADR 0013.

## User Stories

1. As a developer, I want to mark a Config Type field as Local Only, so that designers are not forced to put that column on the Source Sheet.
2. As a developer, I want Local Only fields omitted from Header Row match requirements, so that Pull succeeds when the sheet has fewer columns than the C# nest shape.
3. As a developer, I want Local Only fields never bound from sheet cells, so that online cells cannot overwrite game-owned or derived values during parse.
4. As a developer, I want Local Only usable on nested nest-level fields (not only root item fields), so that Vertical Nest child types can hold local derivatives too.
5. As a developer, I want Local Only to apply to scalars and primitive arrays/lists that would otherwise be Field Headers, so that the same opt-out covers the columns Pull currently requires.
6. As a designer, I want to maintain only the online columns I care about, so that sheet setup stays small.
7. As a designer, I want Ignore Marker (`!!!`) to remain for extra sheet note columns, so that Local Only is not confused with designer notes online.
8. As a developer, I want a clear distinction between Ignore Marker (sheet has extras) and Local Only (Config Type / Baked Asset has extras), so that vocabulary stays unambiguous.
9. As a developer, I want Pull to warn when the sheet still has a column matching a Local Only field name or Column Alias, so that accidental paste of an old full Header Row is visible.
10. As a developer, I want that warning to mention the Column Alias when relevant, so that I can find the bad header text on the sheet.
11. As a developer, I want Pull to ignore that accidental Local Only column and continue, so that a stray header does not hard-fail a good Pull.
12. As a developer, I want Header Emitter to omit Local Only fields from the pasteable Header Row, so that the clipboard matches the real sheet contract.
13. As a developer, I want Header Emitter guidance to say how many Local Only fields were omitted, so that I know the emit is incomplete relative to the full C# shape on purpose.
14. As a developer, I want Copy Header Row menu behavior to follow the same omit rules, so that editor UX matches the emitter.
15. As a developer, I want re-Pull to rebuild rows cleanly without merging prior Inspector Local Only values, so that behavior stays simple and predictable (ADR 0013).
16. As a developer, I want Local Only fields to start at type defaults after parse, so that After Pull has a known baseline before I fill derivatives.
17. As a developer, I want a virtual `OnAfterPull` on Config Type assets, so that I can point my recalculation code at a package-owned lifecycle seam.
18. As a developer, I want `OnAfterPull` to run after every included tab parses successfully, so that I never derive against a half-parsed multi-tab Pull.
19. As a developer, I want `OnAfterPull` to run before any Baked Asset is created or saved, so that derived Local Only values land in the written asset.
20. As a developer, I want a throw from `OnAfterPull` to fail the whole Pull with no writes, so that Baked Assets never end up partially updated across tabs.
21. As a developer, I want the default `OnAfterPull` to be a no-op, so that Config Types that need no derivatives stay unchanged.
22. As a developer, I want to compute derivatives from sheet-owned fields inside `OnAfterPull` (e.g. `turnSpeed` → accel/decel turn speeds), so that every Pull refreshes local math without Google owning those columns.
23. As a developer, I want not to rely on Unity `OnValidate` or Odin `OnValueChanged` for post-Pull fill, so that automatic refill works even when Inspector callbacks do not run after overwrite.
24. As a package consumer, I want README and CHANGELOG to document Local Only and After Pull for 0.5.0, so that I can adopt the feature without reading the ADR only.
25. As a package consumer, I want samples or docs examples to show a Local Only field plus an `OnAfterPull` override pattern, so that I can copy the intended use.
26. As a package maintainer, I want the public attribute named `LocalOnly` (not IgnorePull / NonSerialized), so that it does not collide with Unity serialization or Ignore Marker language.
27. As a package maintainer, I want package version bumped to 0.5.0, so that consumers see a minor feature release.
28. As a QA engineer, I want fixture tests where the sheet omits Local Only columns and parse succeeds, so that the original consumer failure mode is locked green.
29. As a QA engineer, I want a regression where unmarked missing headers still fail Header Row match, so that Local Only does not silently weaken the whole contract.
30. As a QA engineer, I want tests that a Local Only field stays at default after parse even if a matching column exists, so that never-bind is locked.
31. As a QA engineer, I want tests that a present Local Only column produces a warning (with alias when set), so that warn-and-ignore is locked.
32. As a QA engineer, I want Header Emitter tests that omit Local Only and report the omit count in guidance, so that paste output stays correct.
33. As a QA engineer, I want tests that `OnAfterPull` runs before write and can mutate Local Only fields that then appear on the saved/copied items path, so that the derivative workflow is locked.
34. As a QA engineer, I want tests that an `OnAfterPull` throw prevents any Baked Asset write for that Pull, so that all-or-nothing After Pull failure is locked.
35. As an agent implementing tickets, I want domain terms (Local Only, After Pull, Ignore Marker, Field Header, Baked Asset, Pull) used consistently with CONTEXT.md, so that tickets and code share language.
36. As a developer with nested Vertical Nest types, I want Local Only on a child-level field to be skipped for header match at that nest level, so that deep shapes can have local columns too.
37. As a developer, I want `[LocalOnly]` and `[ColumnAlias]` on the same field to remain allowed, so that warning text can still name the designer header if someone pastes it.
38. As a developer, I want existing Column Alias, Ignore Marker, Preamble, and Vertical Nest behavior unchanged for non-Local Only fields, so that 0.5.0 is additive.
39. As a CI/EditMode tester, I want Local Only parse and Header Emitter coverage without live Google, so that the feature is testable offline.
40. As a studio lead, I want no field-level merge / row-id preserve in 0.5.0, so that we do not take on identity-matching risk for this release.

## Implementation Decisions

- Follow ADR 0013: Local Only + clean Pull + virtual After Pull; no Inspector preserve / row-id merge in 0.5.0.
- Add a public `[LocalOnly]` attribute in the Data Config runtime API surface alongside existing `[ColumnAlias]` / `[SheetTab]` style attributes.
- Local Only may mark fields at any nest level that participate in Field Header collection today (scalars and primitive arrays/lists). Nested struct-list fields themselves are not “whole nest Local Only” in this release.
- Header Row discovery and expected-header collection skip Local Only fields; parse bindings never assign into Local Only fields.
- If the sheet Header Row contains a Local Only field’s name or Column Alias, Pull emits a warning (including alias text when present) and does not bind that column; Pull does not hard-fail solely for that reason.
- Header Emitter omits Local Only labels from the emitted Header Row and adds guidance that N Local Only fields were omitted.
- `ConfigTypeAsset` gains `public virtual void OnAfterPull()` with an empty default body.
- After all included Config Types parse successfully into scratches, the Pull runner invokes `OnAfterPull` on each scratch before any create/save. Then create new assets or overwrite existing ones as today.
- If any `OnAfterPull` throws, the Pull fails as a whole: no Baked Asset creates/saves for that run (same all-or-nothing spirit as parse failure before write).
- Live Google Pull and inject `PullFromGrids` share that After Pull timing (same runner path after grids are in hand).
- Package version → 0.5.0; CHANGELOG and README document Local Only, warnings, Header Emitter omit note, and `OnAfterPull`.
- CONTEXT.md / ADR 0013 already record the domain freeze; implementation must not contradict them.
- Do not introduce a separate After Pull interface or UnityEvent wiring in 0.5.0.

## Testing Decisions

- Good tests assert external behavior only: given grids / emit inputs / Pull inject outcomes, assert object graphs, warnings, emit text, write-or-no-write results. Do not assert private helper call order or reflection internals.
- **Primary seam (existing):** Vertical Nest `ParseInto` over an in-memory grid. Covers Local Only not required for Header Row match, never bound, warn-and-ignore when a matching column (name or Column Alias) is present, including nest-level Local Only. Prior art: `VerticalNestParserTests`, `ColumnAliasParserTests`, weapons fixture tests.
- **Secondary seam (existing):** Header Emitter emit API. Covers omission of Local Only fields and guidance mentioning omit count. Prior art: `HeaderEmitterAndTabTests`.
- **After Pull seam (existing product path preferred):** `PullFromGrids` inject Pull. Covers `OnAfterPull` after successful parses, before create/save; throw fails Pull with no writes; successful override can fill Local Only fields that persist through the write path. If EditMode/AssetDatabase cost is too high, fall back to one thin pure pre-write helper invoked by the runner (new seam only then). Prior art: `BakedAssetOverwriteTests`, `PullTargetRulesTests`, inject Pull documentation on the runner.
- No live Google or Sheets HTTP in the automated suite for this feature.
- Unmarked missing required headers must still fail (regression). Non-Local Only Ignore Marker / alias behavior must remain green.

## Out of Scope

- Preserving Inspector-authored Local Only values across re-Pull (field merge, row `id` matching, nest-child identity preserve)
- Sheet-optional bind-if-present semantics (Local Only never binds)
- Package-owned derivative formulas
- Relying on `OnValidate` / Odin `OnValueChanged` as the official After Pull mechanism
- After Pull interface or UnityEvent / Pull Config listener wiring
- Marking an entire nested struct-list field as Local Only (skip whole child nest)
- Two-way sync to Google
- Changing Ignore Marker (`!!!`) semantics
- Live Google integration tests for this feature
- Quests / Tutorial package integration

## Further Notes

- Domain vocabulary: `Packages/com.aerisyn.dataconfigsheet/CONTEXT.md`; system ADR 0013; parent Pull design ADR 0010 / `.scratch/dataconfig-pull-so/`.
- Design docs PR: https://github.com/jackthh/Aerisyn-Moon/pull/25
- Confirmed test seams (to-spec step 2): ParseInto primary; HeaderEmitter secondary; PullFromGrids for After Pull (thin helper only if AssetDatabase blocks tests).
- Next skill: `/to-tickets` → `.scratch/dataconfig-local-only/issues/NN-*.md`.

## Comments

- Published from `/to-spec` after grill-with-docs lock, ADR 0013, and user confirmation of test seams.
