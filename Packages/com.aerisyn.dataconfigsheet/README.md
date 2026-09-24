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

## Requirements

| Dependency | Why |
|---|---|
| **Odin Inspector** | `PullConfig` / `ConfigTypeAsset` drawers and serialization |
| **Google API Editor plugins** | Bundled under `Editor/Plugins/Google` (Sheets + OAuth) |

## Auth (OAuth)

1. Google Cloud **OAuth Desktop** client JSON → e.g. `Assets/AerisynDataConfig/Credentials/oauth-client-secrets.json` (gitignored).
2. Share the spreadsheet with your Google email as Viewer.
3. Select PullConfig → **Aerisyn → Data Config Sheet → Sign In With Google**.

## Quick start (shell)

1. Subclass `ConfigTypeAsset` for each table (root `items` list + nested serializable types).
2. Create **Pull Config**; set spreadsheet id, one shared output folder, and the explicit Config Type list.
3. Select PullConfig → **Sign In With Google**.
4. Inject path (no live Google): call `DataConfigPullRunner.PullFromGrids` with in-memory `SheetGrid`s to create/overwrite `{TypeName}.asset` under the shared folder.
5. Live Google Sheets fetch lands in a follow-up ticket; menu **Pull From Google** still fails clearly until then.

## Layout

| Folder | Purpose |
|---|---|
| `Runtime/` | `PullConfig`, `ConfigTypeAsset`, Vertical Nest parse |
| `Runtime/Parsing/` | `SheetGrid`, `VerticalNestParser`, Baked Asset path/copy helpers |
| `Editor/` | OAuth, Pull menus/runner, `BakedAssetWriter` |
| `Editor/Plugins/Google/` | Google.Apis* for Sheets/Drive |
| `Tests~/VerticalNest.Tests/` | Pure fixture tests for the parse seam (`dotnet test`) |
| `CONTEXT.md` | Domain glossary |

## Design

See [`docs/adr/0010`](../../docs/adr/0010-dataconfigsheet-pull-scriptableobject.md) (active), [`0007`](../../docs/adr/0007-dataconfigsheet-oauth-primary.md) (OAuth), [`0009`](../../docs/adr/0009-dataconfigsheet-luban-google-csv.md) (superseded Luban path).
