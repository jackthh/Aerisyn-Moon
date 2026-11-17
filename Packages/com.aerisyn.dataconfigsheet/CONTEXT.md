# Data Config

Domain language for `com.aerisyn.dataconfigsheet`: Google-authored game tables pulled into Unity ScriptableObject assets for other projects to reuse.

## Language

**Pull**:
One editor pass that signs into Google, downloads the configured tabs, parses nested rows into config data, and writes or refreshes ScriptableObject assets. One-way only. Re-Pull overwrites data on an existing Baked Asset in place so the asset GUID (and references) stay stable. Local Only fields are not read from the sheet; games refill them in After Pull when needed.
_Avoid_: Sync; Bake (Luban-era name for this pass); Export as the everyday name for the whole pipeline; delete-and-recreate of Baked Assets on each Pull; merging prior Inspector values for Local Only fields across re-Pull

**Config Type**:
A hand-written Odin `SerializedScriptableObject` subclass (plus nested serializable types) the game owns. It defines field names, nesting, and data types. Developers control shape here; the package does not generate these types. Pull creates or fills the `.asset` file only. Nest shape for Vertical Nest comes from the type (type-driven parse), not from a second schema language.
_Avoid_: DTO as the product name when the type is the schema itself; plain POCOs behind a package SO shell as the default; Luban bean; BakingSheet SheetRow; requiring only Unity ScriptableObject when Odin nested inspector UX is the template default

**Baked Asset**:
The ScriptableObject asset file produced or refreshed by a Pull. One asset per matching Source Sheet tab. All assets for one Pull Config land in that config’s single output folder; the file name follows the tab / Config Type naming convention. Runtime games load these assets; they are the runtime final.
_Avoid_: Tables (Luban); generated JSON as the default runtime final; treating the Google sheet as a runtime dependency; one asset per top-level row as the default; per-tab output path fields

**Source Sheet**:
The Google Spreadsheet (and its tabs) that designers edit. Official source of truth for sheet-owned values (Field Headers that are not Local Only), not for C# type shape or Local Only fields. Pull reads cell values via the Sheets API (in memory); CSV is not required for a correct Pull.
_Avoid_: Bridge file as the source of truth; local CSV as the official authoring surface; naive comma-split CSV as the parse path; treating every Config Type field as sheet-owned

**Vertical Nest**:
Sheet authoring where a parent identity is written once and child rows leave parent cells blank until the next parent value. Used for nested lists such as weapon → upgrade levels → bonus stats.
_Avoid_: Compact list#sep cell blobs; one normalized tab per nest level as the default; Luban ##var/##type as the schema

**Pull Config**:
Editor settings for one Pull job: Environment Profile reference, Feature Key, auth, one shared output folder, and an explicit list of owned Config Types (candidates). Only candidates with Include In Pull run on a Pull. Tab ↔ type pairing is by exact type name, or an optional `[SheetTab]` attribute on the type when the Google tab title must differ. All Baked Assets for that config write into the one output folder.
_Avoid_: Bake Config; Luban project paths; treating Pull Config as the schema; keeping a parallel Luban pipeline in this package; repeating asset folder per tab; assembly-wide auto-scan of every Config Type; suffix-guessing tab names; treating the full candidate list as always-pulled with no per-type gate

**Environment Key**:
A named stage (seeded: Dev, Test, Production; games may add more) defined once in the Environment Key list asset. Every other asset or Inspector only picks an Environment Key from that list; none accepts a free-typed environment string. Matching is case-sensitive.
_Avoid_: Free-typed environment strings on a Pull Config or profile row; per-asset copies of the environment name list

**Feature Key**:
A game-owned name for one Source Sheet feature (e.g. `ai_ship`, `cannon`), defined once in the Feature Key list asset. The package ships it empty. Environment Profile rows and Pull Config pick a Feature Key from that list; none accepts a free-typed feature string.
_Avoid_: Seeding game-specific Feature Keys in the package; free-typed feature strings; storing a spreadsheet id where a Feature Key belongs

**Environment Profile**:
A ScriptableObject that maps each Environment Key × Feature Key to a Source Sheet spreadsheet id and stores the one active Environment Key for the project. It also holds the Config Manifest asset path and the required Config Type list. Pull resolves a spreadsheet id from the active environment and a Feature Key; an unbound pair or a key outside its list fails clearly.
_Avoid_: A per-Pull-Config active environment; silently falling back to another environment when a Feature Key has no id in the active one

