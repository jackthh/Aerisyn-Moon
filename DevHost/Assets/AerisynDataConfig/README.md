# DevHost: Data Config Sheet exercise

Local exercise for `com.aerisyn.dataconfigsheet` (BakingSheet Google → ScriptableObject bake).

## Setup

1. Place service-account JSON at `Credentials/service-account.json` (gitignored; never commit).
2. Share your Google Sheet with that service account as **Viewer**.
3. Sheet tab **`Items`** with columns `Id`, `Name`, `Price`.
4. Create **DevHost Demo Factory** (Assets → Create → Aerisyn → Data Config Sheet → DevHost Demo Factory).
5. Create **Bake Config**; set spreadsheet id, credential path `Assets/AerisynDataConfig/Credentials/service-account.json`, SO output `Assets/AerisynDataConfig/Baked`, assign the factory.
6. Select BakeConfig → **Aerisyn → Data Config Sheet → Bake From Google (Selected Config)**.

BakingSheet is pinned in `DevHost/Packages/manifest.json` because Unity nested git dependencies are unreliable.
