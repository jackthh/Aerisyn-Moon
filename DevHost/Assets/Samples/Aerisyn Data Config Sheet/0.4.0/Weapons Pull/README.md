# Weapons Pull sample

Copy-paste template for **Pull → ScriptableObject**: one nested Config Type (`WeaponsConfig`),
one Pull Config, and a Google tab shaped for Vertical Nest.

## Import

1. Package Manager → **Aerisyn Data Config Sheet** → Samples → **Weapons Pull** → Import.
2. Ensure **Odin Inspector** is in the project (required by `ConfigTypeAsset`).

## Sheet layout (paste into Google)

Tab title: **`WeaponsConfig`** (matches the type name).

| (preamble) | | | | |
|---|---|---|---|---|
| Weapons demo | | designer notes welcome | | |
| | | `!!!` | | |
| `id` | `Weapon Name` | `designer_note` | `upgrade_level` | `bonus_stats` |
| sword | Iron Sword | do not pull | 1 | 10 |
| | | still notes | | 15 |
| | | | 2 | 20 |
| axe | Battle Axe | ignored | 1 | 5 |

Notes:

- Rows above the header are **Preamble** (ignored).
- `!!!` above a column marks **Ignore Marker** notes (skipped by Pull).
- `Weapon Name` is a **Column Alias** for field `name`.
- Blank parent cells = **Vertical Nest** (upgrades under the same weapon; bonus stats under the same upgrade).

## Pull Config (create in your project)

No sample `.asset` is shipped (keep credentials and spreadsheet ids out of the package).

1. **Create → Aerisyn → Data Config Sheet → Pull Config**.
2. Spreadsheet id from the Google URL (`…/d/{id}/…`).
3. Auth = **OAuth User**; client secrets JSON path, e.g. `Assets/Samples/Aerisyn Data Config Sheet/0.4.0/Weapons Pull/oauth-client-secrets.json` (gitignored by filename).
4. Output Folder: e.g. `Assets/Samples/Aerisyn Data Config Sheet/0.4.0/Weapons Pull/Baked` (one shared folder next to the sample).
5. Config Types list: add **`WeaponsConfig`** from the dropdown (explicit list; no auto-scan).
6. Select `WeaponsConfig` script → **Aerisyn → Data Config Sheet → Copy Header Row** → paste into the sheet.
7. Select PullConfig → **Sign In With Google**, then **Pull From Google**.

On success the Pull report lists created/updated Baked Assets. On failure it shows **A1** cell
coordinates (e.g. `B5`) so you can fix the sheet; nothing is written until every type parses.

## Acceptance check (spec template bar)

Use this sample + the package README to verify:

- [ ] One documented Pull path (Google → Vertical Nest → Baked Asset)
- [ ] Nested weapons → upgrades → bonus stats round-trips into `WeaponsConfig.asset`
- [ ] Re-Pull overwrites the same asset path (stable GUID / references)
- [ ] Failed Pull leaves a clear report with sheet coordinates; no partial write
- [ ] Adding a second table = new Config Type + Pull Config list entry + matching tab

## Inject path (no live Google)

Build a `SheetGrid` matching the layout above and call
`DataConfigPullRunner.PullFromGrids(pullConfig, gridsByType)`. Fixture coverage lives in
`Tests~/VerticalNest.Tests` (weapons nest + aliases).
