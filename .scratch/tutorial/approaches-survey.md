# Tutorial approaches survey (beyond SO lists)

Scratch note for the grill. Not a package decision yet. Sources cited where official.

## Locked already (Aerisyn)

- Soft/Hard = Enforcement per Step (no Soft-track catalog)
- Step success via **game Reports**
- Hard lock owned by the **game** (Gate signals)
- Primary authors in practice: **developers**; designer SO is optional later

## Approach catalog

### A. Report-driven step machine (Quests sibling)

Pure C# runtime: open Tutorial → current Step waits for Report → Soft just coaches, Hard emits Gate signals → advance. Authoring can be code, SO, or tables later.

- **Pros**: matches Q4/Q5; testable; game-agnostic; familiar to this monorepo (Quests).
- **Cons**: you still invent authoring UX; not a drop-in Asset Store product.
- **Proven by**: Aerisyn Quests shape; Idle-Axolotl’s *idea* of step/substep + completion (not its god MonoBehaviour).

### B. Code-first fluent / builder API

Developers write Tutorials in C# (builders or step classes). No SO required for v1.

- **Pros**: matches “devs usually author”; fastest to ship; refactors with IDE; easy unit tests.
- **Cons**: designers blocked until a second authoring path exists.
- **Fits**: user’s Q6 reality check.

### C. Data tables (CSV / Google / Luban via Data Config)

Rows = Steps (id, enforcement, report kind, hint key, next). Designers/PMs edit sheets; package interprets.

- **Pros**: non-code edits; already a house skill (`com.aerisyn.dataconfigsheet`).
- **Cons**: weak for branching / weird waits; easy to invent a second Soft-track by accident; couples Tutorial to Data Config if baked into the package.
- **Use as**: optional adapter in the game or a later package feature, not the core runtime.

### D. Unity Timeline + Signals

Time-based sequences; Signals fire game callbacks ([Unity Timeline Signals](https://unity.com/blog/engine-platform/how-to-use-timeline-signals); [Gameplay Sequence sample](https://docs.unity3d.com/Packages/com.unity.timeline@6.7/manual/samp-gameplay-demo.html)).

- **Pros**: excellent for cutscenes, forced camera, multi-track presentation.
- **Cons**: poor fit for Soft “wait until the player upgrades sometime this session”; heavy scene coupling; not a clean UPM domain core.
- **Use as**: game-side presentation for a Hard Step, not the tutorial brain.

### E. Unity Behavior graphs

Visual BT/FSM hybrid ([Unity Behavior](https://docs.unity3d.com/Packages/com.unity.behavior@1.0/manual/behavior-graph.html)).

- **Pros**: graph authoring, conditions, subgraphs.
- **Cons**: AI-oriented dependency; overkill for “report → next step”; designers still need node literacy; package would depend on Behavior.
- **Use as**: unlikely for Aerisyn Tutorial core.

### F. Unity Tutorial Framework (IET)

[`com.unity.learn.iet-framework`](https://docs.unity3d.com/Packages/com.unity.learn.iet-framework@6.0/manual/index.html) = **in-Editor** tutorials for humans learning Unity.

- **Do not use** for in-game player tutorials. Wrong product.

### G. Drive tutorials from Quests

Onboarding = Quests + game UI that reacts to claimability / progress.

- **Pros**: one progress engine; Soft≈open quest tip, Hard≈gated until claim/report.
- **Cons**: Tutorial packaging disappears; Quests vocabulary (Claim, Board) may fight Soft/Hard Enforcement; hard coupling ADR.
- **Watch**: only if you want one system forever.

### H. Narrative DSL (Yarn Spinner, Ink)

Dialogue-first flows with external functions for “wait for gameplay event”.

- **Pros**: great talk-heavy onboarding.
- **Cons**: Soft/Hard Enforcement and opaque Reports still need a runtime; another dependency.
- **Use as**: optional talk presenter behind Hint, not the brain.

### I. Teach through level design (no package)

Breath of the Wild–style: the world teaches; UI tutorials minimal.

- **Pros**: best player experience when it works.
- **Cons**: not a reusable UPM; does not replace Soft/Hard tooling when mobile idle *needs* fingers and gates.

## Recommendation for Aerisyn (opinion)

1. **Core = A** (report-driven pure C# step machine + Gate/Hint signals).
2. **Authoring v1 = B** (code-first). SO / Data Config later if pain appears.
3. **Presentation = game adapters** (mask/hand/Timeline clips optional per game).
4. Skip E/F as core; use D/H only as game-side presenters; treat G as a future ADR, not default.

## Open grill question

Pick authoring v1 among B (code-first), A+light SO, C (tables), or something else from above.
