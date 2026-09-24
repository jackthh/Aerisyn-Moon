# Data Config

Domain language for `com.aerisyn.dataconfigsheet`: Google-authored game tables pulled into Unity ScriptableObject assets for other projects to reuse.

## Language

**Pull**:
One editor pass that signs into Google, downloads the configured tabs, parses nested rows into config data, and writes or refreshes ScriptableObject assets. One-way only. Re-Pull overwrites data on an existing Baked Asset in place so the asset GUID (and references) stay stable.
_Avoid_: Sync; Bake (Luban-era name for this pass); Export as the everyday name for the whole pipeline; delete-and-recreate of Baked Assets on each Pull

**Config Type**:
A hand-written ScriptableObject subclass (plus nested serializable types) the game owns. It defines field names, nesting, and data types. Developers control shape here; the package does not generate these types. Pull creates or fills the `.asset` file only. Nest shape for Vertical Nest comes from the type (type-driven parse), not from a second schema language.
_Avoid_: DTO as the product name when the type is the schema itself; plain POCOs behind a package SO shell as the default; Luban bean; BakingSheet SheetRow

**Baked Asset**:
The ScriptableObject asset file produced or refreshed by a Pull. One asset per Source Sheet tab by default. Runtime games load these assets; they are the runtime final.
_Avoid_: Tables (Luban); generated JSON as the default runtime final; treating the Google sheet as a runtime dependency; one asset per top-level row as the default

**Source Sheet**:
The Google Spreadsheet (and its tabs) that designers edit. Official source of truth for values, not for C# type shape. Pull reads cell values via the Sheets API (in memory); CSV is not required for a correct Pull.
_Avoid_: Bridge file as the source of truth; local CSV as the official authoring surface; naive comma-split CSV as the parse path

**Vertical Nest**:
Sheet authoring where a parent identity is written once and child rows leave parent cells blank until the next parent value. Used for nested lists such as weapon → upgrade levels → bonus stats.
_Avoid_: Compact list#sep cell blobs; one normalized tab per nest level as the default; Luban ##var/##type as the schema

**Pull Config**:
Editor settings for one Pull job: spreadsheet id, auth, which tabs map to which Config Types / output assets.
_Avoid_: Bake Config; Luban project paths; treating Pull Config as the schema

**Field Header**:
A sheet column named for a C# field (e.g. `id`, `upgrade_level`, `bonus_stats`), not for a type or collection name. Canonical header contract. Vertical Nest uses blank parent cells.
_Avoid_: PascalCase collection columns (`Weapons`, `UpgradeLevels`) as the required contract

**Column Alias**:
An optional attribute on a Config Type field that lets the Header Row use a designer-friendly name (e.g. `Weapons`) while the C# field stays `id`. Header detection and parsing accept the field name or the alias.
_Avoid_: Making aliases required; a second schema file just to rename columns

**Ignore Marker**:
The cell value `!!!` in the marker row above a Field Header (or Column Alias). That column is designer-only notes; Pull skips it for parsing.
_Avoid_: `!!` as the marker; using note columns as nest keys or runtime fields

**Preamble**:
Human-only rows above the Header Row (instructions, navigation text, Ignore Marker row). Pull must not treat Preamble as data.
_Avoid_: Requiring designers to keep sheets preamble-free; treating row 1 as always the header

**Header Row**:
The row that starts the parsable table. Pull finds it by matching cell values to the Config Type’s Field Headers and Column Aliases. Rows above it are Preamble; data rows follow it.
_Avoid_: Assuming the first sheet row is always the header; requiring a fixed header row index in Pull Config
