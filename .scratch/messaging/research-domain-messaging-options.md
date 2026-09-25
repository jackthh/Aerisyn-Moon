# Domain messaging / observer / pub-sub options (Aerisyn Moon)

**Verdict (settled constraints):** For **code↔code domain signals** with **typed C# `Publish<T>` / `Subscribe<T>`**, **production wiring in code**, **Inspector debug/raise only**, and an **Odin-like third-party prerequisite** (not vendored), **Cysharp MessagePipe** is the strongest primary fit among investigated options: DI-managed typed brokers, filters, async, diagnostics, UPM/git install, MIT. **Thin C# events + facade** is the deliberate zero-dependency alternative. **SOAP / Hipple SO Raise** optimize for designer-wired SO channels (wrong primary job). **Feel / MMEventManager** is juice-adjacent tooling that happens to ship a typed struct bus — do not adopt Feel for domain messaging. **R3** is composition of streams, not a domain bus (complement, not substitute). **Idle Axolotl / IdleLotl MessagePipe usage was not publicly findable**; treat MessagePipe-in-Unity docs below as the comparison baseline when that repo is opened.

Settled constraints (not reopened): domain signals primary (not Feel juice); packaging like Odin prerequisite; Editor = debug/raise only; channel identity = typed C# messages. Aerisyn already treats Odin this way ([ADR-0006](../../docs/adr/0006-odin-shared-prerequisite.md), [README](../../README.md)).

---

## Comparison table

| Option | Optimized for | Channel identity | Inspector story | Pure C# / EditMode-friendly? | Install (Odin-like prereq?) | Lock-in / license / maintenance | Fit |
| --- | --- | --- | --- | --- | --- | --- | --- |
| **MessagePipe** | DI-first typed pub/sub + mediator/filters/async | Typed `IPublisher<T>` / `ISubscriber<T>` (+ keyed) | Diagnostics window; not SO wiring | Yes (DI container in tests / BuiltinContainerBuilder) | UPM git or `.unitypackage` + UniTask (+ optional VContainer/Zenject) | MIT; Cysharp; active | **good** — matches typed domain bus |
| **Thin C# events + facade** | Minimal ownership | Typed `event` / `Action<T>` behind your API | Only what you build (e.g. Odin Raise button) | Yes | None | You own it | **good** — deliberate non-library |
| **R3 (Cysharp)** | Reactive operators / frame time / UI streams | Observables / Subjects (not domain broker) | UnityEvent→Observable helpers | Yes (.NET + Unity packages) | UPM git + NuGet core | MIT; UniRx successor | **poor** as primary bus; **good** as optional consumer |
| **UniRx** | Legacy Unity Rx | `MessageBroker` / Subjects | Component helpers | Yes | Archived UPM git | Archived Feb 2024; author pushes R3 | **veto** for new work |
| **Hipple SO Raise / Unity event channels** | Designer-wired SO channels | SO asset identity (± typed payload channel) | First-class Raise / listeners | Awkward (needs SO assets / scene listeners) | Pattern / sample code | Pattern only; you maintain | **poor** — wrong primary job |
| **SOAP** | Low-code SO architecture (vars, events, binds, saves) | Scriptable Event assets + typed concrete SEs | Raise button, debug window, Event Listeners | Weak for pure domain (SO-centric) | Paid Asset Store / itch unitypackage; Odin listed as integration | Commercial EULA; closed product docs | **poor** — SO-channel primary |
| **Feel / MMEventManager** | Game feel / MMTools helpers | Typed **struct** events via static manager; `MMGameEvent` is string-named | `MMGameEventListener` UnityEvent hook | Possible (static manager) but tied to Feel/MMTools | Paid Feel Asset Store (or engines that include MMTools) | Asset Store EULA; juice product | **veto** as domain bus (wrong product) |
| **ExtEvents** | Better UnityEvent inspector listeners | Persistent listeners on components | Rich Inspector method wiring | No (Inspector UnityEvent replacement) | OpenUPM | MIT-ish OSS; SolidAlloy | **veto** — not a domain bus |
| **Messenger (Asset Store)** | Typed Send/Signal + optional SO Channels | Typed `Send(in T)` / `Signal<T>` | Optional SO Channels | Claims pure C# / DI | Paid Asset Store | Commercial; Unity 6–oriented listing | **poor–curious** — verify vs MessagePipe if shortlist expands |
| **Global Message (Curvature / Asset Store “Event System”)** | Hybrid code + editor message flows | Type-safe messages; optional SO representation | Editor trigger/history tools | Claims editor + code | Paid Asset Store | Commercial; smaller ecosystem | **poor** — heavier editor-first than settled need |
| **MediatR-style alone** | Request/response CQRS in .NET | `IRequest` / handlers | N/A | Yes on .NET | NuGet; Unity awkward | MediatR is separate; MessagePipe already embeds mediator | **poor** as Unity primary — use MessagePipe’s request APIs if needed |
| **Idle Axolotl MessagePipe** | (unknown — private?) | (unknown) | (unknown) | (unknown) | (unknown) | **Not publicly findable** | **unknown** |

