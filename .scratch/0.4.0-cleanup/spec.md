# Spec: Aerisyn 0.4.0 cleanup

Status: ready-for-agent

Context: Data Config (`com.aerisyn.dataconfigsheet`); repo-scale version cadence for Quests and Tutorial as well.

Related glossary: `Packages/com.aerisyn.dataconfigsheet/CONTEXT.md` (**Pull**, **Pull Config**, **Config Type**, **Include In Pull**, **Baked Asset**). ADR 0010 remains the Pull → ScriptableObject architecture.

## Problem Statement

The monorepo’s 0.3.x-era Data Config setup left junk and awkward UX. DevHost still has an `Assets/AerisynDataConfig` tree that duplicates what Samples already cover, and PullConfig defaults point at that tree so it feels required. Pull only runs from the menu bar, so designers bounce between Project selection and menus. The Config Types list on a Pull Config is all-or-nothing: every listed type is always Pulled, with no way to keep candidates listed but skip some for a given run. Separately, the Project window shows three sibling Aerisyn packages; a true parent folder is not available without merging packages, and merging would force games to take packages they do not use.

## Solution

Ship a coordinated **0.4.0** cleanup: bump all three Aerisyn packages to the same version for repo cadence while keeping them separately installable. Remove the DevHost `AerisynDataConfig` scaffold and stop baking those paths into Pull Config defaults (empty secrets and output folder; token stays under UserSettings). Add per–Config Type **Include In Pull** as a bare checkbox on the Pull Config list. Add an Inspector **Pull** button that uses the same pull runner path as the existing menu items (Selected Config and All Configs). Docs and the Weapons Pull sample are retargeted; existing Pull Config assets are not auto-migrated (operators re-add Config Types by hand). Packaging stays three packages; no merge, no parent Packages folder, no package-id / repo / displayName renames in this release.

## User Stories

