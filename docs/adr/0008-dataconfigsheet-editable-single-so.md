# DataConfigSheet: editable single-file bake output

Bake writes into a game-owned `BakedSheetContainerAsset` (one `.asset` with serializable row lists). BakingSheet’s `ScriptableObjectSheetExporter` is not the default: it creates read-only JSON row sub-assets under each sheet SO, which blocks fast local SO tweaks and explodes into many child assets.

**Considered options:** keep BakingSheet SO exporter and fork its inspector to be writable; public CSV-only authoring; codegen typed SOs inside the package.

**Why:** Product needs Google as source of truth **and** Inspector-editable SOs for temp tests, with one parent file per bake target. Schema-specific lists stay in the game via `ApplyFromContainer`. Re-bake overwrites local edits; never push SO → Google.

**Revisit when:** a future BakingSheet release offers a flat editable SO exporter we can call without forking.
