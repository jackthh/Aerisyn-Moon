# 03: Delete AerisynDataConfig scaffold + docs / gitignore

**What to build:** The DevHost AerisynDataConfig tree (placeholders, unused Demo scene, local README) is removed so Samples are the only demo setup. Package and sample documentation no longer steer people at that path; first-time setup docs show one concrete sample-local example for credentials and Baked output. Gitignore rules that existed only for that Credentials folder are cleaned or retargeted; global secret filename ignores may remain.

**Blocked by:** 01 (Include In Pull + empty Pull Config defaults).

**Status:** ready-for-agent

- [ ] DevHost Assets/AerisynDataConfig tree is deleted (Credentials/Baked placeholders, Demo, local README)
- [ ] Package README, sample README(s), and related DevHost docs no longer prescribe AerisynDataConfig paths
- [ ] Docs include one concrete sample-local path example for secrets and output folder
- [ ] .gitignore entries sole to that Credentials folder are removed or retargeted; global secret ignores kept if useful
- [ ] Manual smoke: Project window no longer shows AerisynDataConfig under Assets; Samples still present
