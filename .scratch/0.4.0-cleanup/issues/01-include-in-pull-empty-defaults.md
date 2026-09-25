# 01: Include In Pull + empty Pull Config defaults

**What to build:** A Pull Config lists Config Types as owned candidates, each with a bare Include In Pull checkbox (no long label). New candidates default included. Pull (live Google and inject) only processes included types; all unticked or empty output folder fails with a clear error and writes nothing. New Pull Configs start with empty client-secrets and output-folder fields; the OAuth user token default stays under UserSettings. Existing sample Pull Config is cleaned of stale AerisynDataConfig paths and old type-list data so types can be re-added by hand. Automated tests at the pull runner seam cover mixed ticks and all-unticked failure.

**Blocked by:** None (can start immediately).

**Status:** resolved

- [x] Config Types on a Pull Config are candidates with a per-type Include In Pull flag drawn as a bare checkbox
- [x] Newly added candidates default to included
- [x] Live and inject Pull only fetch/parse/bake included Config Types
- [x] Zero included Config Types → clear error, no Pull writes
- [x] Empty output folder → clear error (unchanged rule, still enforced)
- [x] New Pull Config defaults: empty client secrets, empty service-account path, empty output folder; token path remains under UserSettings
- [x] No silent migrate from the old bare type list; sample Pull Config cleared for manual re-add
- [x] Runner-seam tests: mixed Include In Pull only processes included types; all unticked fails before write

## Answer

Shipped Include In Pull on `PullConfigTypeEntry` (bare checkbox, default true), empty Pull Config path defaults (token stays under UserSettings), shared `PullTargetRules` for validation/inclusion on live fetch + inject, cleared DevHost Weapons Pull `PullConfig.asset`. Tests in `PullTargetRulesTests` cover mixed inclusion and all-unticked / empty-folder Validate failures (no-write gate).
