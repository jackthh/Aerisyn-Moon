# 03: Live Google Pull + aliases, tab override, Header Emitter

**What to build:** A developer Signs In with Google, Pulls Source Sheet tabs into an in-memory cell grid (no CSV required), and fills Baked Assets through the same parse path as 02. Tabs match Config Type names by default; optional tab-override covers mismatched titles; optional Column Alias covers designer-friendly headers. A Header Emitter outputs a Header Row from a Config Type for pasting into Google. Commas inside cells do not break Pull.

**Blocked by:** 02 — Nested Vertical Nest → Baked Asset (fixture-backed)

**Status:** ready-for-agent

- [ ] OAuth → Sheets API → in-memory grid → parse → Baked Asset works for listed Config Types
- [ ] Default tab title equals Config Type name; optional tab-override attribute works when titles differ
- [ ] Column Alias is accepted on Header Row match and parse alongside Field Headers
- [ ] Header Emitter produces a usable Header Row (and guidance for `!!!` note columns) from a Config Type
- [ ] Cell values containing commas do not break Pull (CSV is not the required parse path)
