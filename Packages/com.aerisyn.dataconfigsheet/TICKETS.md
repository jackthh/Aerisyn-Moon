# Data Config Sheet — Implementation tickets

Tracked in-repo because this environment’s GitHub token cannot create issues (`403 Resource not accessible by personal access token`). Copy these into GitHub issues when you have write access, or implement against this list.

Parent: ADR 0010 + [`SPEC.md`](./SPEC.md) + [`CONTEXT.md`](./CONTEXT.md). Design PR: https://github.com/jackthh/Aerisyn-Moon/pull/9

---

## T1 — Strip Luban / obsolete bake paths

**Why:** One workflow for newcomers (Q19).

**Done when:**
- [ ] Luban CLI runner, Luban fields on config assets, and Luban-first DevHost sample are removed or clearly not the path
- [ ] Leftover 0.2.0 / BakingSheet-era demo assets that imply the old stack are gone or replaced
- [ ] Package README no longer documents Luban as the product path
- [ ] `CONTEXT.md` / SPEC remain the source of truth for language

**Depends on:** none (can land first or with T2)

---

## T2 — Pull Config shape (Bake → Pull)

**Why:** Explicit type list, one output folder, name match (Q13 / Q13b / Q13c).

**Done when:**
- [ ] User-facing Bake naming renamed to Pull where it matters (menus, asset menu, InfoBoxes)
- [ ] Pull Config holds: spreadsheet id, OAuth settings, one output folder, explicit Config Type list
- [ ] Tab resolution: exact type name, or optional tab-override attribute on the type
- [ ] No per-tab output path; assets write as `{TypeName}.asset` under the shared folder

**Depends on:** T1 helpful but not required

---

## T3 — Type-driven Vertical Nest parser + fixture tests

**Why:** Nested real data without CsvReader clone or Luban (Q12 / Q4 / Q8 / Q9 / Q15 / Q16).

**Done when:**
- [ ] Parser reads in-memory grid + Config Type shape
- [ ] Supports Preamble skip, Header Row match (field name or Column Alias), `!!!` Ignore Marker
- [ ] Vertical Nest for struct arrays and primitive arrays
- [ ] Fixture tests include weapons → upgrade levels → bonus stats with Preamble + `!!!`
- [ ] Failures can surface enough info for T5’s Pull report (tab + cell coords)

**Depends on:** none (pure logic; can develop against fixtures)

---

## T4 — Sheets API → SO write (stable GUID)

**Why:** Comma-safe transport + runtime final SO (Q11 / Q17 / Q2 / Q6 / Q18).

**Done when:**
- [ ] Pull uses Sheets API cell values in memory (CSV not required to parse)
- [ ] Creates missing Baked Assets; re-Pull overwrites data in place (GUID stable)
- [ ] Config Types are Odin `SerializedScriptableObject` subclasses in the sample
- [ ] OAuth Sign In remains the documented happy path

**Depends on:** T2, T3

---

## T5 — Header Emitter + Pull report

**Why:** Little friction per sheet; debuggable Pull (Q14 A+B).

**Done when:**
- [ ] Header Emitter outputs Header Row from a Config Type (field names / aliases; notes how to mark `!!!` columns)
- [ ] Pull report logs success summary and failures with sheet coordinates when possible

**Depends on:** T3 (emitter can ship earlier); report needs T4 wiring

---

## T6 — DevHost sample + package README

**Why:** Install-and-run template quality.

**Done when:**
- [ ] Nested sample (weapons-style) documented end-to-end
- [ ] README matches SPEC: one workflow, no Luban primary path
- [ ] SPEC Acceptance checklist can be checked off for the sample

**Depends on:** T1–T5

---

## Suggested GitHub issue titles (copy/paste)

1. `[DataConfig] T1 Strip Luban and obsolete bake paths`
2. `[DataConfig] T2 Pull Config: type list, one folder, name match`
3. `[DataConfig] T3 Vertical Nest parser + fixture tests`
4. `[DataConfig] T4 Sheets API Pull → SO (stable GUID)`
5. `[DataConfig] T5 Header Emitter + Pull report`
6. `[DataConfig] T6 DevHost sample + README for Pull → SO`
