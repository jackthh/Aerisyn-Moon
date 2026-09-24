# Aerisyn Moon

Modular Unity packages (`com.aerisyn.*`) for reusable gameplay systems.

**Unity:** 2022.3 LTS and newer  
**Install:** Git URL via Package Manager (path-based monorepo)

## Shared prerequisites

| Prerequisite | Scope | Notes |
|---|---|---|
| **[Odin Inspector](https://odininspector.com/) (Sirenix)** | **All** `com.aerisyn.*` packages | Not on UPM; not vendored. Install in the consuming project (and DevHost) per machine. Packages may reference `Sirenix.OdinInspector.Attributes`. |
| Package-specific deps | Per package README | See each package README |

## Packages

| Package | Description | Status |
|---|---|---|
| [`com.aerisyn.quests`](Packages/com.aerisyn.quests) | Game-agnostic quest tracker: definitions, board-scoped progress, claim rules, UI events. | `0.1.3` |
| [`com.aerisyn.dataconfigsheet`](Packages/com.aerisyn.dataconfigsheet) | Editor **Pull**: Google Sheet (OAuth) → Vertical Nest → ScriptableObject Baked Assets. One-way; schema in hand-written Config Types. | `0.3.0` |
| [`com.aerisyn.tutorial`](Packages/com.aerisyn.tutorial) | Upcoming tutorial / onboarding gameplay package (scaffold only). | `0.0.1` |

## Install a package

1. Install **Odin Inspector** into your Unity project (required for every Aerisyn package in this repo).
2. Install any **other package-specific prerequisites** (see the package row / README).
3. Open **Window → Package Manager**
4. Click **+ → Add package from git URL…**
5. Paste a URL below

### Quests

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.quests
```

Pin a git tag when you release:

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.quests#v0.1.3
```

Details: [`Packages/com.aerisyn.quests/README.md`](Packages/com.aerisyn.quests/README.md).

### Data Config Sheet

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.dataconfigsheet
```

Pin a release tag when you cut one:

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.dataconfigsheet#com.aerisyn.dataconfigsheet@0.3.0
```

Details: [`Packages/com.aerisyn.dataconfigsheet/README.md`](Packages/com.aerisyn.dataconfigsheet/README.md).

### Tutorial

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.tutorial
```

Details: [`Packages/com.aerisyn.tutorial/README.md`](Packages/com.aerisyn.tutorial/README.md). Scaffold only; pin a release tag when the first usable version ships.

Private repo: configure Git credentials on the machine so Unity can clone.

## DevHost

`DevHost/` is the local Unity project used to exercise packages. **Do not commit Odin** (`Assets/Plugins/Sirenix`); install it on each machine from the Asset Store (or your Sirenix license workflow). **Do not commit Google service-account JSON** under `Assets/AerisynDataConfig/Credentials/`.

## Repo layout

```text
CONTEXT.md                # Quests domain glossary
CONTEXT-MAP.md            # points at per-context glossaries
docs/adr/                 # architecture decisions
DevHost/                  # local Unity host (Odin / credentials installed locally, not in git)
Packages/
  com.aerisyn.quests/           # quest tracker
  com.aerisyn.dataconfigsheet/  # Google Pull → ScriptableObject tooling
  com.aerisyn.tutorial/         # tutorial / onboarding (scaffold)
```

Each package is independently versioned and installable. Add new systems as sibling folders under `Packages/` with the `com.aerisyn.*` id.

## Adding a package

1. Create `Packages/com.aerisyn.<name>/` with `package.json`, Runtime/Editor asmdefs, README, CHANGELOG
2. Keep game-specific code out of the package
3. You may rely on **Odin** already being installed in the consumer / DevHost (do not vendor Sirenix)
4. Document the Git install URL in this root README
5. Tag releases as `com.aerisyn.<name>@x.y.z` (and optionally a matching `vX.Y.Z` git tag)
