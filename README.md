# Aerisyn Moon

Modular Unity packages (`com.aerisyn.*`) for reusable gameplay systems.

**Unity:** 2022.3 LTS and newer  
**Install:** Git URL via Package Manager (path-based monorepo)

## Packages

| Package | Description | Status |
|---|---|---|
| [`com.aerisyn.quests`](Packages/com.aerisyn.quests) | Game-agnostic quest tracker: definitions, board-scoped progress, claim rules, UI events. Needs [Odin Inspector](https://odininspector.com/) in the project. | `0.1.3` |

## Install a package

1. Install any **package-specific prerequisites** (see the package row / README). Odin is not on UPM; install it in your project when a package needs it. This repo does not vendor Odin (per-machine / per-project install).
2. Open **Window → Package Manager**
3. Click **+ → Add package from git URL…**
4. Paste a URL below

### Quests

**Prerequisite:** [Odin Inspector](https://odininspector.com/) (Sirenix) in the Unity project so `com.aerisyn.quests` can compile. Details: [`Packages/com.aerisyn.quests/README.md`](Packages/com.aerisyn.quests/README.md).

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.quests
```

Pin a git tag when you release:

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.quests#v0.1.3
```

Private repo: configure Git credentials on the machine so Unity can clone.

## DevHost

`DevHost/` is the local Unity project used to exercise packages. **Do not commit Odin** (`Assets/Plugins/Sirenix`); install it on each machine from the Asset Store (or your Sirenix license workflow).

## Repo layout

```text
CONTEXT.md                # shared domain glossary
docs/adr/                 # architecture decisions
DevHost/                  # local Unity host (Odin installed locally, not in git)
Packages/
  com.aerisyn.quests/     # one UPM package (own package.json + asmdefs)
```

Each package is independently versioned and installable. Add new systems as sibling folders under `Packages/` with the `com.aerisyn.*` id.

## Adding a package

1. Create `Packages/com.aerisyn.<name>/` with `package.json`, Runtime/Editor asmdefs, README, CHANGELOG
2. Keep game-specific code out of the package
3. Document the Git install URL in this root README
4. Tag releases as `com.aerisyn.<name>@x.y.z` (and optionally a matching `vX.Y.Z` git tag)
