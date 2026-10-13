# DataConfigSheet: Local Only fields + After Pull (0.5.0)

Config Types may declare **Local Only** fields (`[LocalOnly]`): they are not part of the Source Sheet Header Row contract and are never bound from cells. Pull stays **clean** (rebuild rows; no merge of prior Inspector Local Only values). Games refill derivatives by overriding **`OnAfterPull`** on `ConfigTypeAsset`, invoked after all included tabs parse and before any Baked Asset create/save; a throw fails the whole Pull with no writes.

**Status:** accepted

**Considered options:** (1) sheet-optional bind-if-present + preserve Inspector values by row `id` (full or root-only merge); (2) Local Only header skip only, no After Pull seam (rely on OnValidate / manual menus); (3) interface/`IAfterPull` instead of a virtual on `ConfigTypeAsset`.

**Why:** Designers often ship fewer columns than the C# nest shape; forcing every scalar onto the Header Row blocks Pull. Preserve/merge is higher bug surface (identity + nest matching) than the consumer needed. Inspector/`OnValidate` does not reliably run after Pull overwrite, so a small virtual After Pull hook keeps “recompute every Pull” automatic while leaving all formulas in game code. Virtual on `ConfigTypeAsset` matches existing subclassing and OnValidate-like overrides better than a second interface type.

**Revisit when:** a game needs Local Only Inspector values to survive re-Pull without After Pull; or nest-level preserve by child identity is required.
