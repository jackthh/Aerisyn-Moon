# Data Config Sheet — Pull → SO Spec

Product spec for `com.aerisyn.dataconfigsheet` after ADR 0010. Domain terms live in [`CONTEXT.md`](./CONTEXT.md). This file is the implementation contract; change the glossary when terms change, then update this spec.

## Goal

Ship an installable Unity package template: designers edit Google Sheets; developers hand-write Config Types; Editor Pull fills ScriptableObject assets. Nested real data must work. Per-tab wiring stays small. Newcomers see one workflow.

## Non-goals (v1)

- Two-way sync (never push SO edits back to Google)
- Luban / BakingSheet / required CSV bridge
- Cloning ZBase CsvReader wholesale
- Assembly-wide auto-scan of every Config Type
- Dry-run mode
- One `.asset` per top-level row
- Runtime download from Google in player builds

## User journeys

### Developer: add a config table

1. Create an Odin `SerializedScriptableObject` Config Type with nested serializable types and an `items` (or equivalent) list field.
2. Optionally add Column Alias attributes and/or a tab-override attribute when the Google tab title must differ from the type name.
3. Add the type to a Pull Config’s explicit type list (spreadsheet id, OAuth paths, one output folder already set).
4. Create a Google tab titled exactly as the type name (or the override).
5. Use Header Emitter (or hand-edit) so the Header Row matches Field Headers / aliases; add Preamble and `!!!` note columns as needed.
6. Sign In With Google → Pull.
7. Open the Baked Asset under the shared output folder; runtime loads that asset.

### Designer: edit values

1. Edit cells under the Header Row using Vertical Nest (blank parent cells continue the nest).
2. Put human instructions in Preamble rows above the Header Row.
3. Mark note-only columns with `!!!` in the marker row above the header.
4. Ask a developer to Pull (or Pull themselves if they have OAuth).

## Pull Config

| Field | Rule |
|---|---|
| Spreadsheet id | One Google spreadsheet document |
| Auth | OAuth user happy path; service account may remain for CI later, not required for v1 docs |
| Output folder | Single folder for all Baked Assets from this config |
| Config Types | Explicit list; no assembly auto-scan |
| Tab match | Exact Config Type name, unless tab-override attribute is set |
| Asset file | `{TypeName}.asset` (or stable derived name) inside the output folder |
| Re-Pull | Overwrite data in place; keep asset GUID |

Rename editor menus and types from Bake → Pull where user-facing.

## Sheet contract

1. **Preamble**: any rows above the Header Row are ignored as data (instructions, navigation, Ignore Marker row).
2. **Ignore Marker**: cell value exactly `!!!` in the row immediately above the Header Row marks that column as notes-only; Pull skips it.
3. **Header Row**: found by matching cell values to the Config Type’s Field Headers and Column Aliases (not by fixed row index).
4. **Field Headers**: canonical names follow C# fields (e.g. `id`, `upgrade_level`, `bonus_stats`). Collection PascalCase names are not required.
5. **Column Alias**: optional; designer-friendly header accepted in addition to the field name.
6. **Vertical Nest**: blank parent cell continues the current parent; non-empty parent starts a new element. Nest shape comes from the Config Type (arrays of structs expand to child field columns; primitive arrays use one vertical column).
7. **Data rows**: follow the Header Row until the used range ends.

Transport: Google Sheets API → in-memory grid → parse. CSV is not required for a correct Pull. Optional debug dump may exist later; it must not be the parse path (Idle comma breakage).

## Parser

- Owned, type-driven Aerisyn Vertical Nest parser.
- Fixture tests (checked-in sheet grids → expected object graphs), including a weapons → upgrade levels → bonus stats case with Preamble and `!!!`.
- Pull report on failure: human-readable errors with sheet tab + cell coordinates when possible.

## v1 extras

| Feature | Requirement |
|---|---|
| Header Emitter | From Config Type → clipboard or log of Header Row (+ guidance for `!!!` note columns) |
| Pull report | Success summary + per-failure cell coords |

## Cleanup

Remove from the package and DevHost sample anything that teaches the Luban/BakingSheet path: Luban runner fields, Luban project sample as the primary demo, obsolete Bake naming where it confuses, leftover 0.2.0 SO bake assets that imply the old stack. Keep OAuth Google access code where it still serves Pull.

## Acceptance (template ready)

- [ ] Fresh reader of CONTEXT + this SPEC + README can explain the one workflow without mentioning Luban.
- [ ] DevHost (or Samples) shows nested weapons-style Pull into an SO under one output folder.
- [ ] Fixture tests cover Vertical Nest, Preamble, `!!!`, and Field Header / alias matching.
- [ ] Re-Pull keeps asset GUID; commas inside cells do not break Pull.
- [ ] Per new table: new Config Type + list entry on Pull Config + matching Google tab + headers; output folder assigned once.
