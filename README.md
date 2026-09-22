# Aerisyn Moon

Modular Unity packages (`com.aerisyn.*`) for reusable gameplay systems.

**Unity:** 2022.3 LTS and newer  
**Install:** Git URL via Package Manager (path-based monorepo)

## Packages

| Package | Description | Status |
|---|---|---|
| [`com.aerisyn.quests`](Packages/com.aerisyn.quests) | Game-agnostic quest tracker: definitions, board-scoped progress, claim rules, UI events | `0.1.0` |

## Install a package

1. Open **Window → Package Manager**
2. Click **+ → Add package from git URL…**
3. Paste a URL below

### Quests

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.quests
```

Pin a git tag when you release:

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.quests#v0.1.0
```

Private repo: configure Git credentials on the machine so Unity can clone.

## Repo layout

```text
CONTEXT.md                # shared domain glossary
docs/adr/                 # architecture decisions
Packages/
  com.aerisyn.quests/     # one UPM package (own package.json + asmdefs)
```

Each package is independently versioned and installable. Add new systems as sibling folders under `Packages/` with the `com.aerisyn.*` id.

## Adding a package

1. Create `Packages/com.aerisyn.<name>/` with `package.json`, Runtime/Editor asmdefs, README, CHANGELOG
2. Keep game-specific code out of the package
3. Document the Git install URL in this root README
4. Tag releases as `com.aerisyn.<name>@x.y.z` (and optionally a matching `vX.Y.Z` git tag)
