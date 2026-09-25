Status: ready-for-agent

# Spec: MessagePipe domain messaging (first cut)

Feature slug: `messaging`  
Context: cross-cutting (DevHost / consuming games; not yet a dedicated CONTEXT.md)  
ADR: 0011 (MessagePipe primary bus), 0012 (UniTask shared prerequisite)  
Research: [`.scratch/messaging/research-domain-messaging-options.md`](./research-domain-messaging-options.md)

## Problem Statement

Aerisyn needs one clear primary way for **code↔code domain signals** across packages and games. Today that role is undecided: Quests uses thin C# `event`s; other studio games use ScriptableObject Raise assets, Feel’s `MMEventManager`, or Idle-Axolotl’s ZBase PubSub. Without a locked bus, new packages invent incompatible patterns, EditMode tests drag in scenes or SO assets, and designers get Inspector channels that should stay out of domain logic. The team wants a typed, code-first solution that can later be shared like an Odin-style prerequisite, without adopting VContainer in the same change.

## Solution

Adopt **Cysharp MessagePipe** as the primary domain messaging bus (ADR 0011), with **UniTask** as a mandatory shared prerequisite on any Unity project that installs it (ADR 0012). First cut wires MessagePipe in **DevHost** (or an equivalent consuming game) using MessagePipe’s **Builtin** container and `GlobalMessagePipe` so VContainer is not required. Channel identity is the **CLR message type**. Production publish/subscribe happens in code; Inspector is only for optional debug/raise helpers. Existing package C# events (for example Quests `QuestTracker`) stay unchanged until a later migration. Feel, SO Architecture, and ZBase stay out of Core domain signals. Automated proof is one **Builtin publish/subscribe/dispose** seam with no scene and no ScriptableObject assets.

## User Stories