1. As a package consumer, I want to install Aerisyn Quests without installing Data Config Sheet or Tutorial, so that I only take dependencies I use.
2. As a package consumer, I want to install Data Config Sheet without Quests or Tutorial, so that spreadsheet tooling does not drag in unrelated systems.
3. As a package consumer, I want to install Tutorial without Quests or Data Config Sheet, so that onboarding scaffolding stays optional.
4. As a maintainer, I want all three packages versioned to 0.4.0 together, so that this cleanup ships as one repo-scale release cadence.
5. As a DevHost user, I want `Assets/AerisynDataConfig` gone, so that Samples are the only demo setup and the Project window is less cluttered.
6. As a DevHost user, I want leftover Credentials / Baked placeholders and the unused Demo scene removed with that tree, so that empty scaffold folders do not linger.
7. As a Pull Config author, I want new Pull Configs to start with empty client-secrets and output-folder fields, so that I am not steered toward a deleted convention path.
8. As a Pull Config author, I want the OAuth user token path to remain under UserSettings, so that machine-local auth does not live under Assets.
9. As a Pull Config author, I want validation to fail clearly when the output folder is empty, so that I know what to fill before Pull.
10. As a Pull Config author, I want validation to fail clearly when credential paths required for the chosen auth mode are empty, so that live Google Pull does not fail cryptically.
11. As a designer, I want a checkbox beside each Config Type on the Pull Config, so that I can leave types listed but skip them for this Pull.
12. As a designer, I want that checkbox to show without a long “Include In Pull” label, so that the list stays compact and readable.
13. As a designer, I want a newly added Config Type to start with its checkbox on, so that the default behavior matches “I added this because I want it Pulled.”
14. As a designer, I want Pull to error clearly when every Config Type checkbox is off, so that I do not get a silent no-op.
15. As a designer, I want unticked Config Types to be skipped for Google fetch as well as bake, so that unused tabs are not downloaded.
16. As a designer, I want unticked Config Types to be skipped on inject/fixture Pull as well, so that offline tests and live Pull share the same inclusion rules.
17. As a designer, I want to keep owned Config Types on the list even when temporarily unticked, so that I do not lose my candidate set between runs.
18. As a designer, I want a Pull button on the Pull Config Inspector, so that I can run Pull without hunting the menu bar.
19. As a designer, I want the Inspector Pull button to behave like menu Pull for the same asset, so that I am not learning two workflows.
20. As a designer, I want Sign In / Sign Out to stay menu-only for this release, so that Inspector chrome stays minimal.
21. As a designer, I want “Pull From Google (Selected Config)” to keep working, so that keyboard/menu habits still work.
22. As a designer, I want “Pull From Google (All Configs)” to keep working, so that I can refresh every Pull Config in the project in one go.
23. As a designer, I want All Configs Pull to honor each Pull Config’s own Include In Pull ticks, so that bulk Pull does not ignore per-config selection.
24. As a maintainer, I want menu and Inspector entry points to call one shared pull path into the runner, so that auth, validation, fetch, parse, and bake cannot diverge.
25. As a maintainer, I want no silent serialization migrate from the old bare type list, so that 0.4.0 stays a clean break and sample assets are re-authored deliberately.
26. As a sample author, I want the DevHost Weapons Pull PullConfig cleared of stale AerisynDataConfig paths, so that the sample does not point at deleted folders.
27. As a sample author, I want to re-add Config Types on that sample by hand after the list shape change, so that cleanliness beats convenience migration.
28. As a first-time sample user, I want the Weapons Pull README to show one concrete sample-local path example for credentials and Baked output, so that empty defaults are not guesswork.
29. As a package reader, I want the package README and related DevHost docs updated away from Assets/AerisynDataConfig, so that documentation matches the product.
30. As a maintainer, I want .gitignore rules that only existed for Assets/AerisynDataConfig Credentials cleaned up or retargeted, so that ignore rules match reality (global secret ignores may remain).
31. As a DevHost user, I want imported Samples under Assets/Samples to remain the place I look for demos, so that package samples stay the source of truth.
32. As a Project window user, I accept three sibling Aerisyn package entries for this release, so that selective install is preserved.
33. As a product owner, I want package displayNames left unchanged in 0.4.0, so that rename churn is deferred.
34. As a product owner, I want package ids (`com.aerisyn.*`) left unchanged in 0.4.0, so that consumer manifests do not break.
35. As a product owner, I want the git repo name left as Aerisyn Moon for now, so that remote URL and install-link churn wait for a later pass.
36. As a future planner, I want umbrella-merge and “Aerisyn Archive” repo rename explicitly out of this spec, so that 0.4.0 stays focused.
37. As a QA engineer, I want automated tests at the pull runner inclusion/validation seam, so that tick behavior is locked without testing Odin widgets.
38. As a QA engineer, I want tests to prove mixed ticks only process included Config Types, so that skip behavior is regression-proof.
39. As a QA engineer, I want tests to prove all-unticked fails before write, so that the clear-error rule cannot silently regress.
40. As a runtime game, I want Baked Asset GUID stability on re-Pull of included types unchanged, so that references keep working (ADR 0010).
41. As a sheet author, I want Vertical Nest / tab naming / Column Alias behavior unchanged for included types, so that this release is UX and cleanup, not a parse rewrite.
42. As a CI maintainer, I want service-account auth to keep working when paths are filled, so that headless Pull remains available.
43. As an OAuth user, I want Sign In / Sign Out menu items unchanged, so that browser auth flow is untouched except for empty default paths.
44. As a consumer reading CHANGELOGs, I want each package’s 0.4.0 notes to describe what changed for that package, so that Quests/Tutorial bumps are honest about cadence-only vs Data Config’s UX work.
45. As an agent implementing tickets, I want this spec marked ready-for-agent, so that `/to-tickets` and `/implement` can proceed without re-triage.

## Implementation Decisions

