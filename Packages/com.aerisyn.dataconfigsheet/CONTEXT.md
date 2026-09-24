# Data Config

Domain language for `com.aerisyn.dataconfigsheet`: Google-authored game tables pulled into Unity ScriptableObject assets for other projects to reuse.

## Language

**Pull**:
One editor pass that signs into Google, downloads the configured tabs, parses nested rows into config data, and writes or refreshes ScriptableObject assets. One-way only.
_Avoid_: Sync; Bake (Luban-era name for this pass); Export as the everyday name for the whole pipeline

**Config Type**:
A hand-written C# type the game owns that defines field names, nested structures, and data types for one table. Developers control shape here; the package does not generate these types.
_Avoid_: DTO as the product name when the type is the schema itself; Luban bean; BakingSheet SheetRow

**Baked Asset**:
The ScriptableObject asset file produced or refreshed by a Pull. Runtime games load these assets; they are the runtime final.
_Avoid_: Tables (Luban); generated JSON as the default runtime final; treating the Google sheet as a runtime dependency

**Source Sheet**:
The Google Spreadsheet (and its tabs) that designers edit. Official source of truth for values, not for C# type shape.
_Avoid_: Bridge file as the source of truth; local CSV as the official authoring surface

**Vertical Nest**:
Sheet authoring where a parent identity is written once and child rows leave parent cells blank until the next parent value. Used for nested lists such as weapon → upgrade levels → bonus stats.
_Avoid_: Compact list#sep cell blobs; one normalized tab per nest level as the default; Luban ##var/##type as the schema

**Pull Config**:
Editor settings for one Pull job: spreadsheet id, auth, which tabs map to which Config Types / output assets.
_Avoid_: Bake Config; Luban project paths; treating Pull Config as the schema

**Field Header**:
A sheet column named for a C# field (e.g. `id`, `upgrade_level`, `bonus_stats`), not for a type or collection name. Headers map to Config Type fields; Vertical Nest uses blank parent cells.
_Avoid_: PascalCase collection columns (`Weapons`, `UpgradeLevels`) as the default contract

**Ignore Marker**:
A sheet annotation (`!!` above a column in the legacy Idle flow) that marks a column as designer-only notes. Pull skips that column for parsing.
_Avoid_: Using note columns as nest keys or runtime fields
