# Aerisyn Data Config Sheet (`com.aerisyn.dataconfigsheet`)

Editor bake tooling on top of [BakingSheet](https://github.com/cathei/BakingSheet): **Google Sheet → (optional CSV cache) → one editable ScriptableObject**.

**Unity:** 2022.3+ · **Requires:** [BakingSheet](https://github.com/cathei/BakingSheet) `com.cathei.bakingsheet` **v4.1.3** + [Odin Inspector](https://odininspector.com/) (Sirenix) · **Version:** `0.2.0`

## Product policy

| Rule | Meaning |
|---|---|
| Google Sheet is source of truth | Cloud spreadsheet is the official authoring surface |
| One-way bake only | Never push ScriptableObject edits back to Google Sheets |
| Local SOs are runtime final | After bake, gameplay reads the baked SO; **Inspector edits are allowed for fast tests**; the next bake overwrites them |
| One parent SO file | Bake writes into a game-owned `BakedSheetContainerAsset` (serializable lists). Not BakingSheet’s multi sub-asset SO layout |
| Schema lives in the game | `Sheet` / `SheetContainer` + baked SO type stay in the consuming project; this package owns bake config, menus, and runner |

## Requirements

| Dependency | Why |
|---|---|
| **Unity 2022.3+** | Minimum editor target |
| **[BakingSheet](https://github.com/cathei/BakingSheet) `com.cathei.bakingsheet` v4.1.3** | Google (+ optional CSV) import. **Not vendored** in this repo. |
| **[Odin Inspector](https://odininspector.com/) (Sirenix)** | `BakeConfig` / baked SO Inspector UX. Assumed installed in the consuming project. |

### BakingSheet install (consumer projects)

If Package Manager does not resolve BakingSheet automatically, add to the consumer `Packages/manifest.json`:

```json
"com.cathei.bakingsheet": "https://github.com/cathei/BakingSheet.git?path=UnityProject/Packages/com.cathei.bakingsheet#v4.1.3"
```

## Install this package

1. Install **Odin Inspector** and ensure BakingSheet is resolvable.
2. Package Manager → **+ → Add package from git URL…**

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.dataconfigsheet
```

## Auth (default: OAuth browser sign-in)

1. One-time org: Google Cloud **OAuth Desktop** client JSON → e.g. `Assets/AerisynDataConfig/Credentials/oauth-client-secrets.json` (gitignored). Enable Sheets + Drive APIs; add team as consent test users.
2. Share the spreadsheet with **your Google email** as Viewer (not public).
3. Select BakeConfig → **Aerisyn → Data Config Sheet → Sign In With Google**.
4. Bake.

Optional **Service Account** Auth Mode for CI. Details in older BakingSheet docs.

## Quick start

1. Define `Sheet` / `SheetContainer` in the game.
2. Subclass `SheetContainerFactory` and `BakedSheetContainerAsset` (copy rows into `[Serializable]` lists in `ApplyFromContainer`).
3. Create Bake Config; assign factory + **Baked Output**; set spreadsheet id(s).
4. Sign In → Bake From Google.

Bake flow:

1. `GoogleSheetConverter` (OAuth or service-account JSON)  
2. Optional `CsvSheetConverter` when CSV cache path is set  
3. `BakedSheetContainerAsset.ApplyFromContainer` (Undo + SetDirty) into **one** editable SO  

## Layout

| Folder | Purpose |
|---|---|
| `Runtime/` | `BakeConfig`, `GoogleAuthMode`, `SheetContainerFactory`, `BakedSheetContainerAsset` |
| `Editor/` | OAuth session, bake runner, menus |
| `Samples~/BasicBake` | Demo sheet + factory + `DemoBakedDataAsset` |

## Out of scope (0.2.0)

Luban, two-way sync, runtime Google download, Addressables polish, IdleLotl CSV ports, forking BakingSheet, BakingSheet `ScriptableObjectSheetExporter` as the default output (read-only row sub-assets).

## Design

See [`docs/adr/0005`](../../docs/adr/0005-dataconfigsheet-one-way-google-bake.md), [`0007`](../../docs/adr/0007-dataconfigsheet-oauth-primary.md), [`0008`](../../docs/adr/0008-dataconfigsheet-editable-single-so.md).
