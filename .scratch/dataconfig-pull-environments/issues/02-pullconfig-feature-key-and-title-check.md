# 02: Pull Config feature key + title check

**What to build:** Pull Config stores a feature key (picker) instead of a hard-coded spreadsheet id. At Pull start, resolve id from the Environment Profile, fetch Source Sheet title, require the active environment’s prefix (`[Dev]`, `[Tester]`, `[Production]` by default; overridable per env key). Mismatch aborts before parse/write.

**Blocked by:** 01

**Status:** ready-for-agent

- [ ] Pull Config uses feature key picker; spreadsheet id field removed or hidden as obsolete
- [ ] Pull resolves spreadsheet id via active Environment Profile
- [ ] Title check runs before grid fetch/parse; abort message names expected prefix and actual title
- [ ] Default prefixes match Dev→`[Dev]`, Test→`[Tester]`, Production→`[Production]`
- [ ] CHANGELOG notes breaking Pull Config shape for 0.6.0
- [ ] Pure tests for prefix match; inject seam test for abort-on-mismatch without live Google

## Comments
