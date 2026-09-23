# Aerisyn Data Config Sheet (`com.aerisyn.dataconfigsheet`)

Editor bake tooling on top of [BakingSheet](https://github.com/cathei/BakingSheet): **Google Sheet → (optional CSV cache) → ScriptableObjects**.

**Unity:** 2022.3+ · **Requires:** [BakingSheet](https://github.com/cathei/BakingSheet) `com.cathei.bakingsheet` **v4.1.3** + [Odin Inspector](https://odininspector.com/) (Sirenix) · **Version:** `0.2.0`

## Product policy

| Rule | Meaning |
|---|---|
| Google Sheet is source of truth | Cloud spreadsheet is the official authoring surface |
| One-way bake only | Never push ScriptableObject edits back to Google Sheets |
| Local SOs are runtime final | After bake, gameplay reads ScriptableObjects; local tweaks are for temp tests only |
| Schema lives in the game | `Sheet` / `SheetContainer` classes stay in the consuming project; this package owns bake config, menus, and runner |

## Requirements

| Dependency | Why |
|---|---|
| **Unity 2022.3+** | Minimum editor target |
| **[BakingSheet](https://github.com/cathei/BakingSheet) `com.cathei.bakingsheet` v4.1.3** | Google converter + `ScriptableObjectSheetExporter`. **Not vendored** in this repo. |
| **[Odin Inspector](https://odininspector.com/) (Sirenix)** | `BakeConfig` Inspector uses Odin attributes. Aerisyn-Moon packages assume Odin is already installed in the consuming project (same as Quests). |

Odin and BakingSheet are **not** shipped in this repository. Install Odin per machine / per project; pin BakingSheet via UPM git URL.

### BakingSheet install (consumer projects)

This package lists BakingSheet in `package.json` `dependencies`. Unity **git → git nested dependencies are flaky**. If Package Manager does not resolve BakingSheet automatically, add the official URL to the **consuming project** `Packages/manifest.json`:

```json
"com.cathei.bakingsheet": "https://github.com/cathei/BakingSheet.git?path=UnityProject/Packages/com.cathei.bakingsheet#v4.1.3"
```

DevHost already pins BakingSheet explicitly for the same reason.

## Install this package

1. Install **Odin Inspector** and ensure BakingSheet is resolvable (see above).
2. Package Manager → **+ → Add package from git URL…**

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.dataconfigsheet
```

Pin a release tag when you cut one:

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.dataconfigsheet#com.aerisyn.dataconfigsheet@0.2.0
```

## Auth (service account Viewer)

1. Google Cloud Console → create a **service account** → download JSON key.
2. Share the Google Spreadsheet with the service account email as **Viewer** (read-only is enough).
3. Place the JSON somewhere under the project (recommended: `Assets/AerisynDataConfig/Credentials/service-account.json`).
4. **Never commit credentials.** Keep the Credentials folder gitignored.

Official BakingSheet guide: [google-sheet-import.md](https://github.com/cathei/BakingSheet/blob/v4.1.3/docs/google-sheet-import.md).

## Quick start

1. In the game, define `Sheet` / `SheetContainer` classes (BakingSheet schema).
2. Subclass `SheetContainerFactory` and implement `Create(ILogger)`.
3. Create **Bake Config**: Assets → Create → Aerisyn → Data Config Sheet → Bake Config.
4. Fill spreadsheet id(s), credential path, SO output path, optional CSV cache path, and assign the factory.
5. Select the BakeConfig → **Aerisyn → Data Config Sheet → Bake From Google (Selected Config)**.

Bake flow (package-owned):

1. `GoogleSheetConverter` for each spreadsheet id  
2. Optional `CsvSheetConverter` store when CSV cache path is set  
3. `ScriptableObjectSheetExporter` to the SO output folder  

## Layout

| Folder | Purpose |
|---|---|
| `Runtime/` | `BakeConfig`, `SheetContainerFactory` (game schema stays out) |
| `Editor/` | Bake runner + menus |
| `Samples~/BasicBake` | Demo `Items` sheet + container + factory |

## Out of scope (0.2.0)

Luban, two-way sync, runtime Google download, Addressables polish, IdleLotl CSV ports, forking/vendoring BakingSheet.

## Design

See [`docs/adr/0005-dataconfigsheet-one-way-google-bake.md`](../../docs/adr/0005-dataconfigsheet-one-way-google-bake.md).
