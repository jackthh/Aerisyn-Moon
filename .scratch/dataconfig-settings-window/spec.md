Status: done

# Spec: Data Config Settings window (0.7.0)

Feature slug: `dataconfig-settings-window`  
Package: `com.aerisyn.dataconfigsheet` (after 0.6.0 safer Pull environments → 0.7.0)  
Context: Data Config  
Depends on: 0.6.0 Pull environments / Title Check / Pull All / Config Manifest (merge first; this release replaces the multi-asset setup UX)

## Problem Statement

After 0.6.0, a game must create several ScriptableObjects (Environment Key list, Feature Key list, Environment Profile) and wire them into every Pull Config, plus auth on each Pull Config. Pull All still discovers every Pull Config in the project. Designers and developers want one window under Aerisyn → Data Config Sheet for keys, bindings, auth, and Pull All, and they want Pull All limited to Pull Configs under one chosen folder.

## Solution

Ship **0.7.0** with one **Data Config Settings** asset at a fixed path, edited by a **Data Config Sheet** EditorWindow (settings + Sign In / Sign Out + Pull All). Pull Config keeps Feature Key, output, Config Types, and Skip Title Check only. Pull All recursively scans the settings folder path (folder picker; blank path fails). Remove separate Environment Key list, Feature Key list, and Environment Profile CreateAssetMenu types. Hard break from 0.6.0; no auto-migrator. Document recreate-in-window.

## User Stories

1. As a developer, I want one Data Config Settings asset for the project, so that I do not create Environment Key list, Feature Key list, and Environment Profile assets separately.
2. As a developer, I want Data Config Settings auto-created at `Assets/AerisynDataConfig/DataConfigSettings.asset` when I first open the window, so that setup starts without Create Asset menus.
3. As a developer, I want Environment Keys edited on Data Config Settings, so that the list has a single definition site.
4. As a developer, I want Dev, Test, and Production seeded when settings are first created, so that common stages exist immediately.
5. As a developer, I want to add custom Environment Keys on that same list, so that extra stages are allowed.
6. As a developer, I want Feature Keys edited on Data Config Settings (empty by default), so that game features are named once.
7. As a developer, I want the active Environment Key picked on Data Config Settings, so that Pull and Pull All share one active stage.
8. As a developer, I want Environment Key × Feature Key → spreadsheet id bindings on Data Config Settings, so that ids are not stored on Pull Config.
9. As a developer, I want title-prefix overrides on Data Config Settings, so that Title Check defaults remain and custom keys get prefixes.
10. As a developer, I want Config Manifest path and required Config Types on Data Config Settings, so that completeness stays project-scoped.
11. As a developer, I want Google auth mode and credential paths on Data Config Settings, so that every Pull shares one sign-in.
12. As a developer, I want the OAuth user token under UserSettings (gitignored), so that tokens stay machine-local.
13. As a developer, I want an EditorWindow under Aerisyn → Data Config Sheet, so that I edit settings without hunting Project assets.
14. As a developer, I want Sign In and Sign Out in that window, so that auth is part of the same setup surface.
15. As a developer, I want Pull All in that window, so that run-all is next to the folder path and active environment.
16. As a developer, I want Pull Config to store only a Feature Key (plus output, types, Skip Title Check), so that env and auth are not duplicated per config.
17. As a developer, I want Pull to resolve spreadsheet id from Data Config Settings using active Environment Key and Feature Key, so that switching environment does not edit Pull Configs.
18. As a developer, I want Pull to use auth from Data Config Settings, so that I sign in once for all configs.
19. As a developer, I want a Pull All folder path on Data Config Settings, so that discovery is scoped.
20. As a developer, I want a folder picker for that path (not bare string-only entry), so that I can browse to the folder.
21. As a developer, I want Pull All to recursively include Pull Configs under that folder, so that nested feature folders work.
22. As a developer, I want Pull All to fail clearly when the folder path is blank, so that whole-project scans do not return by accident.
23. As a developer, I want Pull All to fail clearly when the folder has no Pull Configs, so that an empty run is not success.
24. As a developer, I want Manifest Asset Path to keep a `.asset` file picker, so that the manifest remains one file path.
25. As a package consumer, I want Environment Profile and separate key-list CreateAssetMenu entries removed, so that the old multi-file model cannot be recreated by habit.
26. As a package consumer, I want CONTEXT and README to use Data Config Settings (and retire Environment Profile), so that language matches the product.
27. As a package consumer, I want a hard-break CHANGELOG for 0.7.0 that says recreate settings in the window, so that I do not expect an auto-migrator.
28. As a package maintainer, I want this after 0.6.0 merges, so that the three-asset model is not rewritten on the unreleased branch mid-flight.
29. As a QA engineer, I want pure tests for folder-scoped Pull Config path selection and blank-path failure, so that discovery rules are locked without Unity UI.
30. As a QA engineer, I want existing resolve / Title Check / Pull All summary / manifest completeness seams retargeted to Data Config Settings data, so that 0.6.0 behavior is not lost.
31. As a developer, I want Pull All to keep continuing after failures and to show job failures plus missing required Config Types, so that 0.6.0 summary behavior remains.
32. As a developer, I want Skip Title Check to remain on Pull Config, so that exception sheets still opt out without a settings-wide disable.
33. As a developer, I want the build gate helpers to take Data Config Settings (Production check vs completeness), so that CI wiring still works after the rename.
34. As a developer, I want menu aliases for Pull All / Sign In to keep working or clearly redirect to the window, so that muscle memory is not a dead end.
35. As a sample author, I want samples/docs updated for the window and fixed settings path, so that Weapons Pull does not teach Environment Profile.
36. As a studio lead, I want no automatic migration from 0.6.0 assets, so that the package stays small.
37. As a developer, I want Inspectors to pick Environment Keys and Feature Keys only from Data Config Settings lists, so that free-typed strings cannot drift.
38. As a developer, I want a missing or unloadable Data Config Settings asset to fail Pull with a clear message, so that a deleted settings file is obvious.
39. As a developer, I want Include In Pull unchanged on Pull Config, so that per-type gating still works under folder-scoped Pull All.
40. As a CI/EditMode tester, I want folder-scan and settings-resolve coverage without live Google, so that 0.7.0 stays offline-testable.

