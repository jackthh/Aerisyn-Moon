# Aerisyn Data Config Sheet (`com.aerisyn.dataconfigsheet`)

> **Redesign in progress (ADR 0010):** product direction is **Pull → ScriptableObject**. See [agent spec](../../.scratch/dataconfig-pull-so/spec.md), [`CONTEXT.md`](./CONTEXT.md), and [`docs/adr/0010-…`](../../docs/adr/0010-dataconfigsheet-pull-scriptableobject.md). The Luban bake flow below is **obsolete**.

Editor bake tooling: **Google Sheet → CSV → [Luban](https://github.com/focus-creative-games/luban)** (`cs-simple-json` + JSON).

**Unity:** 2022.3+ · **Version:** `0.3.0` · **Requires:** .NET SDK 8+ (or 9 with roll-forward), [Odin Inspector](https://odininspector.com/), [Luban Unity runtime](https://github.com/focus-creative-games/luban_unity) in the game, Luban CLI under `Tools/Luban`

## Product policy

| Rule | Meaning |
|---|---|
| Google Sheet is source of truth | Cloud spreadsheet is the official authoring surface |
| One-way bake only | Never push generated JSON/code back to Google |
| Runtime final (0.3.0) | Luban `Tables` + JSON (editable SO paused; see ADR 0008/0009) |
| Schema in sheet headers / Luban XML | `##var` / `##type` and/or `Defines/*.xml`; not BakingSheet `Sheet` types |

## Requirements

| Dependency | Why |
|---|---|
| **.NET SDK 8+** | Runs `Luban.dll` (`DOTNET_ROLL_FORWARD=Major` allows net9 hosts) |
| **Luban CLI v5.x** | Download release zip into repo `Tools/Luban/` (gitignored) |
| **`com.code-philosophy.luban`** | Runtime `Luban` / `Luban.SimpleJSON` for generated code |
| **Odin Inspector** | `BakeConfig` drawers |
| **Google API Editor plugins** | Bundled under `Editor/Plugins/Google` (Sheets export) |

## Install Luban CLI

1. Install [.NET SDK](https://dotnet.microsoft.com/download) 8+.
2. Download [Luban release](https://github.com/focus-creative-games/luban/releases) `Luban.7z`.
3. Extract so `Tools/Luban/Luban/Luban.dll` exists (path configurable on BakeConfig).

## Auth (OAuth)

1. Google Cloud **OAuth Desktop** client JSON → e.g. `Assets/AerisynDataConfig/Credentials/oauth-client-secrets.json` (gitignored).
2. Share the spreadsheet with your Google email as Viewer.
3. Select BakeConfig → **Aerisyn → Data Config Sheet → Sign In With Google**.

## Quick start

1. Create a Luban project (`luban.conf`, `Defines/`, `Data/`) — see DevHost `Assets/AerisynDataConfig/Luban`.
2. Create **Bake Config**; set spreadsheet id, tab→CSV exports, Luban paths, output dirs.
3. Put Luban headers in Google tabs (`##var` / `##type`), or keep seed CSVs until tabs match.
4. Sign In → **Bake From Google (Selected Config)**.
5. Load with `new cfg.Tables(file => JSON.Parse(File.ReadAllText(...)))`.

## Layout

| Folder | Purpose |
|---|---|
| `Runtime/` | `BakeConfig`, `GoogleAuthMode` |
| `Editor/` | OAuth, CSV export, Luban runner, menus |
| `Editor/Plugins/Google/` | Google.Apis* for Sheets/Drive |
| `CONTEXT.md` | Domain glossary |

## Design

See [`docs/adr/0009`](../../docs/adr/0009-dataconfigsheet-luban-google-csv.md) (active), [`0007`](../../docs/adr/0007-dataconfigsheet-oauth-primary.md) (OAuth), [`0005`](../../docs/adr/0005-dataconfigsheet-one-way-google-bake.md) / [`0008`](../../docs/adr/0008-dataconfigsheet-editable-single-so.md) (superseded / paused).
