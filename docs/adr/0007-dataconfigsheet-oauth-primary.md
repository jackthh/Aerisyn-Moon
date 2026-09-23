# DataConfigSheet: OAuth user sign-in is the primary Google auth

Editor bake authenticates with **OAuth Desktop** (browser sign-in) by default so private sheets shared by email work like Idle Axolotl. After sign-in, the package writes an `authorized_user` JSON token that BakingSheet’s `GoogleCredential.FromJson` accepts. **Service account** remains an optional Auth Mode for CI / headless.

**Considered options:** service-account only; public CSV export URL (Anyone with the link); port IdleLotl downloader instead of BakingSheet Google converter.

**Why:** Matches the remembered Idle UX (browser login, email sharing) without abandoning BakingSheet. One org OAuth client setup; per-dev Sign In. Tokens live under gitignored `UserSettings/`. Public link sharing is weaker for private design data.

**Revisit when:** Google tightens installed-app OAuth in a way that breaks Unity Editor; or CI must be the only supported path.
