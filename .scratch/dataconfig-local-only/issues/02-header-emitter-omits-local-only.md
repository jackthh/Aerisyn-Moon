# 02: Header Emitter omits Local Only

**What to build:** Copy Header Row / Header Emitter pastes only the sheet contract: Local Only fields are omitted from the Header Row text, and the emit guidance states how many Local Only fields were left out so developers know the clipboard is intentionally incomplete vs the full C# shape.

**Blocked by:** 01 (Local Only parse contract).

**Status:** ready-for-agent

- [ ] Emitted Header Row does not include Local Only field names or their Column Aliases
- [ ] Emit guidance mentions the count of omitted Local Only fields
- [ ] Non-Local Only Field Headers and Column Aliases still emit as today
- [ ] Tests at the Header Emitter seam lock omit + guidance behavior
