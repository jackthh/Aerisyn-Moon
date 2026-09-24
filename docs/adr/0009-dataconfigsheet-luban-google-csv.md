# DataConfigSheet: Luban via Google → CSV (0.3.0)

Editor bake uses **Google Sheets (OAuth) → CSV files → Luban CLI** (`cs-simple-json` + `json`). Runtime final for now is Luban-generated `Tables` + JSON (not editable ScriptableObjects). Schema field definitions live in Google sheet headers (`##var` / `##type`, including nested `*` multi-row lists) with tables registered via XML/`readSchemaFromFile`. BakingSheet is removed.

**Status:** accepted (supersedes ADR 0005 converter choice; pauses ADR 0008 editable-SO as the default bake output)

**Considered options:** keep BakingSheet + PostLoad regroup for nested sheets; dual BakingSheet+Luban forever; Luban with Excel-only (no Google).

**Why:** BakingSheet cannot nest vertical lists; Luban can. Google remains source of truth (ADR 0007 OAuth kept). CSV is the bridge because Luban has no first-class Google importer. Editable SO (0008) may return later; spike prioritizes nested authoring + load.

**Revisit when:** editable SO prototyping is required again; or Google→Luban without CSV proves necessary.
