# 02: Pull Config feature key + title check

**What to build:** Pull Config stores a feature key (picker) instead of a hard-coded spreadsheet id. At Pull start, resolve id from the Environment Profile, fetch Source Sheet title, require the active environment’s prefix (`[Dev]`, `[Tester]`, `[Production]` by default; overridable per env key). Mismatch aborts before parse/write.

**Blocked by:** 01

**Status:** resolved

- [x] Pull Config uses feature key picker; spreadsheet id field removed or hidden as obsolete
- [x] Pull resolves spreadsheet id via active Environment Profile
- [x] Title check runs before grid fetch/parse; abort message names expected prefix and actual title
- [x] Default prefixes match Dev→`[Dev]`, Test→`[Tester]`, Production→`[Production]`
- [x] CHANGELOG notes breaking Pull Config shape for 0.6.0
- [x] Pure tests for prefix match; inject seam test for abort-on-mismatch without live Google

## Comments

- Pull Config references an Environment Profile and stores a Feature Key; the active environment stays on the profile. `SpreadsheetId` removed from `PullConfig`.
- Title Check seams: `TitleCheckRules` (prefix match + defaults + per-key overrides) and `SourceSheetTitleGuard` (title read before grid fetch, injected fetchers). Custom Environment Keys without an override prefix fail the Pull. No opt-out for exception sheets yet (spec allows it later).
