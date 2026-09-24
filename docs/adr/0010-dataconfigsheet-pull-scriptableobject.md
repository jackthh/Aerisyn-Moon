# DataConfigSheet: Pull → ScriptableObject (post-0.3.0)

Editor workflow is **OAuth Google Sheets → in-memory cell grid → type-driven Vertical Nest parse → ScriptableObject Baked Assets**. Hand-written Odin `SerializedScriptableObject` Config Types own schema shape. Luban and CSV-as-required-bridge are removed from this package.

**Status:** accepted (supersedes [ADR 0009](0009-dataconfigsheet-luban-google-csv.md); revives editable SO runtime final in the spirit of [ADR 0008](0008-dataconfigsheet-editable-single-so.md); keeps Google-as-source-of-truth and OAuth from [ADR 0005](0005-dataconfigsheet-one-way-google-bake.md) / [ADR 0007](0007-dataconfigsheet-oauth-primary.md))

**Considered options:** keep Luban `Tables`+JSON; Idle/ZBase CsvReader clone with CSV bridge; hybrid Luban+SO.

**Why:** The installable template must feel like Idle’s proven product flow (hand-written types, vertical nested sheets, one-click Pull into SOs) without Idle’s CSV comma breakage or Luban’s per-table XML/`##var` tax. Nesting is solved by a small type-driven parser over Sheets API cells, not by Luban. Schema stays in C# so games control types and names.

**Revisit when:** a game needs codegen `Tables`/bin for shipping size; or two-way sheet sync is required.
