# DevHost: Data Config Sheet (Pull → ScriptableObject)

Local exercise for `com.aerisyn.dataconfigsheet`: **Google Sheets → Pull → Baked Assets**.

Product direction is ADR 0010. Luban / CSV bake is no longer part of this package.

## One-time setup

1. Install **[Odin Inspector](https://odininspector.com/)** in DevHost (not committed).
2. Place OAuth Desktop client JSON at `Credentials/oauth-client-secrets.json` (gitignored).

## Pull Config (shell)

1. **Create → Aerisyn → Data Config Sheet → Pull Config**.
2. Spreadsheet id; Auth = OAuth User.
3. Output Folder: `Assets/AerisynDataConfig/Baked` (shared for all types on this config).
4. Add explicit Config Types (subclasses of `ConfigTypeAsset`). No assembly auto-scan.
5. Select PullConfig → **Aerisyn → Data Config Sheet → Sign In With Google**.

Fixture/inject Pull (no live spreadsheet): build `SheetGrid`s and call `DataConfigPullRunner.PullFromGrids` to create or overwrite Baked Assets under the shared output folder (GUID stable on re-Pull). Menu **Pull From Google** still waits on the live Sheets fetch ticket.

## Layout

| Path | Role |
|---|---|
| `Credentials/` | OAuth / service-account JSON (gitignored keys) |
| `Baked/` | Shared output folder for Baked Assets |
| `Demo/DataConfig.unity` | Scene placeholder for upcoming Pull sample |

## Optional: service account

Set Auth Mode to **Service Account**, put `Credentials/service-account.json`, share the sheet with the robot email.
