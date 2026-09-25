# Idle-Axolotl tutorial: keep / avoid (critique, not a blueprint)

Source: `jackthh/Idle-Axolotl` @ `dev_IL/main_dev` (`IdleLotl/Assets/_Game/Scripts/Runtime/Tutorial/`).
Purpose: inform Aerisyn `com.aerisyn.tutorial` grill. Steal concepts; do not copy structure.
Also cross-checked via [Map Idle-Axolotl Tutorial keep/avoid](bc-a362dde8-a070-5877-9e57-334ca5a74025).

## How it works (workflow)

1. **Bootstrap** (`GameplayManager+SetupTut.cs`): on stage load, build a flat `List<TutorialStepController>` from world/stage (and event-offer mode), then `SetupTutorialSteps` + `TurnOnTutorial`. Remote review/IAP flag can skip all of this.
2. Steps that already look “done” via `CheckConditionAndAutoComplete()` are skipped and marked complete in player tracking (catch-up / save repair).
3. `TurnOnTutorial` walks remaining steps in order. Each step runs a list of `TutorialSubStepController`s (`OnEnter` → wait for player/UI → `NextToSubTutorial` → `OnExit`).
4. Completing a step writes `TutorialId` into `PlayerTrackingOperation` (`CompletedTutorialIds`, or event-offer `TutCompleteIds`) and saves. Completions publish `CompleteTutorialMessage`.
5. Gating UI: block canvas, black mask + hole (“focus”), raise target button `Canvas` sorting into a Tutorials layer, restrict input / back key, point hand, optional talk UI.
6. Soft tutorials are a **separate id track** (`TutorialSoftId` + `CompletedSoftTutorialIds` + `CompleteSoftTutorialMessage`), **not** the hard step machine. Soft is flags + hand/toggle widgets that nudge while hard is not showing. No Soft Soft/Hard modes on the same step.
7. Stage conversations are partly data-driven via `ConversationTutConfig`; interactive focus lessons stay C#.

## Structure

| Piece | Role |
|---|---|
| `GameplayManager+SetupTut` | Imperative catalog: which steps exist for this world/stage |
| `Base/BaseTutorialManager` | Orchestrator + presentation helpers (mask, hand, talk, button registry) |
| `Base/TutorialStepController` / `TutorialSubStepController` | Imperative step/substep state machines |
| `Base/TutorialKey` | String keys for registered UI buttons |
| `Base/TutorialId` vs `TutorialSoftId` | Two completion namespaces (“hard” flow vs soft reminders / packs) |
| `Base/*TutorialHand`, `ToggleWhen*` | Scene widgets that show/hide by completion / soft-vs-hard policy |
| `Tutorial_MapN/…`, `Tutorial_Pets/…`, etc. | **One C# class per substep**, content authored in code |
| Prefabs `tutorial_manager`, `Quick_Tutorial Variant` | Heavy MonoBehaviour singleton + UI; Quick is a thin parallel shell |
| `Data/Csv/Tutorial/tutorial_config.csv` | Stale names; **not** the live enum-driven pipeline |

## Concepts worth keeping

- **Two layers of progress**: coarse Tutorial id completion, fine SubStep sequence inside one Tutorial.
- **Catch-up / auto-complete**: if player state already satisfied the teaching goal, mark complete and skip (avoids sticky gates after save/progress drift).
- **Named UI targets** (`DefineButton` / `TutorialKey`): runtime registers RectTransforms; steps look up by key instead of hard scene references in the step list (still game-coupled, but a clean seam).
- **Soft as a separate track** (if you want both): non-blocking reminders / IAP intros that do not occupy the forced flow. Distinct from Soft *enforcement* on a Step.
- **Signals for UI**: start/complete/turn-off messages so HUD can react without polling the manager forever.
- **Presentation toolkit pattern**: focus mask, hand, text, block overlay as helpers the game owns (matches Aerisyn “Hint presentation stays in the game”).
- **Data-driven talk / stage intros** (`ConversationTutConfig`) vs one C# class per dialogue line.
- **Review / kill-switch** so tutorials can be disabled for store review builds.

## Concepts / smells to avoid

- **Content as code**: every map/feature tutorial is a folder of hand-written SubStep classes (plus dead/commented lessons still in tree). Designers cannot “easily create” anything without a programmer; fights Aerisyn package goals.
- **Imperative catalog in bootstrap**: `SetupTut` hardcodes stage thresholds and constructors; order/content edits require shipping code. Abandoned CSV is a second, stale source of truth.
- **God manager owns presentation**: `BaseTutorialManager` (~1k+ lines) mixes flow, save, mask math, talk UI, input restrict, camera/scroll. Hard to package or test.
- **Soft/Hard naming collision**: in Idle-Axolotl, Soft/Hard ≈ **two tutorial catalogs** + HUD hand widgets, not per-step enforcement. Aerisyn glossary already locked Soft/Hard as **Enforcement per Step**. Keep that distinction explicit so language does not drift back.
- **Soft has no shared step runtime**: only completion flags + hand combinators (`SoftAndIgnoreHard…`, `HardAndSoft…`) encoding policy in MonoBehaviour graphs.
- **Giant enums** (`TutorialId`, `TutorialSoftId`, `TutorialKey`) as the authoring surface: every new beat requires enum edits and rebuilds.
- **Magic id factories** in `TutorialConstants` (world*10000+stage, IAP offsets): opaque, easy to collide.
- **Availability always true** in tracking; real gating is scattered across setup list, auto-complete, and unlock config.
- **Special-case save rules**: mini-game ids never “completed”; event-offer dual lists; some ids ignored on complete.
- **Runtime mutates UI hierarchy** (`AddComponent<Canvas>` / `GraphicRaycaster` on live buttons): fragile, sorting-layer fights, hard to undo cleanly.
- **Talk typing via shared manager int** (`CurrentTypeState`), not a dedicated talk controller.
- **Deep game coupling** in SubSteps (Firebase, specific modals, NotificationCheckFactory, ResourceId): cannot ship as a game-agnostic UPM core.
- **Singleton MonoBehaviour** as the product API: opposite of Quests-style pure C# Tracker.

## Open questions for Aerisyn grill

1. Do we want Idle-Axolotl’s **second Soft track** (reminders/offers) *in addition to* Soft Enforcement on Steps, or only Enforcement? (see Q2b)
2. Keep catch-up / auto-complete as a first-class package concept?
3. Button registry as a package seam (opaque string/int targets) vs game reports only?
4. SubStep as a domain term, or flatten to Step only?
5. Unit of persist: whole Tutorial, Step, or SubStep (mid-quit behavior)?
6. Soft packs / marketing ids (`PACK_*`, rating): in-package “tutorials” or a separate feature-intro system outside this package?
