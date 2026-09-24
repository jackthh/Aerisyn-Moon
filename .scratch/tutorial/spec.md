Status: needs-triage

# Spec: Tutorial package (`com.aerisyn.tutorial`)

Feature slug: `tutorial`  
Context: Tutorial (`com.aerisyn.tutorial`)  
Package: [`Packages/com.aerisyn.tutorial`](../../Packages/com.aerisyn.tutorial)

## Problem Statement

The monorepo needs a home for an upcoming tutorial / onboarding gameplay package. Before domain grilling and implementation, the UPM package folder, DevHost wiring, context map entry, and local issue-tracker directory must exist so design and tickets have a stable place to land.

## Solution

Scaffold `com.aerisyn.tutorial` at version `0.0.1` with Runtime asmdef + package marker only. Wire DevHost and root docs. Keep glossary terms as explicit placeholders until `/grill-with-docs` / `/domain-modeling` resolve them. Implementation issues land under `.scratch/tutorial/issues/`.

## Out of scope (this scaffold)

- Runtime tutorial API, authoring assets, samples, Editor tools
- Dependency on Quests or Data Config
- Release tags / public API promises

## Next steps

1. Grill domain language and replace stub terms in `Packages/com.aerisyn.tutorial/CONTEXT.md`
2. Capture ADRs under `docs/adr/` when decisions land
3. Split implementation into `.scratch/tutorial/issues/NN-*.md` once the spec is ready-for-agent

## Comments

- Workspace scaffold created so the upcoming package has a package id, DevHost entry, and scratch folder.