**Title Check**:
A guard at Pull start: read the Source Sheet file title and require it to start with the active Environment Key's prefix (case-sensitive, brackets included). Defaults: Dev → `[Dev]`, Test → `[Tester]`, Production → `[Production]`; the Environment Profile can override a prefix per key, and a game-added key needs one. A mismatch aborts the Pull before any cell fetch, parse, or write and names the expected prefix and the actual title. The spreadsheet id stays the source of truth; the title only guards against a wrong id.
_Avoid_: Treating the title as the lookup key; silently skipping the check for an Environment Key with no prefix; case-insensitive or trimmed matching

**Pull All**:
The menu pass that Pulls every Pull Config in stable asset-path order. It never stops at the first failure: one summary lists each passed and failed config, and the overall result fails if any config failed or the Config Manifest check finds a problem. Include In Pull on each config still applies.
_Avoid_: Stopping at the first failing config; one dialog per config; reporting success when no Pull Config ran

**Config Manifest**:
A ScriptableObject written after a Pull All in which every config passed, at the path set on the Environment Profile. It records the active Environment Key, a UTC timestamp, per Feature Key the Source Sheet id, title and Drive `modifiedTime` / `version` (empty when Drive is unavailable), and a content hash per Baked Asset. A completeness check compares it to the profile's required Config Type list and the active environment; a missing Config Type or wrong environment fails. Pull All overwrites it; a failed Pull All leaves the previous one. A game may opt in to failing player builds when the active environment is not Production or the manifest is incomplete.
_Avoid_: Treating it as runtime data; hand-editing it; writing it after a partially failed Pull All; failing a Pull because Drive revision fields are unavailable

**Include In Pull**:
Per Config Type flag on a Pull Config. When true, that type is fetched and baked on the next Pull; when false, it stays listed but is skipped. New candidates default true. A Pull with none included is an error, not a no-op.
_Avoid_: Enabled as a vague name; removing a type from the list just to skip one Pull; silent skip when every flag is off

**Field Header**:
A sheet column named for a C# field (e.g. `id`, `upgrade_level`, `bonus_stats`), not for a type or collection name. Canonical header contract. Vertical Nest uses blank parent cells.
_Avoid_: PascalCase collection columns (`Weapons`, `UpgradeLevels`) as the required contract

**Column Alias**:
An optional `[ColumnAlias]` attribute on a Config Type field that lets the Header Row use a designer-friendly name (e.g. `Weapons`) while the C# field stays `id`. Header detection and parsing accept the field name or the alias.
_Avoid_: Making aliases required; a second schema file just to rename columns

**Ignore Marker**:
The cell value `!!!` in the marker row above a Field Header (or Column Alias). That column is designer-only notes; Pull skips it for parsing. Use when the Source Sheet has extra columns beyond the Config Type contract.
_Avoid_: `!!` as the marker; using note columns as nest keys or runtime fields; Local Only (that is for extra fields on the Config Type / Baked Asset, not extra sheet columns)

**Local Only**:
A Config Type field that is not part of the Source Sheet contract. Pull does not require it on the Header Row and does not bind sheet cells into it. Games own these values on the Baked Asset, typically as derivatives filled in After Pull.
_Avoid_: IgnorePull; NonSerialized as the sheet opt-out; Ignore Marker; assuming Inspector edits on these fields survive re-Pull without After Pull refill

**After Pull**:
A Config Type hook that runs after every included tab parses successfully and before any Baked Asset is created or saved. Games use it to fill Local Only fields (and similar) so derived data is written into the asset. Failure fails the whole Pull with no writes.
_Avoid_: Relying on OnValidate or OnValueChanged to run automatically after Pull; package-owned derivative formulas

**Preamble**:
Human-only rows above the Header Row (instructions, navigation text, Ignore Marker row). Pull must not treat Preamble as data.
_Avoid_: Requiring designers to keep sheets preamble-free; treating row 1 as always the header

**Header Row**:
The row that starts the parsable table. Pull finds it by matching cell values to the Config Type’s non-Local Only Field Headers and Column Aliases. Rows above it are Preamble; data rows follow it.
_Avoid_: Assuming the first sheet row is always the header; requiring a fixed header row index in Pull Config; requiring Local Only fields as headers
