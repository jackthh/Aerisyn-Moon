# Quests does not own official save I/O

Quests exposes a serializable Progress Snapshot and keeps live Progress in memory. Official save/load belongs to the game or a later Aerisyn save package. Temporary ScriptableObject / JSON / text helpers may exist only as samples or unofficial tooling, never as the public persistence contract.

**Considered options:** package-owned PlayerPrefs/files; game-only ad hoc saves with no shared snapshot shape.

**Why:** Idle-style games already have per-board save blobs; a package-owned save file would fight that and lock consumers into one I/O path before a shared save package exists.
