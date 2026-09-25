# Aerisyn Data Config Sheet (`com.aerisyn.dataconfigsheet`)

Editor **Pull**: **Google Sheet (OAuth) → in-memory cell grid → type-driven Vertical Nest → ScriptableObject Baked Assets**.

That is the only supported product path for this package.

**Unity:** 2022.3+ · **Version:** `0.4.0` · **Requires:** [Odin Inspector](https://odininspector.com/), Google API Editor plugins (bundled)

See [`CONTEXT.md`](./CONTEXT.md) and [`docs/adr/0010-…`](../../docs/adr/0010-dataconfigsheet-pull-scriptableobject.md).

## Product policy

| Rule | Meaning |
|---|---|
| Google Sheet is source of truth | Cloud spreadsheet is the official authoring surface for values |
| One-way Pull only | Never push Baked Assets back to Google |
| Runtime final | ScriptableObject Baked Assets (one asset per matching tab) |
| Schema in C# Config Types | Hand-written Odin `ConfigTypeAsset` subclasses; nest shape is type-driven |
| No CSV required | Sheets API cells stay in memory; commas inside cells do not break Pull |

## Requirements

| Dependency | Why |
|---|---|
| **Odin Inspector** | `PullConfig` / `ConfigTypeAsset` drawers and serialization |
| **Google API Editor plugins** | Bundled under `Editor/Plugins/Google` (Sheets + OAuth) |

## Auth (OAuth)

1. Google Cloud **OAuth Desktop** client JSON → e.g. next to the imported Weapons Pull sample: `Assets/Samples/Aerisyn Data Config Sheet/0.4.0/Weapons Pull/oauth-client-secrets.json` (gitignored by filename).
2. Share the spreadsheet with your Google email as Viewer.
3. Select PullConfig → **Aerisyn → Data Config Sheet → Sign In With Google**.

## Quick start

1. Subclass `ConfigTypeAsset` for each table (root `items` list + nested serializable types).
2. Optional: `[ColumnAlias("Friendly Name")]` on fields; `[SheetTab("Tab Title")]` on the type when the Google tab differs from the type name.
3. Create **Pull Config**; set spreadsheet id, one shared output folder, and the explicit Config Type list (Odin dropdown of concrete `ConfigTypeAsset` subclasses).
4. Select a Config Type asset (or its script) → **Copy Header Row** to paste headers into Google (guidance covers `!!!` note columns).
5. Select PullConfig → **Sign In With Google**, then **Pull From Google**.
6. Read the **Pull report**: success lists created/updated assets; failures include **A1** sheet coordinates. Failed Pulls write nothing.
7. Or import sample **Weapons Pull** (Package Manager → Samples) and follow its README.
8. Inject path (no live Google): call `DataConfigPullRunner.PullFromGrids` with in-memory `SheetGrid`s.

## Template acceptance checklist

After installing the package and importing **Weapons Pull**, you should be able to:

- [ ] Complete one Pull using only the documented Google → Vertical Nest → Baked Asset path
- [ ] Round-trip nested weapons → upgrades → bonus stats into a Baked Asset
- [ ] Re-Pull and keep the same asset GUID / references
- [ ] Fix a bad cell using the Pull report’s A1 coordinate
- [ ] Add a second table with only: new Config Type + Pull Config list entry + matching tab (+ headers)

## Layout

| Folder | Purpose |
|---|---|
| `Runtime/` | `PullConfig`, `ConfigTypeAsset`, `[ColumnAlias]` / `[SheetTab]` |
| `Runtime/Parsing/` | `SheetGrid`, `VerticalNestParser`, `PullReport`, Header Emitter, tab resolver |
| `Editor/` | OAuth, Sheets fetch, Pull menus/runner, `BakedAssetWriter` |
| `Editor/Plugins/Google/` | Google.Apis* for Sheets/Drive |
| `Samples~/WeaponsPull/` | Nested Config Type template + documented Pull Config happy path |
| `Tests~/VerticalNest.Tests/` | Pure fixture tests for the parse + report seams (`dotnet test`) |
| `CONTEXT.md` | Domain glossary |

## Design

Active ADR: [`0010`](../../docs/adr/0010-dataconfigsheet-pull-scriptableobject.md). OAuth: [`0007`](../../docs/adr/0007-dataconfigsheet-oauth-primary.md). Historical (superseded) Luban spike: [`0009`](../../docs/adr/0009-dataconfigsheet-luban-google-csv.md).
