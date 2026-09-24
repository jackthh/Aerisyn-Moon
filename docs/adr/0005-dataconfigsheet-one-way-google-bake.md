# DataConfigSheet: one-way Google → ScriptableObject bake

**Status:** superseded by [ADR 0009](0009-dataconfigsheet-luban-google-csv.md) for the converter stack (BakingSheet → Luban). One-way Google-as-source-of-truth still holds.

Google Spreadsheet is the official source of truth. Editor bake originally used BakingSheet (`com.cathei.bakingsheet`) to pull into optional CSV cache and ScriptableObjects. Local SOs were the runtime final data and may be tweaked for temporary tests. ScriptableObject edits are never pushed back to Google Sheets.

**Considered options:** vendor/fork BakingSheet; port IdleLotl CSV downloader; two-way sync; runtime Google download in player builds.

**Why (historical):** BakingSheet already owned Google + ScriptableObject converters. Nesting the dependency (pin v4.1.3) avoided flaky Unity git→git resolution without copying upstream.

**Revisit when:** see ADR 0009; two-way sync remains out of scope unless explicitly required.