## Implementation Decisions

- Land only after 0.6.0 (`dataconfig-pull-environments`) is merged; ship as package **0.7.0**.
- Introduce **Data Config Settings** as the single project settings ScriptableObject at `Assets/AerisynDataConfig/DataConfigSettings.asset`. Window creates it on first open if missing, with Environment Keys seeded to Dev, Test, Production and Feature Keys empty.
- Fold into Data Config Settings everything 0.6.0 put on Environment Key list, Feature Key list, and Environment Profile, plus Google auth fields currently on Pull Config, plus Pull All folder path.
- EditorWindow under **Aerisyn → Data Config Sheet** (name consistent with menu root): edit settings, Sign In / Sign Out, Pull All.
- Pull Config: remove Environment Profile reference and auth fields; keep Feature Key picker bound to settings Feature Keys; resolve spreadsheet id and credentials via loaded Data Config Settings.
- Pull All discovery: recursive Pull Configs under the settings folder path only; Odin/folder picker UI; blank path → fail (no `FindAssets` whole-project fallback).
- Manifest Asset Path remains a `.asset` file path with file picker.
- Delete or obsolete CreateAssetMenu for EnvironmentKeyList, FeatureKeyList, EnvironmentProfile; remove PullAllProfileRule multi-profile behavior (one settings asset makes it moot).
- Build gate APIs accept Data Config Settings (or a thin adapter); Production-only vs completeness stay separate.
- Hard break: no migrator from 0.6.0 assets; CHANGELOG/README instruct recreate in the window.
- Update CONTEXT (done with this spec publish), README, CHANGELOG, samples.
- Prefer ADR only if maintainers want a durable “why one window” record; CONTEXT + this spec are enough to implement.

## Testing Decisions

Good tests assert external behavior only (inputs → included paths / errors / resolved ids / summaries). No live Google. No EditorWindow click tests required in the automated suite.

**Proposed seams (confirm or amend before /to-tickets):**

1. **Primary (new):** pure folder-scope rule — given a project-relative folder path and a set of asset paths, return which paths are Pull Configs in scope (recursive prefix rules, stable order). Blank or whitespace folder path → validation failure (clear error contract). Prefer one small pure module called from the Editor discovery wrapper.
2. **Secondary (existing, retarget):** spreadsheet id resolve and key membership against Data Config Settings-shaped data (reuse/adapt `KeyListRules` / `SpreadsheetIdResolver` tests; drop Environment Profile types from fixtures).
3. **Regression (existing):** Title Check / Skip Title Check, Pull All summary aggregation, manifest completeness / mixed-run missing types — fixtures use settings data instead of Environment Profile.

Prior art: `Tests~/VerticalNest.Tests` (`KeyListRulesTests`, `SpreadsheetIdResolverTests`, `PullAllSummaryTests`, `ConfigManifestTests`, `TitleCheckRulesTests`, `SourceSheetTitleGuardTests`).

## Out of Scope

- Auto-migration from 0.6.0 Environment Profile / key-list assets
- Amending unreleased 0.6.0 in place (explicitly deferred to 0.7.0)
- Per-Pull-Config auth override
- Multiple Data Config Settings assets per project
- Whole-project Pull All fallback
- Changing OAuth scopes
- R2 / remote config publish
- Merging Source Sheets into one workbook per environment
- PirateWars asset rewrite (game repo)
- Live Google or full EditMode UI automation for the window

## Further Notes

- Grill lock (confirmed): window + single Data Config Settings; Pull Config Feature Key only; fixed path auto-create; folder-scoped Pull All with folder picker; auth on settings; Sign In in window; retire Environment Profile term; 0.7.0 after 0.6.0; hard break, no migrator.
- Parent feature: `.scratch/dataconfig-pull-environments/` (0.6.0).
- Domain: `Packages/com.aerisyn.dataconfigsheet/CONTEXT.md`.
- **Seam check:** confirmed by user (folder-scope primary; settings resolve secondary; Title Check / Pull All / manifest regression).

## Comments

- Published from `/to-spec` after `/grill-with-docs` confirmation.
- Test seams confirmed.
- Tickets published (`issues/01`–`04`); frontier starts at 01 after 0.6.0 merges.
