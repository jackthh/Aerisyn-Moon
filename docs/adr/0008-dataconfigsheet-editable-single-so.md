# DataConfigSheet: editable single-file bake output

**Status:** paused for 0.3.0 (see [ADR 0009](0009-dataconfigsheet-luban-google-csv.md)). Runtime final is Luban `Tables` + JSON until editable SO is explicitly revived.

Bake previously wrote into a game-owned `BakedSheetContainerAsset` (one `.asset` with serializable row lists). BakingSheet’s `ScriptableObjectSheetExporter` was not used: it creates read-only JSON row sub-assets under each sheet SO.

**Considered options:** keep BakingSheet SO exporter and fork its inspector to be writable; public CSV-only authoring; codegen typed SOs inside the package.

**Why (historical):** Product wanted Google as source of truth **and** Inspector-editable SOs for temp tests, with one parent file per bake target.

**Revisit when:** local SO tweak workflow is required again on top of Luban outputs.

