# Aerisyn Data Config Sheet (`com.aerisyn.dataconfigsheet`)

Editor **Pull**: **Google Sheet (OAuth) → in-memory cell grid → type-driven Vertical Nest → ScriptableObject Baked Assets**.

**Unity:** 2022.3+ · **Version:** `0.3.0` · **Requires:** [Odin Inspector](https://odininspector.com/), Google API Editor plugins (bundled)

See [`CONTEXT.md`](./CONTEXT.md) and [`docs/adr/0010-…`](../../docs/adr/0010-dataconfigsheet-pull-scriptableobject.md). Agent spec: [`.scratch/dataconfig-pull-so/spec.md`](../../.scratch/dataconfig-pull-so/spec.md).

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

1. Google Cloud **OAuth Desktop** client JSON → e.g. `Assets/AerisynDataConfig/Credentials/oauth-client-secrets.json` (gitignored).
2. Share the spreadsheet with your Google email as Viewer.
3. Select PullConfig → **Aerisyn → Data Config Sheet → Sign In With Google**.

## Quick start

1. Subclass `ConfigTypeAsset` for each table (root `items` list + nested serializable types).
2. Optional: `[ColumnAlias("Friendly Name")]` on fields; `[SheetTab("Tab Title")]` on the type when the Google tab differs from the type name.
3. Create **Pull Config**; set spreadsheet id, one shared output folder, and the explicit Config Type list.
4. Select a Config Type asset (or its script) → **Copy Header Row** to paste headers into Google (guidance covers `!!!` note columns).
5. Select PullConfig → **Sign In With Google**, then **Pull From Google**.
6. Inject path (no live Google): call `DataConfigPullRunner.PullFromGrids` with in-memory `SheetGrid`s.

## Layout

| Folder | Purpose |
|---|---|
| `Runtime/` | `PullConfig`, `ConfigTypeAsset`, `[ColumnAlias]` / `[SheetTab]` |
| `Runtime/Parsing/` | `SheetGrid`, `VerticalNestParser`, Header Emitter, tab resolver, Baked Asset helpers |
| `Editor/` | OAuth, Sheets fetch, Pull menus/runner, `BakedAssetWriter` |
| `Editor/Plugins/Google/` | Google.Apis* for Sheets/Drive |
| `Tests~/VerticalNest.Tests/` | Pure fixture tests for the parse seam (`dotnet test`) |
| `CONTEXT.md` | Domain glossary |

## Design

See [`docs/adr/0010`](../../docs/adr/0010-dataconfigsheet-pull-scriptableobject.md) (active), [`0007`](../../docs/adr/0007-dataconfigsheet-oauth-primary.md) (OAuth), [`0009`](../../docs/adr/0009-dataconfigsheet-luban-google-csv.md) (superseded Luban path).
