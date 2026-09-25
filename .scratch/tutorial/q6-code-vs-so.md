# Q6: code-first (1) vs code-first + thin SO (2)

Not a lock yet. Compare only. Runtime model assumed: one immutable **Tutorial definition** (ordered Steps: Enforcement, Report match, Hint id, etc.) + pure C# runner.

## Shared ground (both options)

- Avoid Idle-Axolotl “one SubStep class per beat.”
- User-friendly ≠ designer-only. Devs also want: clear API, Inspector tweaks, no recompile for order/copy/Soft↔Hard flips.
- Flexibility comes from **data definitions**, not from writing new C# types per Step.

## Option 1: code-first only (builders / C# defs)

```text
Developer → TutorialBuilder / static defs → TutorialDefinition → Runner
```

| Wins | Costs |
|---|---|
| Smallest package surface; ships faster | Order / Soft↔Hard / hint id changes need a code edit + recompile (or you invent ad-hoc JSON anyway) |
| One authoring path to learn | No Inspector story for “open asset, tweak, play” |
| Easy unit tests (build defs in test code) | Feels less “UPM product” next to Quests’ SO authoring |
| No dual-path sync bugs | “Easy to use” for content iteration is weaker |

## Option 2: same definition + thin SO mirror

```text
Developer → Builder ─┐
Designer/dev in Inspector → TutorialAsset (SO) ─┴→ TutorialDefinition → Runner
```

Matches Quests: Core pure C#, Authoring SO that *projects into* Core types (see `QuestAsset` / Tracker).

| Wins | Costs |
|---|---|
| Inspector edit of order, Enforcement, Report kinds, Hint keys without new types | Authoring assembly + Odin drawers to maintain |
| Copy/variant assets per map/feature; Addressables-friendly | If Builder and SO diverge in features, dual source of truth |
| Familiar Aerisyn install story (git URL + drop assets) | Slightly more v1 work than builders alone |
| Devs still code when logic is weird; SO for the 80% list shape | SO does not magically make opaque Report ints “friendly” without good drawers |

## Important correction

“SO adds flexibility instead of hard code” is true **only if** Steps are data. SO that still points at custom `TutorialSubStep_*` MonoBehaviours/classes is **not** more flexible; it is Idle-Axolotl with an asset wrapper.

Flexibility checklist for either option:

1. Step list is data (Enforcement, report kind/param, hint id, optional auto-complete rule id).
2. Runner is generic.
3. Game supplies Reports + Gate/Hint presentation.

## Recommendation

**Prefer 2**, with a hard rule: SO serializes the **same** `TutorialDefinition` the builder produces (round-trip or one-way SO→definition). Code-first remains first-class for tests and weird flows; SO is the friendly default for list-shaped tutorials.

Pick **1** only if you want the absolute smallest v1 and accept recompile for content tweaks.

## Decide

- **2a**: SO in v1 (Authoring asmdef from the start), builders + SO both ship  
- **2b**: Definition + builders in v1; SO in the next ticket once the definition shape stabilizes  
- **1**: builders only until a real pain appears  
