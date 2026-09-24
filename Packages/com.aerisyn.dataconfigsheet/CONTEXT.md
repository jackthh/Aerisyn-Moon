# Data Config

Domain language for `com.aerisyn.dataconfigsheet`: pulling designer tables from Google Sheets and baking them into runtime-readable Unity config.

## Language

**Bake**:
One editor pass that downloads Google tabs to CSV, runs Luban, and refreshes generated code/data under the project.
_Avoid_: Sync (implies two-way); export as the everyday name for the whole pipeline

**Bake Config**:
Editor settings for one bake job: spreadsheet id(s), auth paths, Luban project paths, and output folders.
_Avoid_: Factory (BakingSheet-era); treating Bake Config as the schema itself

**Luban Project**:
The Luban `luban.conf`, schema registration (XML/`__tables__`), and data directory that CSV files land in before generation.
_Avoid_: SheetContainer; BakingSheet workbook

**Schema Header**:
`##var` / `##type` (and nested / sep list fields) in Google sheets or seed CSV. Nested multi-row CSV may need compact `list#sep` forms; Excel/xlsx multi-row is Luban's happier path for deep nests.
_Avoid_: Hand-written BakingSheet `Sheet` / `SheetRow` as the schema source

**Tables**:
Luban-generated entry type that loads baked JSON (or bin) at runtime for gameplay lookups.
_Avoid_: BakedSheetContainerAsset / editable SO as the 0.3.0 default runtime final
