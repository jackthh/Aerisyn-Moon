# 03: Data Config Sheet window

**What to build:** An EditorWindow under Aerisyn → Data Config Sheet edits Data Config Settings, auto-creates the fixed-path asset on first open if missing, exposes a folder picker for the Pull All path and a `.asset` file picker for the Config Manifest path, and runs Sign In / Sign Out and Pull All from the same window. Existing menu aliases keep working or clearly call the same workflows.

**Blocked by:** 02

**Status:** ready-for-agent

- [ ] Menu opens the Data Config Sheet window
- [ ] First open creates Data Config Settings at the fixed project path when missing (seeded Environment Keys)
- [ ] Window edits keys, active environment, bindings, title prefixes, manifest fields, auth paths, Pull All folder path
- [ ] Pull All folder path uses a folder picker; Manifest Asset Path uses a `.asset` file picker
- [ ] Sign In / Sign Out in the window use auth from Data Config Settings
- [ ] Pull All in the window uses the same workflow as ticket 02
- [ ] Menu aliases for Sign In / Pull All still work or redirect to the shared workflows

## Comments
