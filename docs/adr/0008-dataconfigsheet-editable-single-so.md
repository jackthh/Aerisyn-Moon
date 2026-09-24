# DataConfigSheet: editable single-file bake output

**Status:** superseded by [ADR 0010](0010-dataconfigsheet-pull-scriptableobject.md)

Bake previously wrote into a game-owned `BakedSheetContainerAsset` (one `.asset` with serializable row lists). BakingSheet’s `ScriptableObjectSheetExporter` was not used: it creates read-only JSON row sub-assets under each sheet SO.

**Considered options:** keep BakingSheet SO exporter and fork its inspector to be writable; public CSV-only authoring; codegen typed SOs inside the package.

**Why (historical):** Product wanted Google as source of truth **and** Inspector-editable SOs for temp tests, with one parent file per bake target.

**Revisit when:** n/a — SO runtime final is restored under ADR 0010 with a different bake/Pull stack.

