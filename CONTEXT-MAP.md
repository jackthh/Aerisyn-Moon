# Context Map

## Contexts

- [Quests](./CONTEXT.md): authored goals, progress tracking, claim signals (`com.aerisyn.quests`)
- [Data Config](./Packages/com.aerisyn.dataconfigsheet/CONTEXT.md): Google-authored game tables pulled into ScriptableObject assets (`com.aerisyn.dataconfigsheet`) — agent spec [`.scratch/dataconfig-pull-so/spec.md`](./.scratch/dataconfig-pull-so/spec.md), [ADR 0010](./docs/adr/0010-dataconfigsheet-pull-scriptableobject.md), [ADR 0013](./docs/adr/0013-dataconfigsheet-local-only-after-pull.md)
- [Tutorial](./Packages/com.aerisyn.tutorial/CONTEXT.md): Shared understanding (Runner / Step / Choreography / Cue) then Soft/Hard glossary; separate from Quests in v1 (`com.aerisyn.tutorial`); agent spec [`.scratch/tutorial/spec.md`](./.scratch/tutorial/spec.md)

## Relationships

- **Quests ↔ Data Config**: none yet; quest definitions may later be authored as config tables, but Quests does not depend on Data Config today
- **Tutorial ↔ Quests**: separate for v1 (no dependency); shared report bus only via future ADR
- **Tutorial ↔ Data Config**: none yet
