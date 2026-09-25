# 02: Builtin publish/subscribe/dispose seam tests

**What to build:** Automated EditMode or pure C# fixture tests that lock the MessagePipe first-cut contract: boot Builtin/`GlobalMessagePipe` (no scene, no ScriptableObject message assets, no VContainer), assert a subscriber receives a published typed message, then assert disposing the subscription stops further receives. Same testing culture as Data Config’s Vertical Nest fixtures: one high seam, external behavior only.

**Blocked by:** 01 (DevHost MessagePipe first pulse).

**Status:** ready-for-agent

- [ ] Tests boot MessagePipe via Builtin/`GlobalMessagePipe` without a scene or SO message assets
- [ ] Tests assert a subscriber receives a published typed CLR message
- [ ] Tests assert dispose stops further receives for that subscription
- [ ] Tests do not assert MessagePipe internals, DI registration call graphs, or Inspector helpers
- [ ] Suite stays limited to this one seam (no Quests migration coverage, no VContainer, no Feel/SO/ZBase adapters)