- Keep three independently installable UPM packages; do not merge into one umbrella; do not invent a parent folder under Packages.
- Bump Quests, Data Config Sheet, and Tutorial package versions to 0.4.0 for coordinated repo cadence; Quests and Tutorial need not gain features beyond version/docs alignment required by that bump.
- Delete the DevHost AerisynDataConfig asset tree (Credentials placeholders, Baked placeholders, Demo scene, local README) as part of cleanup.
- Pull Config field defaults for OAuth client secrets, service-account JSON, and output folder become empty strings; OAuth user token default remains under UserSettings (existing AerisynDataConfig token folder name may stay to avoid breaking existing local tokens).
- Replace the bare Config Types type array with a list of entries: each entry holds a Config Type plus an Include In Pull flag (domain name). Inspector draws a bare checkbox (no long label text) plus the type picker.
- New list entries default Include In Pull to true.
- Pull inclusion is resolved once and used by validation, Google grid fetch, and inject PullFromGrids so live and fixture paths cannot disagree.
- Validation: empty candidate list still errors; zero included (all unticked) errors clearly and does not Pull; empty output folder still errors; Google-path validation still requires spreadsheet id and filled credential paths for live Pull.
- Expose or share one editor helper used by Inspector Pull and both menu Pull commands so there is a single user-facing pull workflow into the runner.
- Inspector gains a Pull control only; do not add Sign In / Sign Out buttons in this release.
- No silent serialization migrate from the old type array; treat 0.4.0 as a clean break; re-author sample Pull Config types manually.
- Update package and sample documentation to remove AerisynDataConfig path guidance; include one concrete sample-local example path for first-time setup.
- Retarget or remove DevHost .gitignore entries that only existed for the deleted Credentials folder; keep global secret filename ignores.
- Do not change package ids, git remote/repo name, or displayNames in this release.
- Do not revise ADR 0010’s architecture; this is workflow cleanup on top of Pull → ScriptableObject.
- Glossary already records Include In Pull; keep that term in docs even when the Inspector checkbox has no long label.

## Testing Decisions

- Good tests assert external Pull behavior only: which Config Types are validated/fetched/parsed/written, and which error conditions fire. Do not assert Odin drawer layout, checkbox chrome, or menu vs button wiring beyond the shared runner contract.
- Primary seam: pull runner inclusion resolution and ValidateTargets (and PullFromGrids behavior for mixed / zero inclusion). Prefer this existing high seam over new modules.
- Cover: mixed Include In Pull → only included types processed; all unticked → clear failure, no writes; empty output folder still fails; candidates still must be concrete Config Type assets.
- Prior art: Data Config Sheet `Tests~/VerticalNest.Tests` (inject PullFromGrids documentation path, baked overwrite helpers, Vertical Nest fixtures). Extend that suite rather than inventing a parallel test stack.
- Inspector button and menu aliasing are manual verification (same shared helper → runner).
- Deleting AerisynDataConfig, version bumps, and doc path edits are checklist / smoke verification, not unit-tested.

## Out of Scope

- Merging packages into one “Aerisyn Moon” UPM package or any meta-package umbrella.
- Renaming package ids (`com.aerisyn.*`).
- Renaming the git repository (Aerisyn Moon → Aerisyn Archive) or updating remote install URLs for a rename.
- Changing displayNames (e.g. Aerisyn Data Sheets / Aerisyn Tutorials).
- Silent migrate of old Pull Config Config Types arrays.
- Inspector Sign In / Sign Out controls.
- Changes to Vertical Nest parsing, Column Alias, SheetTab, Ignore Marker, or Baked Asset GUID policy beyond respecting Include In Pull.
- New cross-package dependencies between Quests, Data Config, and Tutorial.
- Publishing to a scoped registry or changing git `?path=` distribution model.
- Prototype or redesign of Package Manager “install features” UX beyond keeping three packages.

## Further Notes

- Selective install overrides Project-window aesthetics: three sibling Packages entries remain by design.
- “Cinemachine-like” optional modules are not UPM pick-and-choose across products; optional compile integrations are unrelated to this cleanup.
- After tickets are cut, human still re-adds Config Types on the DevHost sample Pull Config once the list shape lands (cleanliness over migrate).
- Deferred rename work should not be sneakily partially applied in 0.4.0 docs titles.
