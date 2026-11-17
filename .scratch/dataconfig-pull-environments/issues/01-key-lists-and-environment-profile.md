# 01: Key lists and environment profile

**What to build:** One definition site for environment keys (seed Dev, Test, Production; games may add more) and one for feature keys (game-owned). Environment Profile ScriptableObject maps feature key → spreadsheet id per environment, holds active environment, and exposes resolve(spreadsheet id) for Pull. Inspectors only pick keys from those lists.

**Blocked by:** None.

**Status:** ready-for-agent

- [ ] Environment key list asset/API exists; create path seeds Dev, Test, Production
- [ ] Feature key list asset/API exists; empty by default; games add keys
- [ ] Environment Profile maps each environment × feature → spreadsheet id
- [ ] Active environment is stored once and readable by Pull
- [ ] Resolve(feature key) returns spreadsheet id for active environment or fails clearly
- [ ] Pure tests cover seed defaults, pick membership, resolve hit/miss

## Comments
