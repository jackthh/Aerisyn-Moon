# Basic Bake sample

Minimal BakingSheet schema for exercising `com.aerisyn.dataconfigsheet`.

## Google Sheet shape

Create a spreadsheet with a tab named **`Items`** and columns:

| Id | Name | Price |
|---|---|---|
| POTION_001 | Health Potion | 30 |
| POTION_002 | Mana Potion | 50 |

Share the sheet with your service account as **Viewer**.

## Wire-up

1. Create **Demo Sheet Container Factory** (Assets → Create → Aerisyn → Data Config Sheet → Demo Sheet Container Factory).
2. Create **Bake Config**; set spreadsheet id, credential path, SO output path; assign the factory.
3. **Aerisyn → Data Config Sheet → Bake From Google (Selected Config)**.

Property name `Items` on `DemoSheetContainer` must match the Google Sheet tab name.
