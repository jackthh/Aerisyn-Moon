# 04: Ship 0.5.0 docs + sample pattern

**What to build:** Consumers can adopt Local Only + After Pull from package docs at version 0.5.0: CHANGELOG/README describe the attribute, warn-and-ignore, Header Emitter omit note, and `OnAfterPull`; a sample or doc example shows the turnSpeed-style derivative pattern. Domain language stays aligned with CONTEXT and ADR 0013.

**Blocked by:** 01 (Local Only parse contract), 02 (Header Emitter omits Local Only), 03 (After Pull hook).

**Status:** ready-for-human

- [x] Package version is 0.5.0
- [x] CHANGELOG documents Local Only, warnings, Header Emitter omit guidance, and After Pull for 0.5.0
- [x] README teaches how to mark Local Only fields and override `OnAfterPull`
- [x] Sample or documented example shows sheet-owned input + Local Only derivatives filled in After Pull
- [x] Wording matches Data Config CONTEXT (Local Only vs Ignore Marker; clean Pull; After Pull)

## Answer

Shipped on `cursor/ship-0.5.0-docs-sample-dad2`:

- `package.json` + `DataConfigSheetPackage.Version` → `0.5.0`
- CHANGELOG `[0.5.0]` covers Local Only, warn-and-ignore, Header Emitter omit guidance, After Pull / throw / clean Pull, Ignore Marker distinction
- Package README: Local Only vs Ignore Marker table, `OnAfterPull` teaching, turnSpeed example, ADR 0013 link, acceptance checklist item
- Sample `MovementConfig` in Weapons Pull (+ DevHost `0.5.0` sample folder): sheet-owned `turnSpeed`, Local Only accel/decel, `OnAfterPull` refill
- Root README pin for dataconfigsheet → 0.5.0 (Quests/Tutorial stay 0.4.0)
- `VerticalNest.Tests` green (39)

## Comments

- Claimed for implement. Docs/sample ship; no new test seam (behavior locked by tickets 01–03).
