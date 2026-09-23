# Odin is a shared DevHost / consumer prerequisite

Every `com.aerisyn.*` package in this repo may assume [Odin Inspector](https://odininspector.com/) (Sirenix) is already installed in the consuming Unity project and in DevHost. Packages may reference `Sirenix.OdinInspector.Attributes` for authoring ScriptableObjects. Odin is not on UPM and is never vendored in this repository.

**Considered options:** Odin only for Quests; optional Odin via versionDefines; vendoring Sirenix into the monorepo.

**Why:** Authoring UX is shared across packages; per-package “maybe Odin” forks Inspector code and confuses DevHost setup. One install covers the whole monorepo.

**Revisit when:** a package must ship without any Inspector authoring, or Odin licensing blocks a consumer and a non-Odin authoring path is required.