---

## Per-option notes

### 1. SOAP — ScriptableObject Architecture Pattern

**What it is.** Commercial Unity toolkit by **Obvious Game** that builds gameplay architecture on ScriptableObjects: variables, events, listeners, bindings, lists/dicts, saves, singletons, enums, wizard/type creator. Publisher docs: [obvious-game.gitbook.io/soap](https://obvious-game.gitbook.io/soap). Store: [Asset Store #232107](https://assetstore.unity.com/packages/tools/utilities/soap-scriptableobject-architecture-pattern-232107). Also sold on [itch.io](https://obvious-game.itch.io/soap) (`Soap-3.8.x.unitypackage`, paid). Explicitly points readers to Ryan Hipple’s talk for the underlying idea ([What is Soap?](https://obvious-game.gitbook.io/soap)).

**Optimized for.** Speed/simplicity and **editor-driven decoupling** (“solve dependencies through the editor,” A/B-toggleable features, drag-and-drop reuse) — [Why use Soap?](https://obvious-game.gitbook.io/soap/getting-started/why-use-soap).

**Typed vs SO-channel.** Channels are **Scriptable Event assets** (no-param and with-param variants). Listeners are typed concrete components / `OnRaised` handlers matching the SE type (e.g. `ScriptableEventInt`) — [Scriptable Event](https://obvious-game.gitbook.io/soap/soap-core-assets/scriptable-event), [Registering from code](https://obvious-game.gitbook.io/soap/scene-documentation/4_scriptableevents/registering-to-events-from-code). Not a global `Publish<T>` type bus; identity is the **asset reference**.

**Editor/Inspector.** First-class: Event Listener UnityActions without code ([Event Listener](https://obvious-game.gitbook.io/soap/soap-core-assets/event-listener)); play-mode **Raise** button + Debug Value ([Properties](https://obvious-game.gitbook.io/soap/soap-core-assets/scriptable-event/properties)); Event Debug Window ([Debugging](https://obvious-game.gitbook.io/soap/soap-core-assets/scriptable-event/debugging)). itch lists **Odin** under Integrations ([itch page](https://obvious-game.itch.io/soap)).

**Pure C# / EditMode.** Possible to `+= OnRaised` in code, but the architecture assumes SO assets and often scene listeners — poor match for Aerisyn’s pure Core / fixture-test style (see Quests Core: no UnityEngine).

**Install.** Paid unitypackage (Asset Store / itch). Not UPM-first. Fits “prerequisite installed in consumer” economically, but pulls a **whole SO framework**, not a thin messaging prereq.

**Lock-in / license / maintenance.** Commercial product; docs on GitBook; Discord community; itch shows active updates (v3.8.x). No public first-party source repo found under Obvious Game for the product itself (itch “GitHub” link present; open search did not surface a public SOAP product repo distinct from unrelated “SOAP” projects).

**Fit:** **poor** — Inspector/SO-channel architecture as primary, opposite of “production wiring is code + typed C# bus.”

---

### 2. ScriptableObject events / Raise pattern (Hipple + Unity how-tos)

**What it is.** Unite 2017 “Game Architecture with Scriptable Objects” pattern: `GameEvent` SO holds listeners; `Raise()` notifies; `GameEventListener` MonoBehaviour registers OnEnable/OnDisable and fires a UnityEvent response. Primary sources: [Hipple blog + sample links](http://www.roboryantron.com/2017/10/unite-2017-game-architecture-with.html), sample [`GameEvent.cs`](https://raw.githubusercontent.com/roboryantron/Unite2017/master/Assets/Code/Events/GameEvent.cs), Unity how-to summarizing Hipple ([Architect with ScriptableObjects](https://unity.com/how-to/architect-game-code-scriptable-objects)), modern Unity mini-guide on **event channels** ([Use ScriptableObjects as event channels](https://unity.com/how-to/scriptableobjects-event-channels-game-code)).

**Capabilities vs typed C# bus.**

| | SO Raise / event channel | Typed C# bus (`Publish<T>`) |
| --- | --- | --- |
| Channel identity | Asset reference (and/or concrete typed SO class per payload) | CLR type `T` |
| Wiring | Serialize field to SO; designers add listeners in Inspector | Code subscribe; DI optional |
| Payloads | Paramless `Raise()` in Hipple sample; Unity guide adds `GenericEventChannelSO<T>` with concrete subclasses (open generics not createable as assets) | Any `T` without asset proliferation |
| Debug | Raise from Inspector / custom editors (Unity guide; Hipple: “button in the Inspector”) | Tooling you add, or MessagePipe Diagnostics |
| Scene independence | Project-level SO; survives scene loads | Container/scope lifetime |

Unity’s own guide contrasts **static `GameEvents`** (code-defined, not designer-friendly) vs **SO channels** (Inspector-friendly) and notes patterns are situational ([event channels guide](https://unity.com/how-to/scriptableobjects-event-channels-game-code)).

**Fit:** **poor** as primary domain bus under settled constraints (SO identity + designer wiring). Remains useful **adjacent** pattern for rare designer-facing juice hooks — not for Aerisyn package Core.

---

### 3. Feel / More Mountains events (`MMEventManager`)

**What Feel is optimized for.** On-demand **game feel / juice** (feedbacks, springs, haptics, demos) — [Feel docs index](https://feel-docs.moremountains.com/index.html), [feel.moremountains.com](https://feel.moremountains.com/), [Asset Store Feel #183370](https://assetstore.unity.com/packages/tools/particles-effects/feel-183370).

**MMEventManager (ships in MMTools inside Feel / engines).** Static event manager; events are **structs**; trigger via `YOUR_EVENT.Trigger(...)` or `MMEventManager.TriggerEvent`; listeners implement `MMEventListener<T>`, start/stop in OnEnable/OnDisable. Documented in Feel API ([MMEventManager.cs file ref](https://feel-docs.moremountains.com/API/_m_m_event_manager_8cs.html)), Feel state-machine examples using `MMEventListener<MMStateChangeEvent<...>>` ([mmstatemachine](https://feel-docs.moremountains.com/mmstatemachine.html)), and identical patterns in TopDown Engine docs ([Events](https://topdown-engine-docs.moremountains.com/events.html)).

**Typed vs string.** Custom events: any **struct** type (typed). Bundled `MMGameEvent` is **string `EventName`** multi-purpose ([MMGameEvent API](https://feel-docs.moremountains.com/API/struct_more_mountains_1_1_tools_1_1_m_m_game_event.html); TopDown docs). `MMGameEventListener` matches by string name and invokes UnityEvent ([API](http://feel-docs.moremountains.com/API/class_more_mountains_1_1_tools_1_1_m_m_game_event_listener.html)).

**Editor story.** Feel’s product Inspector is for **MMFeedbacks**, not domain architecture. Event listening for named game events can use `MMGameEventListener` UnityEvents.

**Pure C#.** Static manager works without a scene component, but taking Feel/MMTools as a **domain messaging dependency** pulls a juice product + Asset Store EULA.

**Fit:** **veto** as domain messaging choice — wrong primary product; stringy `MMGameEvent` encourages the anti-pattern you want to avoid; if Feel is already a juice prereq later, keep MMEventManager **out** of Aerisyn Core.

---

### 4. Cysharp MessagePipe

**What it is.** “High-performance in-memory/distributed messaging pipeline for .NET and Unity” — DI-first pub/sub, filters, sync/async, keyed/keyless, buffered, singleton/scoped, broadcast + request/response, optional interprocess/distributed. Primary: [GitHub Cysharp/MessagePipe](https://github.com/Cysharp/MessagePipe), [README](https://github.com/Cysharp/MessagePipe/blob/master/README.md), [MIT LICENSE](https://raw.githubusercontent.com/Cysharp/MessagePipe/master/LICENSE). NuGet: [MessagePipe](https://www.nuget.org/packages/MessagePipe). VContainer pointer: [vcontainer.hadashikick.jp/integrations/messagepipe](https://vcontainer.hadashikick.jp/integrations/messagepipe).

**API shape (matches settled channel identity).**

```csharp
IPublisher<MyEvent> / ISubscriber<MyEvent>
publisher.Publish(new MyEvent());
subscriber.Subscribe(x => ...).AddTo(bag); // IDisposable
```

Also `IPublisher<TKey, TMessage>` keyed topics; `IAsyncPublisher` / `IAsyncSubscriber`; buffered variants; `IRequestHandler<TReq, TRes>` / `IAsyncRequestHandler` (**“Similar as MediatR”** per README); middleware **filters** (`MessageHandlerFilter<T>`, request filters, global/per-handler/per-subscribe).

**GlobalMessagePipe.** After `SetProvider(IServiceProvider)`, static `GetPublisher<T>` / `GetSubscriber<T>` / `CreateEvent<T>` etc.; required for Diagnostics window ([README Global provider / Unity sections](https://github.com/Cysharp/MessagePipe/blob/master/README.md)).

**Lifetime.** Brokers: Singleton or Scoped (`MessagePipeOptions.InstanceLifetime`); disposing a scope unsubscribes managed subscriptions. Subscribe returns `IDisposable`; `DisposableBag` for composites. Roslyn analyzer mentioned to prevent subscription leak. `EnableCaptureStackTrace` for diagnostics (DEBUG only recommended).

**Unity support.** UPM git paths:

- Core: `https://github.com/Cysharp/MessagePipe.git?path=src/MessagePipe.Unity/Assets/Plugins/MessagePipe`
- VContainer / Zenject companion packages
- Or `.unitypackage` from Releases

Requires **UniTask** (Unity replaces `ValueTask`). Unity lacks open-generics auto-registration → **manual** `RegisterMessageBroker<T>` (except: Unity 2022.1+ + VContainer 1.14+ can auto-resolve pub/sub open generics; request handlers still manual). **BuiltinContainerBuilder** = tiny DI if you do not want VContainer/Zenject; docs recommend using via `GlobalMessagePipe` in that mode.

**Editor.** MessagePipe **Diagnostics** window (subscription counts / stack traces when capture enabled) — debug tooling, not SO production wiring. Fits “debug/raise in Inspector only” if you add a thin Odin Raise helper that calls `Publish` (not provided by MessagePipe itself).

**Pure C# / EditMode / tests.** First-class on .NET Generic Host / MS.DI; in Unity, spin up BuiltinContainerBuilder or test DI container without a scene. Aligns with Aerisyn fixture-test culture.

**AsObservable.** Bridge to Rx (`AsObservable` on sync subscribers); README cites UniRx; R3 can consume similarly. neuecc: MessagePipe and R3 **remain separate** because DI lifetime + async reachability are not Rx signatures ([R3#194](https://github.com/Cysharp/R3/issues/194)).

**Install vs Odin model.** Open-source MIT UPM/git prereq (like UniTask), not Asset-Store-closed like Odin — **closer to UniTask than Odin**, but still “consumer installs; Aerisyn does not vendor.” Optional VContainer is a second prereq if chosen.

**Lock-in.** Cysharp ecosystem (strong Unity track record). MediatR-style needs covered in-box → no need for MediatR-on-Unity. Risk: DI discipline mandatory; forgetting Dispose leaks (mitigated by analyzer + diagnostics).

**Fit:** **good** — best match to typed domain signals + code wiring + EditMode + prereq packaging.

**Typical MessagePipe-in-Unity shape (for Idle comparison):**

1. Install MessagePipe (+ UniTask; + MessagePipe.VContainer if using VContainer).
2. In `LifetimeScope.Configure`: `RegisterMessagePipe`, `GlobalMessagePipe.SetProvider`, `RegisterMessageBroker<DomainMsg>` (as needed).
3. Inject `IPublisher<T>` / `ISubscriber<T>` into systems; `Subscribe(...).AddTo(DisposableBag)`; dispose on teardown / scope end.
4. Optional: filters for logging; Diagnostics in Editor; `AsObservable` into R3 for UI.

---

### 5. Idle-Axolotl / IdleLotl pub-sub

**Public search result:** No findable public repo, docs, or blog describing **Idle Axolotl / IdleLotl** Unity architecture or MessagePipe usage. A commercial mobile title “Idle Axolotl: Magic Tycoon” / MagiLotl (package id `com.unimob.idle.axolotl`, publisher UbiMob) appears in APK mirrors; that is **not** evidence of MessagePipe or a public source tree.

**Do not invent Idle source.** When the Idle repo is available, compare against the MessagePipe-in-Unity shape above (LifetimeScope registration, typed brokers, disposable subscriptions, GlobalMessagePipe diagnostics).

---

### 6. Strong alternatives

#### UniRx / R3 (Cysharp)

- **R3:** “new future of dotnet/reactive and UniRx”; Unity 2021.3+; UPM `R3.Unity` + NuGet core — [Cysharp/R3](https://github.com/Cysharp/R3).
- **UniRx:** Author banner: use R3 instead; repo **archived 2024-02-16** — [neuecc/UniRx](https://github.com/neuecc/UniRx).
- **Role vs MessagePipe:** R3 = operators / time / frame / UI streams; MessagePipe = DI pub/sub + async lifetime ([R3#194](https://github.com/Cysharp/R3/issues/194)). MessagePipe `AsObservable` can feed R3.
- **Fit as primary domain bus:** **poor**. **Fit as optional pipeline on top of a bus:** good. **UniRx for new Aerisyn work:** **veto** (archived).

#### MessagePipe vs MediatR-style

MessagePipe README explicitly implements mediator/`IRequestHandler` “Similar as MediatR,” plus filters and pub/sub in one stack. Standing up **MediatR itself** on Unity is a weaker path (NuGet/.NET-centric; no Unity-first Diagnostics/GlobalMessagePipe story). Prefer MessagePipe’s request APIs if CQRS-in-process is needed.

#### ExtEvents

[SolidAlloy/ExtEvents](https://github.com/SolidAlloy/ExtEvents) / OpenUPM `com.solidalloy.extevents`: **UnityEvent replacement** (richer Inspector persistent listeners, up to 4 args). Not a global typed domain bus. **Fit:** **veto** for this decision.

#### Messenger (Asset Store #292256)

Publisher listing: typed `Send(in T)` / `Signal<T>`, fluent `IDisposable` subscriptions, optional ScriptableObject Channels, DI examples (Zenject/VContainer/Reflex/pure), claims pure C# usable, zero external deps, 481 tests — [Asset Store](https://assetstore.unity.com/packages/tools/utilities/messenger-type-safe-event-system-for-unity-292256). Listing targets recent Unity 6.x. **Fit:** curious commercial peer to MessagePipe but **closed**, younger ecosystem vs Cysharp — **poor** unless MessagePipe is rejected for non-technical reasons; not shortlisted ahead of MessagePipe without a bake-off.

#### Global Message / “Event System” (Malte Husung / Curvature Games, Asset Store #242055)

Type-safe global messages; optional SO representation; code-only / hybrid / editor-only; editor history + trigger tools — [Asset Store](https://assetstore.unity.com/packages/tools/utilities/event-system-242055). Studio site confirms team ([curvaturegames.com](https://curvaturegames.com/team-jobs/)) but no separate public API docs found beyond the store page. Editor-heavy vs settled “debug only.” **Fit:** **poor**.

#### Just C# events + thin facade

Deliberate non-library option Unity’s own guide still acknowledges (static events / ordinary C# delegates) before recommending SO channels for designers ([event channels guide](https://unity.com/how-to/scriptableobjects-event-channels-game-code)). A tiny Aerisyn facade (`IDomainBus` / `Publish<T>` / `Subscribe<T>` returning `IDisposable`, optional Odin debug Raise) preserves constraints with zero third-party messaging lock-in. You re-implement filters/diagnostics/async yourself if needed. **Fit:** **good** as shortlist alternative to MessagePipe when dependency budget is zero.

#### Other Cysharp / well-maintained Unity messaging

Within Cysharp, **MessagePipe** is the messaging product; **R3** is Rx; **UniTask** is async runtime (often paired). No other Cysharp “domain bus” peer at MessagePipe’s level appeared in primary sources for this pass.

---

## Unknowns / need Idle repo

1. Whether Idle uses **MessagePipe + VContainer**, BuiltinContainerBuilder, or another bus.
2. Whether Idle registers brokers per message type or relies on VContainer 1.14+ open-generic resolve.
3. Whether Idle uses **GlobalMessagePipe** + Diagnostics, keyed brokers, async publishers, or request handlers.
4. How Idle handles subscription lifetime (scope dispose vs MonoBehaviour Destroy vs DisposableBag).
5. Whether Idle mixes SO event channels or Feel MMEventManager alongside MessagePipe (and for which concerns).
6. SOAP: public source availability / exact license terms beyond Asset Store EULA (not fetched from store TOS text).
7. Messenger Asset Store: independent verification of EditMode test story and Unity 2022.3 LTS support (listing emphasizes Unity 6).

---

## Recommended shortlist for grilling (not a final pick)

1. **MessagePipe (Cysharp)** — typed `Publish<T>`/`Subscribe<T>`, DI lifetimes, filters/async/mediator, Unity UPM, Diagnostics, MIT; closest to settled constraints.
2. **Thin C# events + Aerisyn facade** — same API shape, zero messaging vendor; grill on whether filters/diagnostics/async are worth a library.
3. **MessagePipe + optional R3 at edges** — only if grilling reveals heavy stream composition needs; keep R3 off the domain bus itself.

Explicitly **not** for grilling as primary: SOAP, Hipple SO Raise-as-architecture, Feel/MMEventManager, ExtEvents, UniRx.
