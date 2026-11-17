Status: ready-for-agent

# Spec: Safer Pull (environments + manifest)

Feature slug: `dataconfig-pull-environments`  
Package: `com.aerisyn.dataconfigsheet` (shipping 0.5.2 → 0.6.0)  
Context: Data Config  
Builds on: ADR 0010 (Pull → SO), existing Pull All menu

## Problem Statement

Games often keep several Pull Config assets, each with a hard-coded Source Sheet id. Environment is only a Drive naming habit (`[Dev]`, `[Tester]`, `[Production]`). Pull All already runs every Pull Config but stops on the first failure and writes no combined summary. Nothing records which environment was active, which sheet revision was read, or which Baked Assets were written. Needed config can stay unpulled without a clear fail, and the wrong sheet can be pulled if an id is pasted into the wrong asset.

## Solution

Ship one feature that makes environment explicit and verifiable:

1. **Key lists (single definition, pick elsewhere).** One place defines environment keys; one place defines feature keys. Inspectors and assets only pick from those lists. Package seeds environment keys **Dev**, **Test**, **Production**. Games may add more. Feature keys ship empty (or sample-only); games define their own (e.g. ai_ship, cannon). No free-typed env/feature strings on Pull Config or the profile map.
2. **Environment profile.** A ScriptableObject maps, per environment, each feature key → Source Sheet spreadsheet id. The project picks one active environment. Each Pull Config stores a feature key (not a spreadsheet id). Resolve id from active environment + feature key at Pull time.
3. **Title check.** Before fetch/parse, read the Source Sheet title. Compare it to the expected prefix for the active environment. Package defaults: `[Dev]`, `[Tester]` (for **Test**), `[Production]`. Mismatch aborts that Pull. Spreadsheet id remains the source of truth; title is a guard only. Allow documented exceptions such as `[Production]_CONVENTIONS…` when the game opts that sheet out of the check or uses a non-feature Pull (out of default path).
4. **Hardened Pull All.** Find every Pull Config, run in stable order, continue or stop per policy below, print one summary of pulled and failed. Include In Pull on each config still applies.
5. **Bake manifest.** After a successful Pull All (and optionally after a single Pull), write one Config Manifest asset: active environment, timestamp, per feature (sheet id, title, Drive `modifiedTime` / `version` when available), and a content hash per written Baked Asset. An editor test or build helper compares the manifest to a required Config Type list and fails on missing entries or wrong environment.

OAuth already requests `spreadsheets.readonly` and `drive.readonly` for user and service account. Title + Drive metadata need no new scopes.

## User Stories

1. As a developer, I want environment keys defined in one list, so that Pull Config, profile, title check, and manifest never disagree on env names.
2. As a developer, I want Dev, Test, and Production seeded by the package, so that common studios start without inventing names.
3. As a developer, I want to add custom environment keys to that same list, so that studios with more stages are not blocked.
4. As a developer, I want feature keys defined in one list and picked on Pull Config and the profile map, so that spreadsheet binding cannot drift by typo.
5. As a developer, I want a Pull Config to store a feature key instead of a spreadsheet id, so that switching environment does not require editing every Pull Config.
6. As a developer, I want one active environment for the project, so that Pull and Pull All resolve ids from one place.
7. As a developer, I want Pull to abort when the Source Sheet title lacks the expected environment prefix, so that a wrong id cannot silently bake Production data into a Dev pull (or the reverse).
8. As a developer, I want default prefixes `[Dev]`, `[Tester]`, `[Production]` kept, so that existing Drive folders keep working.
9. As a developer, I want Pull All to report every Pull Config that ran, passed, or failed, so that unpulled config is visible.
10. As a developer, I want a bake manifest after Pull All, so that CI or a build step can prove environment and completeness.
11. As a package consumer, I want chestgacha and other unowned features left out of package defaults, so that the package does not invent PirateWars-only keys.
12. As a QA engineer, I want pure tests for key pick validation, title prefix match, Pull All summary aggregation, and manifest completeness without live Google.

## Implementation Decisions

- **Ticket scope (decided):** One feature, one release (0.6.0). Split into sequenced issues under this folder; do not ship half of environments without title check and manifest in the same minor.
- **Pull All failure policy:** Run all Pull Configs; collect failures; overall failure if any job failed or any required feature/type is missing from the manifest check. Do not stop at the first failure (change from today’s early return) so the summary shows the full set.
- **Key storage:** Prefer a small ScriptableObject (or pair of SOs) owned by the game project for environment keys and feature keys, with package code that seeds Dev/Test/Production when the env list is created. Editors use dropdowns bound to that list only.
- **Pull Config migration:** Replace (or deprecate and hide) the hard-coded spreadsheet id field with a feature key picker. Resolve id via active environment profile at runtime of Pull. Document breaking change in CHANGELOG for 0.6.0.
- **Title prefixes:** Configurable per environment key; defaults as above. Match is prefix on the spreadsheet **file** title from Sheets `properties.title` (or Drive `name`). Case-sensitive exact prefix including brackets.
- **Drive metadata:** Use Drive files.get with `supportsAllDrives=true` for `modifiedTime` / `version` on the manifest. Viewer access is enough.
- **Manifest path:** One project-chosen asset path (field on the environment profile or a dedicated settings asset). Overwrite on each successful Pull All.
- **Required list:** Completeness check reads an explicit required Config Type (or feature) list from settings; missing baked entry or wrong environment fails.
- **Optional Production build gate:** Package may expose a preprocessor/helper that fails player builds when active environment is not Production; wire-up stays opt-in for the game.
- **Out of this release:** Merge feature sheets into one workbook per environment; R2 remote publish; PirateWars asset migration (game repo); inventing chestgacha as a package feature key.
- Update `CONTEXT.md` with Environment Key, Feature Key, Environment Profile, Title Check, Config Manifest.
- Prefer an ADR only if a decision conflicts with ADR 0010; otherwise CONTEXT + this spec are enough for 0.6.0.

## Testing Decisions

- Assert external behavior only (inputs → accept/reject, summary text, manifest fields).
- Pure seams first: prefix match helper; key membership; resolve spreadsheet id from profile + active env + feature key; Pull All summary aggregation from fake per-config results; manifest completeness against a required list.
- No live Google in the automated suite. Mock or inject sheet title / Drive metadata at a thin fetcher seam.
- Regression: existing single-config Pull path, Include In Pull gating, and OAuth scope list remain green.

## Out of Scope

- chestgacha as a seeded feature key
- Renaming Drive sheets away from `[Dev]` / `[Tester]` / `[Production]` defaults
- One workbook per environment
- R2 / remote config publish
- Changing OAuth scopes
- Automatic migration of PirateWars Pull Config assets
- Full EditMode suite beyond targeted fixtures named in issues

## Further Notes

- Handoff origin: PirateWars design note `/tmp/handoff-config-pull-environments.md` (ideas only; implement here).
- Confirmed with user: env keys Dev/Test/Production seeded + extensible; feature keys same single-list pattern; keep title prefix defaults; leave chestgacha; one release covering Pull All + env + title check + manifest.
- Domain language: `Packages/com.aerisyn.dataconfigsheet/CONTEXT.md`.

## Comments

- Spec written after clarifying env vs feature key lists, title prefix defaults, and ticket scope.
- Review fixes landed (branch `feature/dataconfig-pull-environments-review-fixes`): Skip Title Check opt-out on Pull Config; Pull All checks required Config Types even when jobs failed; Pull All uses one Environment Profile (others reported failed, not pulled); build gate split into `Require` (Production) and `RequireCompleteManifest`; Config Manifest wording and missing XML summaries fixed. Still 0.6.0.
