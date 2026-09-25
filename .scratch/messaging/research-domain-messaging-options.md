# Domain messaging / observer / pub-sub options (Aerisyn Moon)

**Verdict (locked):** Primary domain bus is **Cysharp MessagePipe** ([ADR 0011](../../docs/adr/0011-messagepipe-domain-messaging.md)); **UniTask** is the paired prerequisite ([ADR 0012](../../docs/adr/0012-unitask-shared-prerequisite.md)). Staged: first cut = DevHost/game + Builtin (no VContainer required); packages *may* depend later; Quests C# events stay until a migration ticket. Idle’s **ZBase.Foundation.PubSub** and Tap/Pixel/Dice **SO Architecture** (+ optional Feel bridge) are documented alternatives, not Aerisyn primary.

Settled constraints (not reopened): domain signals primary (not Feel juice); packaging like Odin prerequisite; Editor = debug/raise only; channel identity = typed C# messages. Aerisyn already treats Odin this way ([ADR-0006](../../docs/adr/0006-odin-shared-prerequisite.md), [README](../../README.md)).

See full comparison in git history / local checkout; grill lock recorded in ADR 0011 and 0012.

## Decision (grill lock)

Chose **MessagePipe** over ZBase / thin facade / SO Raise. See ADR 0011 and 0012. Explicitly not primary: SOAP, SO Architecture Raise (Tap/Pixel/Dice), Feel/MMEventManager, ExtEvents, UniRx, Idle ZBase (game-local only).
