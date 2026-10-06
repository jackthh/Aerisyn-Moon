# 04: Ship 0.5.0 docs + sample pattern

**What to build:** Consumers can adopt Local Only + After Pull from package docs at version 0.5.0: CHANGELOG/README describe the attribute, warn-and-ignore, Header Emitter omit note, and `OnAfterPull`; a sample or doc example shows the turnSpeed-style derivative pattern. Domain language stays aligned with CONTEXT and ADR 0013.

**Blocked by:** 01 (Local Only parse contract), 02 (Header Emitter omits Local Only), 03 (After Pull hook).

**Status:** ready-for-agent

- [ ] Package version is 0.5.0
- [ ] CHANGELOG documents Local Only, warnings, Header Emitter omit guidance, and After Pull for 0.5.0
- [ ] README teaches how to mark Local Only fields and override `OnAfterPull`
- [ ] Sample or documented example shows sheet-owned input + Local Only derivatives filled in After Pull
- [ ] Wording matches Data Config CONTEXT (Local Only vs Ignore Marker; clean Pull; After Pull)
