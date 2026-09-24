# 02: Nested Vertical Nest → Baked Asset (fixture-backed)

**What to build:** A developer can hand-write an Odin Config Type with nested serializable types and, from an in-memory/fixture cell grid (weapons → upgrade levels → bonus stats), run Pull so the package parses Vertical Nest (Preamble, `!!!` Ignore Marker, Field Headers, blank-parent nests, struct arrays and primitive arrays) and creates or overwrites one Baked Asset in the Pull Config’s shared output folder with a stable GUID. Fixture tests lock the parse seam without live Google.

**Blocked by:** 01 — Prefactor — Strip Luban; expose Pull Config shell

**Status:** ready-for-agent

- [ ] Type-driven Vertical Nest parse covers Preamble skip, Header Row match by Field Headers, `!!!` ignore columns, and nested struct + primitive arrays
- [ ] Fixture tests include a weapons-style nested grid with Preamble and `!!!` and assert the object graph (or structured errors with coordinates)
- [ ] Pull creates a missing Baked Asset and re-Pull overwrites data in place (GUID stable) under the single output folder
- [ ] End-to-end is verifiable via fixture/inject path without requiring a live Google spreadsheet for the automated seam
