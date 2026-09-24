# Context Map

## Contexts

- [Quests](./CONTEXT.md): authored goals, progress tracking, claim signals (`com.aerisyn.quests`)
- [Data Config](./Packages/com.aerisyn.dataconfigsheet/CONTEXT.md): Google-authored game tables pulled into ScriptableObject assets (`com.aerisyn.dataconfigsheet`) — see also [SPEC](./Packages/com.aerisyn.dataconfigsheet/SPEC.md), [TICKETS](./Packages/com.aerisyn.dataconfigsheet/TICKETS.md), [ADR 0010](./docs/adr/0010-dataconfigsheet-pull-scriptableobject.md)

## Relationships

- **Quests ↔ Data Config**: none yet; quest definitions may later be authored as config tables, but Quests does not depend on Data Config today