1. As a gameplay programmer, I want a single primary bus for domain signals, so that I do not invent a new messaging style per package.
2. As a gameplay programmer, I want messages identified by CLR type, so that channels are discoverable in the IDE and refactor-safe.
3. As a gameplay programmer, I want to publish a typed message from code, so that domain logic does not depend on scene wiring.
4. As a gameplay programmer, I want to subscribe to a typed message from code, so that handlers stay explicit and testable.
5. As a gameplay programmer, I want to dispose a subscription, so that handlers stop receiving after teardown.
6. As a gameplay programmer, I want sync `IPublisher<T>` / `ISubscriber<T>` available in the first cut, so that simple domain signals do not require async ceremony.
7. As a gameplay programmer, I want the option to use async publish/subscribe later, so that UniTask-based flows can join the same bus without a second system.
8. As a package author, I want MessagePipe treatable as a shared prerequisite (like Odin), so that `com.aerisyn.*` packages may depend on it in a later stage without vendoring it into the monorepo.
9. As a package author, I want Quests C# events left alone in the first cut, so that first delivery does not force a risky API migration.
10. As a DevHost maintainer, I want MessagePipe installed in DevHost’s UPM manifest, so that the monorepo has a working reference consumer.
11. As a DevHost maintainer, I want UniTask installed alongside MessagePipe, so that the Unity MessagePipe stack is complete (ADR 0012).
12. As a DevHost maintainer, I want Builtin container + `GlobalMessagePipe` bootstrap for the first cut, so that VContainer is not a blocker to land the bus.
13. As a studio tech lead, I want VContainer deferred to a separate ADR, so that DI adoption does not tangle with messaging adoption.
14. As a QA engineer, I want EditMode or pure C# tests that publish and subscribe without a scene, so that CI can lock the bus contract cheaply.
15. As a QA engineer, I want those tests to avoid ScriptableObject message assets, so that domain messaging does not depend on AssetDatabase.
16. As a QA engineer, I want one primary automated seam (Builtin bootstrap → publish → subscribe → dispose), so that the suite stays small and high-signal.
17. As a designer, I want production domain signals not owned by Inspector SO channels, so that gameplay contracts stay in code.
18. As a designer, I want optional Editor debug/raise helpers later if needed, so that I can poke a message without making SO assets the channel identity.
19. As a feel/juice programmer, I want Feel / MMFeedbacks kept off domain signals, so that feedback tooling does not become the game’s event bus.
20. As a maintainer of Idle-Axolotl patterns, I want ZBase PubSub to remain game-local, so that Aerisyn Core does not fork onto Idle’s static messenger shape.
21. As a maintainer of Tap/Pixel/Dice patterns, I want SO Architecture Raise demoted for Core domain logic, so that asset-channel designs do not become Aerisyn’s primary API.
22. As an architect, I want stringly-typed global topic names rejected as the primary API, so that the bus stays typed.
23. As an architect, I want R3 not used as the domain bus itself, so that stream composition stays separate from publish/subscribe contracts.
24. As a future package migrator, I want a clear target stage where `com.aerisyn.*` may depend on MessagePipe, so that migration tickets have a known end state.
25. As a future package migrator, I want keyed publishers (`TKey, TMessage`) available when needed later, so that filtered channels do not need a second library.
26. As a consumer game programmer, I want documentation that states “install MessagePipe + UniTask,” so that half-configured projects are rare.
27. As a consumer game programmer, I want a minimal DevHost sample that publishes and receives one domain message, so that I can copy the bootstrap pattern.
28. As a consumer game programmer, I want the sample to show dispose/lifetime, so that I do not leak subscriptions.
29. As a CI engineer, I want fixture-style tests modeled after Data Config’s Vertical Nest suite, so that messaging tests match monorepo testing culture.
30. As a monorepo agent, I want ADR 0011 and 0012 treated as normative, so that implementation tickets do not reopen the vendor choice.
31. As a monorepo agent, I want research notes kept as the decision trail, so that “why not SOAP/Feel/ZBase” stays answerable.
32. As a package consumer, I want first-cut scope limited to DevHost wiring + seam tests + docs, so that delivery stays focused.
33. As a runtime programmer, I want message payloads to be ordinary CLR types (structs/classes), so that domain models are not forced into UnityEngine types.
34. As a runtime programmer, I want multiple subscribers to receive the same published message, so that fan-out works for domain reactions.
35. As a runtime programmer, I want publishing with zero subscribers to be safe, so that producers are not coupled to listener presence.
36. As a tech lead, I want Feel bridges (SO Raise → MMEventManager) explicitly out of scope as Core messaging, so that juice stays optional and local.
37. As a tech lead, I want “thin C# events at package boundaries until migration” preserved, so that existing Quests APIs remain valid.
38. As a future DI adopter, I want Builtin/`GlobalMessagePipe` documented as the first-cut path, so that moving to VContainer later is an upgrade, not a rewrite of message types.
39. As a documentation reader, I want domain language (“domain signals,” “code↔code,” “Builtin first cut”) used consistently in tickets, so that agents and humans share vocabulary.
40. As a release owner, I want no promise that every `com.aerisyn.*` package already depends on MessagePipe in v0, so that staged adoption stays honest.
41. As a debugger, I want failure modes for missing bootstrap (using publishers before GlobalMessagePipe is set) to be obvious in sample/docs, so that silent no-ops are avoided where MessagePipe would throw or no-op.
42. As a multi-game studio, I want the bus choice reusable across games, so that each new title does not re-litigate SOAP vs MessagePipe vs ZBase.
43. As an EditMode test author, I want tests to boot their own Builtin container (or reset GlobalMessagePipe safely), so that tests do not depend on Play Mode scene objects.
44. As an implementation agent, I want out-of-scope items listed so I do not migrate Quests, add VContainer, or vendor MessagePipe into `Packages/` by accident.

## Implementation Decisions

