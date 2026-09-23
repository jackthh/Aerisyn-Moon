# DevHost: Data Config Sheet exercise

Local exercise for `com.aerisyn.dataconfigsheet` (Google → one editable ScriptableObject).

## Setup (OAuth default)

1. Place Desktop OAuth client JSON at `Credentials/oauth-client-secrets.json` (gitignored).
2. Share your Google Sheet with **your Google email** as **Viewer**.
3. Sheet tab **`Items`** with columns `Id`, `Name`, `Price`.
4. Create assets:
   - **DevHost Demo Factory** (Create → Aerisyn → Data Config Sheet → DevHost Demo Factory)
   - **DevHost Baked Data** (Create → Aerisyn → Data Config Sheet → DevHost Baked Data) — **one** editable SO
   - **Bake Config**
5. On Bake Config: set spreadsheet id, Auth Mode = OAuth User, assign Factory + **Baked Output** (the DevHost Baked Data asset).
6. Select BakeConfig → **Sign In With Google**, then **Bake From Google (Selected Config)**.

Open the **DevHost Baked Data** asset: you can edit `Items` rows in the Inspector for fast tests. Re-bake overwrites them. Do not push SO edits to Google.

You can delete the old BakingSheet `Baked/Items` + `POTION_*` sub-assets if they are leftover from the previous exporter.

## Optional: service account

Set Auth Mode to **Service Account**, put `Credentials/service-account.json`, share the sheet with the robot email.
