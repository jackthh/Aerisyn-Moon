# 03: Delete AerisynDataConfig scaffold + docs / gitignore

**What to build:** The DevHost AerisynDataConfig tree (placeholders, unused Demo scene, local README) is removed so Samples are the only demo setup. Package and sample documentation no longer steer people at that path; first-time setup docs show one concrete sample-local example for credentials and Baked output. Gitignore rules that existed only for that Credentials folder are cleaned or retargeted; global secret filename ignores may remain.

**Blocked by:** 01 (Include In Pull + empty Pull Config defaults).

**Status:** resolved

- [x] DevHost Assets/AerisynDataConfig tree is deleted (Credentials/Baked placeholders, Demo, local README)
- [x] Package README, sample README(s), and related DevHost docs no longer prescribe AerisynDataConfig paths
- [x] Docs include one concrete sample-local path example for secrets and output folder
- [x] .gitignore entries sole to that Credentials folder are removed or retargeted; global secret ignores kept if useful
- [x] Manual smoke: Project window no longer shows AerisynDataConfig under Assets; Samples still present

## Answer

Deleted `DevHost/Assets/AerisynDataConfig` (and its `.meta`). Retargeted package README, Weapons Pull sample READMEs (package + DevHost import), and root DevHost docs to sample-local paths under `Assets/Samples/Aerisyn Data Config Sheet/0.3.0/Weapons Pull/` for credentials and Baked output. Removed Credentials-folder-only `.gitignore` rules; kept global secret filename ignores. `UserSettings/AerisynDataConfig` OAuth token default left unchanged (per spec).
