# Idle-Axolotl tutorial: keep / avoid (critique, not a blueprint)

Source: `jackthh/Idle-Axolotl` @ `dev_IL/main_dev` (`IdleLotl/Assets/_Game/Scripts/Runtime/Tutorial/`).
Purpose: inform Aerisyn `com.aerisyn.tutorial` grill. Steal concepts; do not copy structure.

## How it works (workflow)

1. Game builds a list of `TutorialStepController` instances and calls `BaseTutorialManager.SetupTutorialSteps`.
2. Steps that already look “done” via `CheckConditionAndAutoComplete()` are skipped and marked complete in player tracking (catch-up / save repair).
3. `TurnOnTutorial` walks remaining steps in order. Each step runs a list of `TutorialSubStepController`s (`OnEnter` → wait for player/UI → `NextToSubTutorial` → `OnExit`).
4. Completing a step writes `TutorialId` into `PlayerTrackingOperation` and saves.
5. Gating UI: block canvas, black mask + hole (“focus”), raise target button `Canvas` sorting into a Tutorials layer, restrict input / back key, point hand, optional talk UI.
6. Soft tutorials are a **separate id track** (`TutorialSoftId` + `CompleteSoftTutorialMessage`), not Soft/Hard modes on the same step. “Hand” MonoBehaviours toggle reminder graphics from hard and/or soft completion + world/stage gates.

## Structure

| Piece | Role |
|---|---|
| `Base/BaseTutorialManager` | Orchestrator + presentation helpers (mask, hand, talk, button registry) |
| `Base/TutorialStepController` / `TutorialSubStepController` | Imperative step/substep state machines |
| `Base/TutorialKey` | String keys for registered UI buttons |
| `Base/TutorialId` vs `TutorialSoftId` | Two completion namespaces (“hard” flow vs soft reminders / packs) |
| `Base/*TutorialHand` | Scene widgets that show/hide by completion |
| `Tutorial_MapN/…`, `Tutorial_Pets/…`, etc. | **One C# class per substep**, content authored in code |
| Prefabs `tutorial_manager`, `Quick_Tutorial Variant` | Heavy MonoBehaviour singleton + UI |

## Concepts worth keeping

- **Two layers of progress**: coarse Tutorial id completion, fine SubStep sequence inside one Tutorial.
- **Catch-up / auto-complete**: if player state already satisfied the teaching goal, mark complete and skip (avoids sticky gates after save/progress drift).
- **Named UI targets** (`DefineButton` / `TutorialKey`): runtime registers RectTransforms; steps look up by key instead of hard scene references in the step list (still game-coupled, but a clean seam).
- **Soft as a separate track** (if you want both): non-blocking reminders / IAP intros that do not occupy the forced flow. Distinct from Soft *enforcement* on a Step.
- **Signals for UI**: start/complete/turn-off messages so HUD can react without polling the manager forever.
- **Presentation toolkit pattern**: focus mask, hand, text, block overlay as helpers the game owns (matches Aerisyn “Hint presentation stays in the game”).

## Concepts / smells to avoid

- **Content as code**: every map/feature tutorial is a folder of hand-written SubStep classes. Designers cannot “easily create” anything without a programmer; fights Aerisyn package goals.
- **God manager owns presentation**: `BaseTutorialManager` (~1k+ lines) mixes flow, save, mask math, talk UI, input restrict, camera/scroll. Hard to package or test.
- **Soft/Hard naming collision**: in Idle-Axolotl, Soft/Hard ≈ **two tutorial catalogs** + HUD hand widgets, not per-step enforcement. Aerisyn glossary already locked Soft/Hard as **Enforcement per Step**. Keep that distinction explicit so language does not drift back.
- **Giant enums** (`TutorialId`, `TutorialSoftId`, `TutorialKey`) as the authoring surface: every new beat requires enum edits and rebuilds.
- **Magic id factories** in `TutorialConstants` (world*10000+stage, IAP offsets): opaque, easy to collide.
- **Runtime mutates UI hierarchy** (`AddComponent<Canvas>` / `GraphicRaycaster` on live buttons): fragile, sorting-layer fights, hard to undo cleanly.
- **Deep game coupling** in SubSteps (Firebase, specific modals, NotificationCheckFactory, ResourceId): cannot ship as a game-agnostic UPM core.
- **Singleton MonoBehaviour** as the product API: opposite of Quests-style pure C# Tracker.

## Open questions for Aerisyn grill

1. Do we want Idle-Axolotl’s **second Soft track** (reminders/offers) *in addition to* Soft Enforcement on Steps, or only Enforcement?
2. Keep catch-up / auto-complete as a first-class package concept?
3. Button registry as a package seam (opaque string/int targets) vs game reports only?
4. SubStep as a domain term, or flatten to Step only?
