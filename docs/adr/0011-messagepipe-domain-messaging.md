# MessagePipe is the primary domain messaging bus

Aerisyn’s primary solution for **code↔code domain signals** is [Cysharp MessagePipe](https://github.com/Cysharp/MessagePipe): typed publish/subscribe (`IPublisher<T>` / `ISubscriber<T>`), production wiring in code, Inspector used only for debug/raise helpers. Channel identity is the CLR message type, not ScriptableObject assets or string topic names.

**Staged adoption (deliberate):**

1. **First cut:** install MessagePipe in DevHost / a consuming game; prefer MessagePipe’s Builtin container (or equivalent) so VContainer is not required to land the bus. Existing package APIs that already use C# `event` (e.g. Quests `QuestTracker`) stay as-is until a later migration ticket.
2. **Target:** `com.aerisyn.*` packages **may** depend on MessagePipe the same way they may assume shared tooling prerequisites; games adapt and expand usage from there.
3. **VContainer:** allowed later as general DI; not part of the MessagePipe decision. Reopen in a separate ADR when adopted.

**Floor constraints:** EditMode / pure C# tests must be able to publish and subscribe without a scene or ScriptableObject assets. Feel / MMFeedbacks must not carry domain signals. No stringly-typed global topic bus as the primary API.

**Considered options:** Obvious Game SOAP; ScriptableObjectArchitecture / Hipple SO Raise (as in Tap-Tap-Color, Pixel-Shooter, Dice-Puzzle); Feel `MMEventManager`; Idle-Axolotl’s [ZBase.Foundation.PubSub](https://github.com/Zitga-Tech/ZBase.Foundation.PubSub) (`WorldMessenger`); thin C# events + Aerisyn facade; MessagePipe + R3 as the bus itself.

**Why MessagePipe:** matches typed code-first domain signals and EditMode testing; Cysharp ecosystem (pairs with UniTask, ADR 0012); richer path into filters / async / mediator / diagnostics than ZBase; avoids locking Aerisyn Core to Idle’s static `WorldMessenger` shape or to designer SO-channel architectures.

**Why not the others as primary:** SOAP and SO Architecture optimize for asset-channel / Inspector wiring. Feel is juice tooling (Tap/Pixel optionally bridge SO Raise into `MMEventManager`). ZBase already works in Idle and needs no DI, but is a smaller ecosystem and would fork Aerisyn away from Cysharp; keep it game-local. Thin C# events remain fine at package boundaries until migration. R3 is stream composition, not the domain bus.

**Research trail:** [`.scratch/messaging/research-domain-messaging-options.md`](../../.scratch/messaging/research-domain-messaging-options.md).

**Revisit when:** VContainer (or another DI container) is adopted house-wide; a package must ship without MessagePipe; or two+ shipped games prove ZBase-shaped static messengers are enough and MessagePipe’s DI surface is unused cost.
