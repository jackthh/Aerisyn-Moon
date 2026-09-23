# Basic Bake sample

Minimal BakingSheet schema + one editable baked ScriptableObject.

## Google Sheet shape

Tab name **`Items`**:

| Id | Name | Price |
|---|---|---|
| POTION_001 | Health Potion | 30 |
| POTION_002 | Mana Potion | 50 |

## Wire-up

1. Create **Demo Sheet Container Factory**.
2. Create **Demo Baked Data** (single editable SO).
3. Create **Bake Config**; assign factory + baked data; set spreadsheet id; Sign In; Bake.

Output is one `DemoBakedData` asset with an editable `Items` list (not BakingSheet row sub-assets).
