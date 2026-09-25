# UniTask is a shared prerequisite with MessagePipe

Any DevHost or consuming Unity project that uses MessagePipe (ADR 0011) must install [Cysharp UniTask](https://github.com/Cysharp/UniTask) (`com.cysharp.unitask`). MessagePipe’s Unity build replaces `ValueTask` with UniTask; shipping one without the other is an incomplete stack.

**Considered options:** UniTask only inside games that already have it (e.g. Idle-Axolotl); document MessagePipe without a blanket UniTask rule.

**Why:** MessagePipe-on-Unity is not practical without UniTask. One shared rule avoids half-configured consumers and matches how Odin is treated as a monorepo prerequisite (ADR 0006), except UniTask *is* UPM-installable and still not vendored into this repository.

**Revisit when:** MessagePipe is dropped, or a future MessagePipe/Unity path removes the UniTask requirement.
