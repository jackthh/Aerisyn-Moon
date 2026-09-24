# 04: Pull report + template sample and docs

**What to build:** Failed Pulls report enough detail to fix sheets (including tab and cell coordinates when possible). DevHost or Samples plus the package README demonstrate one Pull → SO workflow only, so a newcomer can copy the template and check off the spec acceptance bar.

**Blocked by:** 03 — Live Google Pull + aliases, tab override, Header Emitter

**Status:** resolved

- [x] Pull report surfaces success summary and failures with sheet coordinates when available
- [x] Sample shows nested (weapons-style) Config Type + Pull Config + documented happy path
- [x] README describes Pull → SO as the product path (not Luban)
- [x] Spec acceptance checklist items for the template are achievable with the sample + docs

## Answer

Shipped on `feature/v0.3.0` (ticket 04):

- `PullReport` (+ `PullWriteAction`): success lists created/updated Baked Assets; failures use A1 (or row/column fallback) plus Google **tab title** prefix; menus log and show the report; failed Pull still writes nothing.
- Sample `Samples~/WeaponsPull`: nested `WeaponsConfig` + Pull Config happy-path README + acceptance checklist; registered in `package.json`.
- Package + DevHost READMEs teach Pull → SO only (Luban demoted to historical ADR 0009).

## Comments

- Claimed for implement. Seams: pure `PullReport` format (success summary + A1 sheet coordinates from 0-based parse errors); Vertical Nest parse seam unchanged. Sample + README are docs (no automated seam). No PullConfig `.asset` committed (create via menu; sample documents the fields).
