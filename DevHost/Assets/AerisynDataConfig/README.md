# DevHost: Data Config Sheet (Luban 0.3.0)

Local exercise for `com.aerisyn.dataconfigsheet`: **Google → CSV → Luban → `cfg.Tables`**.

## One-time setup

1. Install **.NET SDK 8+** (9 works; bake sets `DOTNET_ROLL_FORWARD=Major`).
2. Download [Luban](https://github.com/focus-creative-games/luban/releases) `Luban.7z` → extract to repo `Tools/Luban/` so `Tools/Luban/Luban/Luban.dll` exists (gitignored).
3. DevHost already references `com.code-philosophy.luban` (Unity runtime).
4. Place OAuth Desktop client JSON at `Credentials/oauth-client-secrets.json` (gitignored).

## Luban project (already scaffolded)

| Path | Role |
|---|---|
| `Luban/luban.conf` | Luban project entry |
| `Luban/Defines/tables.xml` | Table + nested bean registration |
| `Luban/Data/items.csv` | Flat Items seed (schema-from-file headers) |
| `Luban/Data/demo_nested.csv` | Nested DemoNested seed (compact `list#sep` cells) |
| `Gen/` | Generated C# (`cfg.*`) |
| `GeneratedData/` | Generated JSON |

Offline generation (no Google) was verified with the seed CSVs.

### Nested note

Sparse multi-row `*levels` + nested `*bonus_stats` in **CSV** did not parse cleanly in this spike. DevHost uses **compact sep** for levels (`list#sep=;` of `Level`, with `bonus_stats` as `5|10|15`). You can later move designers to Excel/xlsx multi-row layouts or tune CSV headers.

## Bake Config

1. **Create → Aerisyn → Data Config Sheet → Bake Config**.
2. Spreadsheet id; Auth = OAuth User.
3. Tab export examples:
   - Tab `Items` → `items.csv`
   - Tab `DemoNested` → `demo_nested.csv`
4. Luban Project Path: `Assets/AerisynDataConfig/Luban`
5. Luban Dll Path: `Tools/Luban/Luban/Luban.dll`
6. Output Code: `Assets/AerisynDataConfig/Gen` · Output Data: `Assets/AerisynDataConfig/GeneratedData`
7. Select BakeConfig → **Sign In With Google** → **Bake From Google (Selected Config)**.

Google tabs must match Luban-compatible headers (or you will overwrite seed CSVs with incompatible exports).

## Smoke test

Add `DataConfigLubanSmoke` to a scene object (e.g. `Demo/DataConfig.unity`). Play Mode logs `TbItem` / `TbDemoNested` counts.

## Optional: service account

Set Auth Mode to **Service Account**, put `Credentials/service-account.json`, share the sheet with the robot email.
