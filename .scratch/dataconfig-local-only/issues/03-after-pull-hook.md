# 03: After Pull hook

**What to build:** Config Types can override a no-op `OnAfterPull` so that after every included tab parses successfully and before any Baked Asset is created or saved, game code can fill Local Only derivatives (e.g. from sheet-owned `turnSpeed`). A throw from After Pull fails the whole Pull and writes nothing. Inject Pull and live Pull share this timing.

**Blocked by:** 01 (Local Only parse contract).

**Status:** ready-for-agent

- [ ] `ConfigTypeAsset` exposes virtual `OnAfterPull()` with an empty default
- [ ] After all included parses succeed, Pull invokes `OnAfterPull` on each scratch before any create/save
- [ ] A successful override can populate Local Only fields that then land on the written Baked Asset path
- [ ] A throw from `OnAfterPull` fails the Pull as a whole with no Baked Asset writes
- [ ] Coverage via `PullFromGrids` (or a thin pre-write helper only if AssetDatabase blocks EditMode tests)
- [ ] No separate After Pull interface or UnityEvent wiring in this ticket
