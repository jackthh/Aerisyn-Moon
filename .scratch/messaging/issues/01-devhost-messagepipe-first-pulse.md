# 01: DevHost MessagePipe first pulse

**What to build:** A working first pulse of MessagePipe domain messaging in DevHost: MessagePipe and UniTask installed as UPM prerequisites, Builtin container + `GlobalMessagePipe` bootstrap (no VContainer), one CLR-typed domain message published and received from code with a disposed subscription, plus a short consumer note covering install and bootstrap so a developer can copy the pattern. Demoable in the Editor without ScriptableObject channels, Feel, SO Architecture, or ZBase. Leave Quests and other package C# events unchanged.

**Blocked by:** None (can start immediately).

**Status:** ready-for-agent

- [ ] DevHost resolves MessagePipe and UniTask via UPM (not vendored into the monorepo `Packages/` tree)
- [ ] Builtin/`GlobalMessagePipe` bootstrap runs without VContainer
- [ ] One sample CLR message type can be published and received from code
- [ ] Sample subscription is disposed so further publishes do not fire that handler
- [ ] Short consumer-facing note documents install (MessagePipe + UniTask) and the Builtin bootstrap path
- [ ] No Quests (or other `com.aerisyn.*`) public event APIs are changed
- [ ] Feel, SO Architecture Raise assets, ZBase PubSub, and string topic buses are not introduced as Core domain messaging
