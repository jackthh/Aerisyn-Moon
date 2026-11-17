# 02: Pull / Pull All read settings

**What to build:** Pull Config keeps Feature Key, output folder, Config Types, Include In Pull, and Skip Title Check only. It no longer stores an Environment Profile reference or auth fields. Single Pull and Pull All load Data Config Settings for spreadsheet id and Google auth. Pull All discovers Pull Configs only under the settings folder path (recursive); a blank path fails with no whole-project fallback. Title Check, Pull All summary (continue on failure), and Config Manifest completeness keep 0.6.0 behavior against settings data.

**Blocked by:** 01

**Status:** ready-for-agent

- [ ] Pull Config has no Environment Profile reference and no auth fields
- [ ] Feature Key picker reads Feature Keys from Data Config Settings
- [ ] Single Pull resolves spreadsheet id and credentials from Data Config Settings; missing settings fails clearly
- [ ] Pull All uses folder-scoped discovery only; blank path fails; empty folder is not success
- [ ] Title Check / Skip Title Check still work with prefixes from Data Config Settings
- [ ] Pull All summary and manifest completeness (including mixed-run missing types) still pass pure/regression tests
- [ ] Build gate helpers accept Data Config Settings (Production vs completeness stay separate)

## Comments
