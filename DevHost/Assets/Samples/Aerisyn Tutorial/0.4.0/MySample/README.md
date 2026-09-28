# MySample — interactive UI Tutorial

Local DevHost example (not the package Runner Smoke sample). Screen-space UGUI with a
4-step Soft/Hard recruit flow: highlights scale/recolor controls, coach copy explains each beat,
and button clicks drive Cue Done + Reports.

## Open

1. Open `Sample Runner.unity`.
2. Enter Play mode (Game view).
3. Follow the HUD status line:

| Step | Enforcement | What to click |
|---|---|---|
| 1 `open_panel` | Soft | **Recruit** → coach **OK** |
| 2 `choose_hero` | Soft | **Hero A** or **Hero B** → coach **OK** |
| 3 `assign_slot` | Hard Gate | **Hero Slot** only (other clicks blocked) |
| 4 `confirm_recruit` | Hard Gate | **Confirm** → coach **OK** |

## Assets

- `Tutorial Sample.asset` — SO Tutorial `recruit.hero` (4 Steps)
- `MySampleTutorialDriver` — UGUI presentation + Runner seam
- `MySampleClickTarget` — Button onClick → driver

World-space cubes under `Tutorial Targets` are disabled; the Canvas is the play surface.
