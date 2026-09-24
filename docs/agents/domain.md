# Domain Docs

How the engineering skills should consume this repo's domain documentation when exploring the codebase.

## Before exploring, read these

- **`CONTEXT-MAP.md`** at the repo root: it points at one `CONTEXT.md` per context. Read each one relevant to the topic.
- Per-context glossaries (today): root `CONTEXT.md` (Quests), `Packages/com.aerisyn.dataconfigsheet/CONTEXT.md` (Data Config), `Packages/com.aerisyn.tutorial/CONTEXT.md` (Tutorial, scaffold).
- **`docs/adr/`**: read ADRs that touch the area you're about to work in. Package-scoped ADRs may appear later under a package `docs/adr/` if needed.

If any of these files don't exist, **proceed silently**. Don't flag their absence; don't suggest creating them upfront. The `/domain-modeling` skill (reached via `/grill-with-docs` and `/improve-codebase-architecture`) creates them lazily when terms or decisions actually get resolved.

## File structure

Multi-context repo (this repo):

```
/
├── CONTEXT-MAP.md
├── CONTEXT.md                              ← Quests context
├── docs/adr/                               ← system-wide decisions
└── Packages/
    ├── com.aerisyn.dataconfigsheet/
    │   └── CONTEXT.md                      ← Data Config context
    └── com.aerisyn.tutorial/
        └── CONTEXT.md                      ← Tutorial context (scaffold)
```

## Use the glossary's vocabulary

When your output names a domain concept (in an issue title, a refactor proposal, a hypothesis, a test name), use the term as defined in the relevant `CONTEXT.md`. Don't drift to synonyms the glossary explicitly avoids.

If the concept you need isn't in the glossary yet, that's a signal: either you're inventing language the project doesn't use (reconsider) or there's a real gap (note it for `/domain-modeling`).

## Flag ADR conflicts

If your output contradicts an existing ADR, surface it explicitly rather than silently overriding:

> _Contradicts ADR-0007 (…), but worth reopening because…_
