# 01: Data Config Settings core

**What to build:** One Data Config Settings asset holds Environment Keys (seed Dev, Test, Production), empty Feature Keys, active Environment Key, title-prefix overrides, Feature×Env spreadsheet bindings, Config Manifest path and required Config Types, Google auth fields, and the Pull All folder path. Pure tests lock key membership, spreadsheet id resolve from settings-shaped data, folder-scope Pull Config path filtering, and blank-path failure. Old Environment Profile / key-list types may still exist unused.

**Blocked by:** None (can start immediately). Prerequisite: 0.6.0 pull-environments merged on the integration line you build from.

**Status:** ready-for-agent

- [ ] Data Config Settings exists as the single settings ScriptableObject with the fields above
- [ ] First-create seeding: Environment Keys Dev/Test/Production; Feature Keys empty
- [ ] Resolve(active env × Feature Key) returns spreadsheet id or fails clearly (pure tests)
- [ ] Folder-scope rule: given folder path + asset paths, returns in-scope paths in stable order (pure tests)
- [ ] Blank or whitespace Pull All folder path fails with a clear validation error (pure tests)
- [ ] Inspectors/pickers for keys bind to the settings lists (no free-typed env/feature strings on bindings)

## Comments
