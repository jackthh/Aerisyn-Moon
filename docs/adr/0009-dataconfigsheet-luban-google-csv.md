# DataConfigSheet: Luban via Google → CSV (abandoned spike)

Editor bake uses **Google Sheets (OAuth) → CSV files → Luban CLI** (`cs-simple-json` + `json`). Runtime final for now is Luban-generated `Tables` + JSON (not editable ScriptableObjects). Schema field definitions live in Google sheet headers (`##var` / `##type`, including nested `*` multi-row lists) with tables registered via XML/`readSchemaFromFile`. BakingSheet is removed.

**Status:** superseded by [ADR 0010](0010-dataconfigsheet-pull-scriptableobject.md). Never shipped as the product `0.3.0`; shipping `0.3.0` is Pull → ScriptableObject (ADR 0010).

**Considered options:** keep BakingSheet + PostLoad regroup for nested sheets; dual BakingSheet+Luban forever; Luban with Excel-only (no Google).

**Why (historical):** BakingSheet cannot nest vertical lists; Luban can. Google remains source of truth (ADR 0007 OAuth kept). CSV is the bridge because Luban has no first-class Google importer. Editable SO (0008) may return later; spike prioritizes nested authoring + load.

**Revisit when:** see ADR 0010.
