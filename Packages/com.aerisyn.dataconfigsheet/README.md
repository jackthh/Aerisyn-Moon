# Aerisyn Data Config Sheet (`com.aerisyn.dataconfigsheet`)

Editor **Pull**: **Google Sheet (OAuth) → in-memory cell grid → type-driven Vertical Nest → ScriptableObject Baked Assets**.

That is the only supported product path for this package.

**Unity:** 2022.3+ · **Version:** `0.5.0` · **Requires:** [Odin Inspector](https://odininspector.com/), Google API Editor plugins (bundled)

See [`CONTEXT.md`](./CONTEXT.md), [`docs/adr/0010-…`](../../docs/adr/0010-dataconfigsheet-pull-scriptableobject.md), and [`docs/adr/0013-…`](../../docs/adr/0013-dataconfigsheet-local-only-after-pull.md).

## Product policy

| Rule | Meaning |
|---|---|
| Google Sheet is source of truth | Cloud spreadsheet is the official authoring surface for sheet-owned values |
| One-way Pull only | Never push Baked Assets back to Google |
| Runtime final | ScriptableObject Baked Assets (one asset per matching tab) |
| Schema in C# Config Types | Hand-written Odin `ConfigTypeAsset` subclasses; nest shape is type-driven |
| No CSV required | Sheets API cells stay in memory; commas inside cells do not break Pull |
| Clean re-Pull | Existing Baked Assets are overwritten in place; Local Only fields are not merged from the prior asset |

## Requirements

| Dependency | Why |
|---|---|
| **Odin Inspector** | `PullConfig` / `ConfigTypeAsset` drawers and serialization |
| **Google API Editor plugins** | Bundled under `Editor/Plugins/Google` (Sheets + OAuth) |

## Auth (OAuth)

1. Google Cloud **OAuth Desktop** client JSON → e.g. next to the imported Weapons Pull sample: `Assets/Samples/Aerisyn Data Config Sheet/0.5.0/Weapons Pull/oauth-client-secrets.json` (gitignored by filename).
2. Share the spreadsheet with your Google email as Viewer.
3. Select PullConfig → **Aerisyn → Data Config Sheet → Sign In With Google**.

## Quick start

1. Subclass `ConfigTypeAsset` for each table (root `items` list + nested serializable types).
2. Optional: `[ColumnAlias("Friendly Name")]` on fields; `[SheetTab("Tab Title")]` on the type when the Google tab differs from the type name; `[LocalOnly]` on fields the sheet must not own (see below).
3. Create **Pull Config**; set spreadsheet id, one shared output folder, and the explicit Config Type list (Odin dropdown of concrete `ConfigTypeAsset` subclasses).
4. Select a Config Type asset (or its script) → **Copy Header Row** to paste headers into Google (guidance covers `!!!` note columns and omitted Local Only fields).
5. Select PullConfig → **Sign In With Google**, then **Pull From Google**.
6. Read the **Pull report**: success lists created/updated assets; failures include **A1** sheet coordinates. Failed Pulls write nothing.
7. Or import sample **Weapons Pull** (Package Manager → Samples) and follow its README.
8. Inject path (no live Google): call `DataConfigPullRunner.PullFromGrids` with in-memory `SheetGrid`s.

## Local Only + After Pull

Use **Local Only** when the Baked Asset holds fields the Source Sheet should not require (derivatives, game-owned values). That is the opposite of **Ignore Marker** (`!!!`), which skips *extra sheet columns* designers keep for notes.

| Concept | Where the extras live | What Pull does |
|---|---|---|
| **Ignore Marker** (`!!!`) | Extra columns on the Source Sheet | Skips those columns |
| **Local Only** (`[LocalOnly]`) | Extra fields on the Config Type / Baked Asset | Not required on Header Row; never bound from cells |

Mark fields with `[LocalOnly]`. Header Row match ignores them. If someone pastes a matching Field Header or Column Alias anyway, Pull **warns** (alias text included when set) and **ignores** the column. Re-Pull is clean: Local Only values start at type defaults after parse; they are not merged from the previous Inspector state.

Override `OnAfterPull` on the Config Type to refill Local Only derivatives after every included tab parses and **before** any Baked Asset create/save. Default is a no-op. A throw fails the whole Pull with no writes. Do not rely on `OnValidate` / Odin `OnValueChanged` for this refill; those callbacks may not run after overwrite.

### Example: sheet-owned `turnSpeed`, Local Only derivatives

```csharp
using System;
using System.Collections.Generic;
using Aerisyn.DataConfigSheet;

public sealed class MovementConfig : ConfigTypeAsset
{
    public List<MovementRow> items = new List<MovementRow>();

    public override void OnAfterPull()
    {
        for (int i = 0; i < items.Count; i++)
        {
            MovementRow row = items[i];
            // Sheet owns turnSpeed; Local Only accel/decel are game math
            row.accelerateTurnSpeed = row.turnSpeed * 2;
            row.decelerateTurnSpeed = row.turnSpeed;
        }
    }
}

[Serializable]
public sealed class MovementRow
{
    public string id = "";
    public int turnSpeed;

    [LocalOnly]
    public int accelerateTurnSpeed;

    [LocalOnly]
    public int decelerateTurnSpeed;
}
```

Sheet Header Row needs only `id` and `turnSpeed` (plus any Ignore Marker note columns). Copy Header Row omits the Local Only fields and notes how many were left out. After a successful Pull, `accelerateTurnSpeed` / `decelerateTurnSpeed` are written into the Baked Asset via After Pull.

## Template acceptance checklist

After installing the package and importing **Weapons Pull**, you should be able to:

- [ ] Complete one Pull using only the documented Google → Vertical Nest → Baked Asset path
- [ ] Round-trip nested weapons → upgrades → bonus stats into a Baked Asset
- [ ] Re-Pull and keep the same asset GUID / references
- [ ] Fix a bad cell using the Pull report’s A1 coordinate
- [ ] Add a second table with only: new Config Type + Pull Config list entry + matching tab (+ headers)
- [ ] Mark a Local Only field, omit it from the sheet, override `OnAfterPull`, and see derivatives on the Baked Asset

## Layout

| Folder | Purpose |
|---|---|
| `Runtime/` | `PullConfig`, `ConfigTypeAsset`, `[ColumnAlias]` / `[SheetTab]` / `[LocalOnly]` |
| `Runtime/Parsing/` | `SheetGrid`, `VerticalNestParser`, `PullReport`, Header Emitter, After Pull helper, tab resolver |
| `Editor/` | OAuth, Sheets fetch, Pull menus/runner, `BakedAssetWriter` |
| `Editor/Plugins/Google/` | Google.Apis* for Sheets/Drive |
| `Samples~/WeaponsPull/` | Nested Config Type template + Pull Config happy path; `MovementConfig` Local Only + After Pull |
| `Tests~/VerticalNest.Tests/` | Pure fixture tests for the parse + report seams (`dotnet test`) |
| `CONTEXT.md` | Domain glossary |

## Design

Active ADRs: [`0010`](../../docs/adr/0010-dataconfigsheet-pull-scriptableobject.md) (Pull → SO), [`0013`](../../docs/adr/0013-dataconfigsheet-local-only-after-pull.md) (Local Only + After Pull). OAuth: [`0007`](../../docs/adr/0007-dataconfigsheet-oauth-primary.md). Historical (superseded) Luban spike: [`0009`](../../docs/adr/0009-dataconfigsheet-luban-google-csv.md).
