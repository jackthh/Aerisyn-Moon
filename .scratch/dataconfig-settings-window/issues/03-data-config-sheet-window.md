# 03: Data Config Sheet window

**What to build:** An EditorWindow under Aerisyn → Data Config Sheet edits Data Config Settings, auto-creates the fixed-path asset on first open if missing, exposes a folder picker for the Pull All path and a `.asset` file picker for the Config Manifest path, and runs Sign In / Sign Out and Pull All from the same window. Existing menu aliases keep working or clearly call the same workflows.

**Blocked by:** 02

**Status:** resolved

- [x] Menu opens the Data Config Sheet window
- [x] First open creates Data Config Settings at the fixed project path when missing (seeded Environment Keys)
- [x] Window edits keys, active environment, bindings, title prefixes, manifest fields, auth paths, Pull All folder path
- [x] Pull All folder path uses a folder picker; Manifest Asset Path uses a `.asset` file picker
- [x] Sign In / Sign Out in the window use auth from Data Config Settings
- [x] Pull All in the window uses the same workflow as ticket 02
- [x] Menu aliases for Sign In / Pull All still work or redirect to the shared workflows

## Comments

## Answer

Added `Editor/DataConfigSheetWindow.cs` (menu `Aerisyn/Data Config Sheet/Open Window`). It calls new `DataConfigSettingsLoader.LoadOrCreate()` (creates the folder chain and the asset at `DefaultAssetPath`; the field initializers seed Dev/Test/Production and empty Feature Keys), draws the settings with `Editor.CreateEditor` (Odin draws it; the `[FolderPath]` / `[FilePath(Extensions="asset")]` attributes on the settings fields are the pickers, so no Odin Editor asmdef reference was added). Toolbar Sign In / Sign Out / Pull All save the asset and call `DataConfigPullWorkflow.SignInWithUiAsync` / `SignOutWithUi` / `PullAllWithUiAsync`; status uses `GoogleOAuthSession.IsSignedIn`. Existing Sign In / Sign Out / Pull All menus are unchanged and share those workflows.
- New pure `Runtime/Parsing/AssetFolderChain.cs` (missing-folder list for first create) with `Tests~/VerticalNest.Tests/AssetFolderChainTests.cs`. 151 tests pass.
- Not compiled in Unity (none here); hand-reviewed. `.meta` files added.