- Follow ADR 0011: MessagePipe is the primary bus for code↔code domain signals; CLR type is channel identity; production wiring is code-first; Inspector only for optional debug/raise helpers.
- Follow ADR 0012: any Unity project that installs MessagePipe must also install UniTask (`com.cysharp.unitask`).
- Staged adoption for this spec’s delivery (first cut only):
  1. Install MessagePipe + UniTask in DevHost (or the designated consuming Unity project in this monorepo).
  2. Bootstrap with MessagePipe Builtin container and `GlobalMessagePipe` (no VContainer).
  3. Provide a minimal sample: define one example message type, publish it, subscribe in code, dispose cleanly.
  4. Add automated tests on the single Builtin publish/subscribe/dispose seam.
  5. Document install + bootstrap + “existing package events stay until migration.”
- Target stage (document, do not implement in this spec): `com.aerisyn.*` packages may depend on MessagePipe as a shared prerequisite; individual package migrations are separate tickets.
- VContainer (or other DI) is explicitly deferred; reopen only under a new ADR.
- Do not vendor MessagePipe or UniTask source into `Packages/`; install via UPM/git like other external prerequisites.
- Do not change Quests (or other packages’) public C# `event` APIs in this delivery.
- Do not introduce SOAP, ScriptableObjectArchitecture Raise assets, Feel `MMEventManager`, ZBase PubSub, R3-as-bus, or string topic buses as Core domain messaging.
- Optional Editor debug/raise helpers are allowed later as thin adapters over the typed bus; they must not redefine channel identity as assets or strings.
- Message types for the sample/tests should be plain CLR types suitable for EditMode/pure C# use (avoid forcing UnityEngine-only payloads in the seam tests).
- CONTEXT-MAP / a dedicated Messaging context glossary may be added when domain terms stabilize; this spec may ship without a new CONTEXT.md if ADRs + this file remain the contract.

## Testing Decisions

- Good tests assert external behavior only: after Builtin bootstrap, a subscriber receives a published typed message; after dispose, it does not. Do not assert MessagePipe internals, DI registration call graphs, or Inspector helpers.
- **Primary seam (one, confirmed):** MessagePipe Builtin container + `GlobalMessagePipe` → publish `T` → subscribe → receive → dispose → no further receive. No scene. No ScriptableObject message assets. No VContainer.
- Modules under test: the DevHost (or first-cut consumer) bootstrap/sample path and the seam test assembly that exercises that contract. Not Quests Core. Not Feel/SO/ZBase adapters.
- Prior art: Data Config `Tests~/VerticalNest.Tests` fixture style (NUnit, high seam, no live Google / minimal Unity surface). Prefer the same culture: small fixture suite, one seam, CI-friendly EditMode or pure C#.
- Smoke in Editor Play Mode for the DevHost sample is optional and secondary; it must not expand the automated seam surface for v0.

## Out of Scope

- Migrating Quests `QuestTracker` (or any existing package) from C# `event` to MessagePipe
- Adding MessagePipe as a dependency of any `com.aerisyn.*` package (target stage; separate tickets)
- Adopting VContainer / LifetimeScope as the house DI (separate ADR)
- Vendoring MessagePipe or UniTask into the monorepo `Packages/` tree
- Making SOAP, SO Architecture, Feel, or ZBase PubSub the primary Aerisyn domain bus
- Using R3 or UniRx as the domain bus
- Stringly-typed global topic APIs as the primary surface
- Full mediator/filter/diagnostics productization beyond what MessagePipe already provides out of the box for the first cut
- Multiplayer / networked messaging, persistence, or cross-process buses
- Designer-facing SO channel authoring as the production contract
- Reworking Idle-Axolotl (or other private games) onto MessagePipe in this repo

## Further Notes

- Decision trail: research note under `.scratch/messaging/`; normative locks in ADR 0011 and ADR 0012.
- First cut intentionally favors learning MessagePipe without stacking VContainer; Builtin/`GlobalMessagePipe` is the on-ramp, not the forever DI story.
- When package migration begins, prefer preserving package-boundary clarity: message types that cross packages should live where the domain owns them, not in a junk drawer of global DTOs.
- If implementation discovers MessagePipe Unity package IDs or Builtin bootstrap details that need a house convention, capture them in a short follow-up note or ADR amendment rather than silently diverging from 0011/0012.
