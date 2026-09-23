# DataConfigSheet: one-way Google → ScriptableObject bake

Google Spreadsheet is the official source of truth. Editor bake uses BakingSheet (`com.cathei.bakingsheet`) to pull into optional CSV cache and ScriptableObjects. Local SOs are the runtime final data and may be tweaked for temporary tests. ScriptableObject edits are never pushed back to Google Sheets.

**Considered options:** vendor/fork BakingSheet; port IdleLotl CSV downloader; two-way sync; runtime Google download in player builds.

**Why:** BakingSheet already owns Google + ScriptableObject converters. Nesting the dependency (pin v4.1.3) and documenting a DevHost/consumer manifest pin avoids flaky Unity git→git resolution without copying upstream. Schema (`Sheet` / `SheetContainer`) stays in the game so this package remains bake tooling only.

**Revisit when:** two-way sync is an explicit product requirement; or BakingSheet is abandoned and a replacement converter stack is chosen.
