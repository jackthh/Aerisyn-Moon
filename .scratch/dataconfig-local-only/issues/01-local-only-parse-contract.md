# 01: Local Only parse contract

**What to build:** A developer can mark Config Type fields as Local Only so Pull succeeds when the Source Sheet omits those columns. Marked fields are never filled from sheet cells; if a matching Field Header or Column Alias still appears on the sheet, Pull warns (including alias text when set) and ignores the column. Unmarked missing headers still fail Header Row match. Works at any nest level for scalars and primitive arrays/lists.

**Blocked by:** None (can start immediately).

**Status:** ready-for-human

- [x] `[LocalOnly]` is available on Config Type fields and is the documented name (not IgnorePull / NonSerialized)
- [x] Header Row match succeeds when Local Only columns are absent from the sheet (including nest-level Local Only fields)
- [x] Parse never binds into Local Only fields (default/empty after parse even if a matching column exists)
- [x] A present Local Only column (name or Column Alias) produces a warning that can mention the alias; Pull does not hard-fail for that alone
- [x] Unmarked missing required Field Headers still fail Header Row match (regression)
- [x] Fixture/tests at the Vertical Nest `ParseInto` seam lock the above without live Google

## Comments

- Implemented on parse seam: `LocalOnlyAttribute`, `FieldHeaderNames.IsLocalOnly`, Header Row skip + never-bind + `VerticalNestParseResult.Warnings`. Fixture: `LocalOnlyParserTests` (8 cases). Full `VerticalNest.Tests` green (31).
