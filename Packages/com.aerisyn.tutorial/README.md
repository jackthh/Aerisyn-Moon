# Aerisyn Tutorial (`com.aerisyn.tutorial`)

Scaffold for an upcoming game-agnostic tutorial / onboarding package. Domain language and
runtime API are not defined yet; see [`.scratch/tutorial/spec.md`](../../.scratch/tutorial/spec.md)
and [`CONTEXT.md`](CONTEXT.md).

**Unity:** 2022.3+ · **Requires:** [Odin Inspector](https://odininspector.com/) (Sirenix) · **Runtime assembly:** `Aerisyn.Tutorial` · **Version:** `0.0.1` (unreleased scaffold)

## Requirements

| Dependency | Why |
|---|---|
| **Unity 2022.3+** | Minimum editor / player target |
| **Odin Inspector (Sirenix)** | Shared Aerisyn prerequisite so packages compile against `Sirenix.OdinInspector.Attributes`. |

Install Odin from the Unity Asset Store (or your usual Sirenix workflow) into the **consuming project**. Odin is not on UPM and is **not** shipped in this repository.

## Install

1. Install **Odin Inspector** into your Unity project.
2. Package Manager → **+ → Add package from git URL…**

```text
https://github.com/jackthh/Aerisyn-Moon.git?path=/Packages/com.aerisyn.tutorial
```

## Status

This package is a **workspace scaffold** only: `package.json`, runtime asmdef, and a package marker type.
No gameplay API yet. Design work lives under `.scratch/tutorial/`.

## Layout

```text
Packages/com.aerisyn.tutorial/
  package.json
  README.md
  CHANGELOG.md
  CONTEXT.md          # glossary stub (filled when domain is grilled)
  Runtime/
    Aerisyn.Tutorial.asmdef
    TutorialPackage.cs
```
