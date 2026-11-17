# 01: Key lists and environment profile

**What to build:** One definition site for environment keys (seed Dev, Test, Production; games may add more) and one for feature keys (game-owned). Environment Profile ScriptableObject maps feature key → spreadsheet id per environment, holds active environment, and exposes resolve(spreadsheet id) for Pull. Inspectors only pick keys from those lists.

**Blocked by:** None.

**Status:** resolved

- [x] Environment key list asset/API exists; create path seeds Dev, Test, Production
- [x] Feature key list asset/API exists; empty by default; games add keys
- [x] Environment Profile maps each environment × feature → spreadsheet id
- [x] Active environment is stored once and readable by Pull
- [x] Resolve(feature key) returns spreadsheet id for active environment or fails clearly
- [x] Pure tests cover seed defaults, pick membership, resolve hit/miss

## Comments

- Integrated into `feature/dataconfig-pull-environments` via fast-forward to `3f6328fc482950ab5f7c55663b9ef14c54bb144a`.
