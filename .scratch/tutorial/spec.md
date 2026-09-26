Status: ready-for-agent

# Spec: Tutorial Runner (`com.aerisyn.tutorial`)

Feature slug: `tutorial`  
Context: Tutorial (`com.aerisyn.tutorial`)  
Package: `Packages/com.aerisyn.tutorial`  
Glossary: `Packages/com.aerisyn.tutorial/CONTEXT.md`

## Problem Statement

Game teams need a reusable, easy-to-install way to run Soft and Hard player tutorials without baking presentation, rewards, or save I/O into a god manager. Idle-Axolotl proved Soft/Hard teaching in product, but its shape (content as C# subclasses, dual Soft-track ids, MonoBehaviour orchestrator) does not fit a game-agnostic UPM package. Authors are usually developers; they still want a clear, friendly Core they can code against first, with ScriptableObject authoring later once the Runner is proven.

## Solution

Ship a pure C# **Runner** that runs at most one **Tutorial** at a time. A Tutorial is an ordered list of **Steps**. Each Step has **Enforcement** (Soft or Hard), a **Report** match for success, and optional **Choreography** of opaque **Cues** (sequential await via **Cue Done**, concurrent fire-and-forget, or a mix). The game **Start**s / **Stop**s Tutorials, feeds **Report** and **Cue Done**, and listens for **Cue**, **Gate**, and **Completion** signals. Presentation, input lock, rewards, catch-up, and disk save stay in the game (**Progress Snapshot** is the only save-shaped export). Authoring is code-first builders and/or a thin ScriptableObject (`TutorialAsset`) that projects into the same Tutorial definition. No Quests package dependency.

## User Stories

1. As a gameplay programmer, I want a pure C# Runner with no MonoBehaviour requirement, so that I can unit-test tutorial flow without entering Play Mode.
2. As a gameplay programmer, I want to Start a Tutorial by passing a definition, so that I control when onboarding begins.
3. As a gameplay programmer, I want to Stop an active Tutorial, so that I can abandon or interrupt flow without completing it.
4. As a gameplay programmer, I want at most one Tutorial active at a time, so that Soft/Hard Gates never fight each other.
5. As a gameplay programmer, I want Start to fail clearly if a Tutorial is already active, so that I do not silently overwrite state.
6. As a content author (developer), I want a Tutorial to be an ordered sequence of Steps, so that I can teach multi-beat flows like “level up a character.”
7. As a content author, I want each Step to choose Soft or Hard Enforcement independently, so that one Tutorial can mix coaching and gated teaching.
8. As a player on a Soft Step, I want play to stay unblocked, so that coaching never freezes the game.
9. As a player on a Hard Step, I want the game to be able to lock input/UI until I succeed, so that I am guided through the required action.
10. As a gameplay programmer, I want Gate signals when a Hard Step starts and ends, so that my game can apply and release locks without the package owning InputManager.
11. As a gameplay programmer, I want Soft Steps to emit no Gate lock requirement, so that Soft stays “coach only.”
12. As a gameplay programmer, I want to Report opaque kind (and optional param) when gameplay facts happen, so that Steps advance without scene wiring in the package.
13. As a content author, I want a Step to declare which Report matches success, so that “upgrade the sword” completes when that fact is reported.
14. As a gameplay programmer, I want unmatched Reports to be ignored safely, so that noisy gameplay does not corrupt the active Step.
15. As a content author, I want a Step to optionally list Cues, so that I can request highlight/scale/hide/text without implementing UI in Core.
16. As a content author, I want Choreography to run Cues sequentially (await), so that I can stage presentation like UniTask await chains.
17. As a content author, I want Choreography to run Cues concurrently (forget), so that multiple presentation intents can exist at once.
18. As a content author, I want to mix sequential and concurrent Cues in one Step’s Choreography, so that real tutorial beats are expressible.
19. As a presentation programmer, I want Cue signals with opaque Cue ids when the Runner wants presentation, so that my game binds ids to highlight/scale/hide.
20. As a presentation programmer, I want to send Cue Done for sequential Cues, so that Choreography can advance.
21. As a presentation programmer, I want concurrent Cues not to require Cue Done, so that fire-and-forget stays cheap.
22. As a gameplay programmer, I want Step success to be driven by Report, not by finishing Choreography, so that presentation cannot soft-lock progress.
23. As a gameplay programmer, I want Completion signals when a Step finishes, so that UI or analytics can react per beat.
24. As a gameplay programmer, I want a Completion signal when the whole Tutorial finishes, so that I can grant rewards at the listen site.
25. As a economy/rewards programmer, I want reward contents to stay outside the Tutorial package, so that loot tables are not duplicated in Core.
26. As a gameplay programmer, I want catch-up (“player already did this”) to stay in my game before Start, so that I control whether to run the Tutorial and whether Completion still pays.
27. As a save programmer, I want a Progress Snapshot from the Runner, so that I can persist Tutorial id and Step index in my own save pipeline.
28. As a save programmer, I want to apply a Progress Snapshot when Starting (or restoring), so that my game can resume mid-Tutorial if I choose that policy.
29. As a save programmer, I want the package not to touch disk or PlayerPrefs, so that official save ownership stays in the game.
30. As a content author, I want code-first builders (or equivalent definition construction) in v1, so that I can author Tutorials in C# without waiting for SO tools.
31. As a package consumer, I want definitions to be plain data consumed by the Runner, so that SO authoring and builders project into the same shape.
32. As a package maintainer, I want thin SO authoring (`TutorialAsset`) after Core is proven, so that the definition shape stays stable and there is no second runtime model.
33. As a package consumer, I want no dependency on com.aerisyn.quests in v1, so that I can install Tutorial alone.
34. As a package consumer, I want install via git URL like other Aerisyn packages, so that onboarding matches Quests/Data Config.
35. As a package consumer, I want Odin as the shared DevHost/consumer prerequisite only as needed for assembly consistency, so that Core runtime logic does not call Odin.
36. As a QA engineer, I want tests only through the Runner public API, so that Soft/Hard, Report, Cue/Cue Done, Gate, and Completion behavior stay locked without UI tests.
37. As a QA engineer, I want a test that Hard Step emits Gate on enter and off on leave/complete, so that lock contracts do not regress.
38. As a QA engineer, I want a test that Soft Step does not require Gate lock semantics, so that Soft stays non-blocking at the signal level.
39. As a QA engineer, I want a test that sequential Choreography waits for Cue Done between Cues, so that await semantics stay correct.
40. As a QA engineer, I want a test that concurrent Cues can be issued without Cue Done, so that Forget semantics stay correct.
41. As a QA engineer, I want a test that Report completes the Step even if Choreography is unfinished, so that success stays Report-driven.
42. As a QA engineer, I want a test that a second Start fails while one Tutorial is active, so that single-active is enforced.
43. As a QA engineer, I want a test that Stop clears active state and ends Gates/Cues cleanly enough for the game to unlock, so that abandon is safe.
44. As a sample author, I want a minimal Samples~ or DevHost demo that Starts a Tutorial, Reports, and logs signals, so that consumers see the seam without copying Idle-Axolotl.
45. As a documentation reader, I want README + CONTEXT vocabulary (Tutorial, Step, Soft, Hard, Cue, Choreography, Report, Gate, Completion, Runner, Progress Snapshot), so that agents and humans share language.
46. As a designer, I want SO authoring that does not invent a second runtime model, so that Inspector edits feed the same Runner.
47. As a studio lead, I want the package to avoid Idle-Axolotl’s one-class-per-substep pattern, so that content stays data + generic Runner.
48. As an analytics programmer, I want stable Tutorial and Step identities on Completion, so that I can log funnels without parsing presentation.
49. As a gameplay programmer, I want opaque Report kinds owned by the game (enum cast to int or similar), so that the package does not ship a verb catalog.
50. As a presentation programmer, I want opaque Cue ids owned by the game, so that the package does not ship UI prefabs as API.
51. As a gameplay programmer, I want advancing to the next Step after Completion to start that Step’s Choreography and Gate rules, so that multi-step Tutorials flow automatically once Started.
52. As a player, I want finishing the last Step to Complete the Tutorial, so that rewards and cleanup can run once.
53. As an agent implementing tickets, I want domain terms from CONTEXT.md used in code and tests, so that tickets map cleanly to types.

## Implementation Decisions

- **Primary module:** Tutorial Core Runtime built around a pure C# **Runner** (Quests Tracker spirit; ADR 0003-style: no MonoBehaviour in Core API; samples may add facades later).
- **Single test/product seam:** Runner public surface only: Start, Stop, Report, Cue Done, Progress Snapshot export/apply, and outbound Cue / Gate / Completion signals.
- **Definition model:** Immutable (or replace-on-Start) Tutorial definition = ordered Steps; each Step has Enforcement (Soft|Hard), Report match (opaque kind + optional param / match rules as needed for v1), and optional Choreography (list/tree of Cues with sequential vs concurrent grouping). Builders construct definitions in code for v1.
- **Active set:** At most one active Tutorial. Start while active is an error (or rejected result); Stop abandons without Tutorial Completion reward semantics left to the game (emit enough teardown signals for Gate off / Cue cancel as needed).
- **Soft vs Hard:** Soft = coaching; no Gate lock contract. Hard = emit Gate start when the Step becomes active and Gate end when the Step leaves (complete, stop, or replace). Package never freezes input itself.
- **Report:** Game pushes facts; Runner matches against the active Step only. Success advances Step (Completion signal) and moves to the next Step or Tutorial Completion. Choreography does not gate Report success in v1.
- **Choreography:** On Step enter, Runner schedules Cues. Sequential: emit Cue, wait for Cue Done, then next. Concurrent: emit without waiting. Mix allowed via definition structure. Cue ids opaque.
- **Signals:** Cue (id), Gate (started/ended), Step Completion (tutorial id + step identity), Tutorial Completion (tutorial id). Exact event vs callback shape is an implementation detail behind the Runner seam.
- **Progress Snapshot:** Includes at least Tutorial identity and Step index (and whatever else is required to restore “active or last position” for the game’s policy). No disk I/O in package.
- **Rewards / catch-up:** Out of Core. Game decides Start eligibility and grants on Completion.
- **Dependencies:** No Quests package reference. No Data Config reference. Align with shared Odin prerequisite for Aerisyn packages if the asmdef already expects it; Runner logic must not require Odin at runtime.
- **Authoring v1:** Code-first builders and thin ScriptableObject authoring (`TutorialAsset`) both project into the same Tutorial definition. No second runtime model; no Idle-Axolotl one-class-per-substep pattern.
- **Samples:** Thin sample showing Start → signals → Report → Completion; presentation can be stubs/logs. No port of Idle-Axolotl managers.
- **Versioning:** Advance package past scaffold `0.0.1` when the Runner API is first usable; keep CHANGELOG/README aligned with CONTEXT vocabulary.
- **ADRs (recommended companions, not blockers for coding):** Soft/Hard means Enforcement not dual tracks; completion/rewards/presentation/save stay outside Core; single active Tutorial + game Start/Stop.

## Testing Decisions

- Good tests assert **external behavior at the Runner seam only**: given definitions + a sequence of Start/Report/Cue Done/Stop and snapshot apply, assert signals emitted and resulting snapshot/active state. Do not assert private timers, list internals, or Unity UI.
- **Modules under test:** Runner + definition/builder types needed to construct inputs. No presentation adapters in the automated suite for v1.
- **Prior art:** Data Config `Tests~` NUnit fixtures (parse seam). Quests has no in-package test project today but documents a pure Tracker API; Tutorial should follow Data Config’s `Tests~` layout more than Idle-Axolotl. Prefer pure C# / EditMode-style tests without Play Mode.
- Coverage targets for v1: Soft vs Hard Gate signals; Report advances Step; sequential Cue/Cue Done; concurrent Cues; Report success independent of unfinished Choreography; single-active Start rejection; Stop teardown; Tutorial Completion after last Step; Progress Snapshot round-trip enough to restore Step index.
- Samples/DevHost are smoke illustrations, not the primary regression suite.

## Out of Scope

- ScriptableObject / Inspector authoring beyond the thin `TutorialAsset` projection (custom editors, Addressables catalogs, etc.)
- Quests integration or shared report bus
- Data Config / spreadsheet-authored tutorials
- Package-owned input freeze, mask UI, hand prefabs, talk UI
- Reward tables, currency, inventory grants inside the package
- Core catch-up / auto-skip of already-true Steps
- Multiple concurrent Tutorials
- Package auto-start from world/remote-config rules
- Nested Report-waiting “SubStep” progress machines (Idle-Axolotl style)
- Porting Idle-Axolotl Tutorial_* content folders
- Timeline/Behavior/Yarn as Core dependencies (games may use them only as Cue presenters)

## Further Notes

- Idle-Axolotl critique (keep/avoid): `.scratch/tutorial/idle-axolotl-lessons.md`
- Authoring survey and Q6 decision (2b): `.scratch/tutorial/approaches-survey.md`, `.scratch/tutorial/q6-code-vs-so.md`
- Soft-track reminders are not a second catalog; they are Soft Steps / Soft Tutorials under Enforcement.
- Prefer one Runner seam across the codebase; do not add parallel “manager” APIs in Core.
- Tickets: `.scratch/tutorial/issues/` — frontier starts at **01**; **03**/**04** after 01; **05** after 04; **06** and **02** after 01+03+04+05.

## Comments

- 2026-09-25: Domain grilled; seam confirmed as Runner public API; spec marked ready-for-agent.
- 2026-09-25: `/to-tickets` published 01, 03–06; updated 02 blockers to 01/03/04/05.
