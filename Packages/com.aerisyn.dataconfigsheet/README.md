# Aerisyn Data Config Sheet (`com.aerisyn.dataconfigsheet`)

Editor **Pull**: **Google Sheet (OAuth) → in-memory cell grid → type-driven Vertical Nest → ScriptableObject Baked Assets**.

That is the only supported product path for this package.

**Unity:** 2022.3+ · **Version:** `0.7.0` · **Requires:** [Odin Inspector](https://odininspector.com/), Google API Editor plugins (bundled)

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
| **Odin Inspector** | `DataConfigSettings` / `PullConfig` / `ConfigTypeAsset` drawers and serialization |
| **Google API Editor plugins** | Bundled under `Editor/Plugins/Google` (Sheets + OAuth) |

## Data Config Settings window

Open **Aerisyn → Data Config Sheet → Open Window**. The first open creates the one **Data Config Settings** asset at the fixed path `Assets/AerisynDataConfig/DataConfigSettings.asset` (Environment Keys seeded Dev, Test, Production; Feature Keys empty). Commit it. The window edits it and hosts **Sign In**, **Sign Out** and **Pull All**. The top of the window shows the active environment, a setup checklist with one-click fixes (Choose… folder or credentials file, Sign In) and **Pull All**, which stays disabled until nothing blocks it; the settings are split into **Keys**, **Bindings**, **Manifest**, **Auth** and **Pull All** tabs.

Data Config Settings holds everything project-wide:

| Group | Contents |
|---|---|
| Keys | Environment Keys, Feature Keys, the active Environment Key |
| Bindings | Environment Key x Feature Key -> spreadsheet id; Title Check prefix overrides |
| Config Manifest | Manifest Asset Path (`.asset` file picker), Required Config Types |
| Google auth | Auth mode, OAuth client secrets path, OAuth user token path (default under `UserSettings/`, ignored by version control), service-account JSON path |
| Pull All | Folder path (folder picker) |

Upgrading from 0.6.0: there is no migrator. Recreate keys, bindings and auth in the window, then re-pick the Feature Key on each Pull Config.

## Auth (OAuth)

1. Google Cloud **OAuth Desktop** client JSON → keep it out of version control and set **OAuth Client Secrets Path** in the Data Config Settings window.
2. Share the spreadsheet with your Google email as Viewer.
3. Click **Sign In** in the window (status shows Signed in / Not signed in). One sign-in serves every Pull Config. Menu alias: **Aerisyn → Data Config Sheet → Sign In With Google**.

## Quick start

1. Subclass `ConfigTypeAsset` for each table (root `items` list + nested serializable types).
2. Optional: `[ColumnAlias("Friendly Name")]` on fields; `[SheetTab("Tab Title")]` on the type when the Google tab differs from the type name; `[LocalOnly]` on fields the sheet must not own (see below).
3. Open the Data Config Sheet window; add your **Feature Keys**, bind each to a spreadsheet id per Environment Key, and pick the active Environment Key. Create **Pull Config**; pick a **Feature Key**, set one shared output folder, and the explicit Config Type list (Odin dropdown of concrete `ConfigTypeAsset` subclasses). Pull checks the Source Sheet title starts with the active environment's prefix (`[Dev]`, `[Tester]`, `[Production]` by default; override per key in Data Config Settings) and aborts before any parse or write when it does not. A missing Data Config Settings asset fails Pull with a clear message.
4. Select a Config Type asset (or its script) → **Copy Header Row** to paste headers into Google (guidance covers `!!!` note columns and omitted Local Only fields).
5. **Sign In** in the window, then **Pull** on the Pull Config Inspector, or **Pull All** in the window (menus under **Aerisyn → Data Config Sheet** remain aliases).
6. Read the **Pull report**: success lists created/updated assets; failures include **A1** sheet coordinates. Failed Pulls write nothing.
7. Or import sample **Weapons Pull** (Package Manager → Samples) and follow its README.
8. Inject path (no live Google): call `DataConfigPullRunner.PullFromGrids` with in-memory `SheetGrid`s.

## Pull All + Config Manifest

**Pull All** (window button, or **Aerisyn → Data Config Sheet → Pull From Google (All Configs)**) Pulls every Pull Config under the settings **Pull All Folder Path**, recursively, in asset-path order. A blank folder path fails, and so does a folder with no Pull Configs; there is no whole-project fallback. It never stops at the first failure: one summary lists each passed and failed config, and the overall result fails if any config failed. Every config shares the one active Environment Key and the one manifest.

When every config passes, Pull All writes one **Config Manifest** (`Aerisyn/Data Config Sheet/Config Manifest`) at the settings **Manifest Asset Path**. It records the active Environment Key, the UTC time, per Feature Key the Source Sheet id, title and Drive `modifiedTime` / `version` (best effort; empty if Drive is unavailable), and a SHA-256 per Baked Asset. Drive metadata uses the existing `drive.readonly` scope with `supportsAllDrives=true`; no new scope.

List the Config Types that must exist in **Required Config Types** in Data Config Settings. After the manifest write, Pull All checks it against that list and the active environment; a missing type or wrong environment fails the run. A failed Pull All leaves the previous manifest untouched, but the required list is still checked against what the passed configs produced, so the summary lists failed configs and missing Config Types together.

**Skip Title Check** (Pull Config → Google source) is off by default. Turn it on only for a documented exception sheet whose title does not carry the environment prefix (for example a shared `CONVENTIONS` sheet). That Pull skips the title read, so its manifest title is empty.

Opt-in build gates: the package registers nothing. Call them from your own preprocessor. `Require` is the Production gate (fails when the active environment is not Production); `RequireCompleteManifest` is a separate check that fails when the manifest is missing, for another environment, or lacks a required Config Type:

```csharp
class DataConfigBuildCheck : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        DataConfigSettings settings = Aerisyn.DataConfigSheet.Editor.DataConfigSettingsLoader.Load();
        Aerisyn.DataConfigSheet.Editor.DataConfigBuildGate.Require(settings);
        Aerisyn.DataConfigSheet.Editor.DataConfigBuildGate.RequireCompleteManifest(settings); // optional
    }
}
```

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
| `Runtime/` | `DataConfigSettings`, `PullConfig`, `ConfigTypeAsset`, `[ColumnAlias]` / `[SheetTab]` / `[LocalOnly]` |
| `Runtime/Parsing/` | `SheetGrid`, `VerticalNestParser`, `PullReport`, Header Emitter, After Pull helper, tab resolver |
| `Editor/` | Data Config Sheet window, OAuth, Sheets fetch, Pull menus/runner, `BakedAssetWriter` |
| `Editor/Plugins/Google/` | Google.Apis* for Sheets/Drive |
| `Samples~/WeaponsPull/` | Nested Config Type template + Pull Config happy path; `MovementConfig` Local Only + After Pull |
| `Tests~/VerticalNest.Tests/` | Pure fixture tests for the parse + report seams (`dotnet test`) |
| `CONTEXT.md` | Domain glossary |

## Design

Active ADRs: [`0010`](../../docs/adr/0010-dataconfigsheet-pull-scriptableobject.md) (Pull → SO), [`0013`](../../docs/adr/0013-dataconfigsheet-local-only-after-pull.md) (Local Only + After Pull). OAuth: [`0007`](../../docs/adr/0007-dataconfigsheet-oauth-primary.md). Historical (superseded) Luban spike: [`0009`](../../docs/adr/0009-dataconfigsheet-luban-google-csv.md).
