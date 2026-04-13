# Seesaw

A cooperative two-player physics-based Unity experiment. Two players move vertically on either side of a seesaw, working together to land in their respective targets while keeping a ball from rolling off. Designed for research use with Lab Streaming Layer (LSL) integration for synchronized data capture across machines.

## Requirements

- **Unity Editor** 2022.3.22f1 (LTS)
- **Platform:** macOS (build target). Editor runs on macOS / Windows / Linux.
- **LSL4Unity** — pulled automatically as a Git package from `labstreaminglayer/LSL4Unity` via `Packages/manifest.json`

## Getting Started

1. Clone or copy the repository.
2. Open the project folder in Unity Hub → Add → select the project root.
3. Open with Unity 2022.3.22f1. First import will fetch LSL4Unity from Git.
4. **Create an experiment config:** in the Project window, right-click → **Create → Seesaw → Experiment Config**. Name it (e.g. `BaselineExperiment.asset`). The defaults match the previously hardcoded values.
5. **Wire the config** into each scene's manager: select the GameObject with `LocalManager` / `ServerManager` / `ClientManager`, drag your config asset into the **Config** slot in the Inspector. Save the scene.
6. Open `Assets/Scenes/MainMenuScene.unity` and press Play.

A pre-built macOS standalone is also included as `Seesaw.app` at the project root.

## Game Modes

Selectable from the main menu:

| Scene | Description |
|---|---|
| `BaseScene` | Local two-player on one machine (W/S and O/L) |
| `ServerScene` | Networked — runs the authoritative game loop, broadcasts state via LSL |
| `ClientScene` | Networked — receives state from server, renders local view |
| `PassiveServerScene` | Server-side observer (no input) |
| `PassiveClientScene` | Client-side observer (no input) |

## Controls

| Action | Player 1 | Player 2 |
|---|---|---|
| Move up | `W` | `O` |
| Move down | `S` | `L` |
| Gamepad | Left stick Y | Right stick Y |
| Pause/resume | `Space` or gamepad ⓐ | `Space` or gamepad ⓐ |
| Return to menu | `Esc` | `Esc` |

## Project Layout

```
Assets/
├── Scripts/          C# game logic — Seesaw.asmdef
├── Tests/EditMode/   EditMode unit tests — Seesaw.EditModeTests.asmdef
├── Scenes/           6 .unity scene files
├── Materials/        Red/Green target materials
├── TextMesh Pro/     UI text assets
└── Samples/          LSL4Unity samples (reference only)
Packages/             Unity package manifest (LSL4Unity is a Git dep)
ProjectSettings/      Unity project configuration
```

## Architecture Overview

### Game Manager Hierarchy

```
GameManager (abstract)               ← owns Update(), Pause/Unpause, trial logic, component cache
├── LocalManager                     ← BaseScene; uses base loop unchanged
├── ServerManager                    ← ServerScene; uses base loop, paired with ManagerOutlet
└── ClientManager                    ← ClientScene; overrides Start/Update to skip base loop
                                       (state arrives via ManagerInlet)
```

`LocalManager` and `ServerManager` are intentionally empty — they exist as distinct types so each scene can wire up the right component and so `ManagerInlet`/`ManagerOutlet` can `RequireComponent` the correct manager.

**Subclassing rule:** any override of `Start()` MUST call `CacheComponents()` first. The base class caches `Rigidbody`, `MeshRenderer`, `BoxCollider`, and `PlayerController` references once at startup; the rest of the code assumes they are non-null.

### ExperimentConfig — ScriptableObject for tunables

All experiment parameters live in a `ScriptableObject` (`ExperimentConfig.cs`), not in code or scene fields. Each scene's manager has a `Config` slot that must be wired to a config `.asset`.

| Field | Default | Meaning |
|---|---|---|
| `numberOfTrials` | 5 | Trials per block before `LeadingPlayerID` flips |
| `targetShiftMin` / `targetShiftMax` | -3 / 4 | Inclusive/exclusive bounds for the random Y shift |
| `minTargetPlayerDistance` | 1.0 | Vertical buffer between target and player |
| `placementMaxAttempts` | 20 | Retry cap in `MoveTargets` before fallback |
| `perturbationMin` / `perturbationMax` | 0.5 / 2.0 | Random movement multiplier per player per trial |

**Why ScriptableObject:**
- One asset per experiment condition (e.g. `Easy.asset`, `Hard.asset`, `Pilot1.asset`). Swap them in the Inspector before each session — no code or scene changes.
- Plain YAML — diffable in `git`, editable in any text editor for bulk find/replace or scripted generation.
- Researchers can edit parameters without touching code.
- Runtime mutations of a config asset persist to disk in the Editor — treat configs as read-only at runtime to avoid accidentally committing parameter drift.

### Player Input

`PlayerController` is a tiny interface (`IsFreeze`, `Perturbation`) implemented by:

- **`PlayerMovementController`** — local movement. Inspector-configurable `upKey`, `downKey`, and `stick` enum (Left/Right) so a single class drives both players.
- **`ClientControllerOutlet`** — same input handling, but writes the per-frame distance into an LSL outlet instead of moving a transform.
- **`ClientControllerInlet`** — receives the LSL stream and applies the delta to its transform.

### Lab Streaming Layer (LSL)

Three streams are produced/consumed:

| Stream | Channels | Producer | Consumer |
|---|---|---|---|
| `Unity.ClientPlayerDistance` | `PlayerShiftDistance` | `ClientControllerOutlet` | `ClientControllerInlet` |
| `Unity.GameManagerState` | `IsPause`, `LeadingPlayerID`, `PlayScore` | `ManagerOutlet` | `ManagerInlet` |
| `Unity.Position.<ObjectName>` | `PosX`, `PosY`, `PosZ` | `PositionOutlet` | `PositionInlet` |

`PositionOutlet`/`PositionInlet` derive their stream name from the GameObject's name, so attaching them to multiple objects yields uniquely-named streams automatically.

### Game Loop (per frame, in `GameManager.Update()`)

1. **Unpause input** — `Space` or gamepad ⓐ unfreezes players and the ball
2. **Mission outcome** (gated by `trialResolved` flag — fires at most once per trial)
   - Both **players** in their respective targets → score++, advance trial
   - Ball touched a stopper → reset stage, pause
3. **Block advance** — when `trialCount >= config.numberOfTrials`, increment block, flip `LeadingPlayerID`
4. **Escape** → return to `MainMenuScene`

### Trial / Block Mechanics

- **Trial:** one attempt. Each new trial picks a random `Perturbation` per player from `[config.perturbationMin, config.perturbationMax)` that scales their movement output.
- **Block:** group of `config.numberOfTrials` trials. After each block, `LeadingPlayerID` flips sign (`+1` ↔ `-1`), which controls whose target is visible (`CheckAndShowTarget()`).
- **Acceleration:** holding an input ramps movement from `1.0` toward `MaxAcceleration` (2.5) at `AccelerationRate` (0.03) per FixedUpdate; releasing snaps back to `1.0`. These are *movement-feel* constants and live in `SeesawHelper.PlayerHelper`, not in the config.
- **Target placement:** `MoveTargets()` picks a random integer Y shift in `[config.targetShiftMin, config.targetShiftMax)`, retrying up to `config.placementMaxAttempts` times until the candidate is non-zero, different from the previous shift, at least `config.minTargetPlayerDistance` away from both players' current Y, and on-screen for `gameCamera`.

## Assemblies

| Assembly | Path | References |
|---|---|---|
| `Seesaw` | `Assets/Scripts/Seesaw.asmdef` | `labstreaminglayer.LSL4Unity.Runtime`, `Unity.TextMeshPro`, `Unity.InputSystem` |
| `Seesaw.EditModeTests` | `Assets/Tests/EditMode/Seesaw.EditModeTests.asmdef` | `Seesaw`, `nunit.framework.dll`, Unity test runner (Editor-only) |

When adding a new package dependency, update the references array in `Seesaw.asmdef` — auto-referenced packages are not picked up automatically by named assemblies.

## Tests

EditMode tests live in `Assets/Tests/EditMode/`:

- `FixedSizedQueueTests` — bounded queue semantics (7 tests)
- `LSLStreamHelperTests` — `ResolveStreamSuffix` mappings (8 tests, parameterized)
- `PlayerHelperTests` — sanity checks on tunable constants (7 tests)

Run via **Window → General → Test Runner → EditMode → Run All**.

Tests cover pure logic only — MonoBehaviours and the game loop are not tested.

## Common Tasks

### Create a new experiment condition

1. Right-click in the Project window → **Create → Seesaw → Experiment Config**
2. Name it descriptively (e.g. `HighPerturbation.asset`)
3. Adjust the fields in the Inspector
4. Assign it to the manager component's `Config` slot in your scene

### Add a new tracked object to LSL

Attach `PositionOutlet` to it on the server side and `PositionInlet` to a matching object on the client side. Both will derive `Unity.Position.<GameObjectName>` from the GameObject name — keep the names identical on both ends.

### Add a new key binding to local play

Edit the Inspector fields on the `PlayerMovementController` component (`upKey`, `downKey`, `stick`). No code changes needed.

### Add shared logic to the game loop

Put it in `GameManager.Update()` (or a method called from it). Then ask: should `ClientManager` run it too? `ClientManager` overrides `Update()` to skip the base loop, so you may need to mirror the call there.

### Debug a null reference in a manager

Two common causes:
1. **`Config` not assigned** — `Start()` logs an error and disables the manager. Check the Console.
2. **`Start()` override that didn't call `CacheComponents()`** — required because the base class caches all the component references.

## Known Quirks

- **Config is required.** Every scene's manager must have its `Config` slot wired. Without it, the manager logs an error in `Start()` and disables itself rather than NRE-ing.
- **Ball kinematic warning** — if the ball's `Rigidbody.isKinematic` is true at scene start, `InitializeStage()` skips velocity zeroing (Unity disallows setting velocity on kinematic bodies). Guarded in `GameManager.cs`.
- **`PlayerController` must stay `public`** — `GameManager` exposes `protected` fields of that interface type, and C# accessibility rules forbid a less-accessible interface there.
- **`gameCamera` fallback is silent** — if both `gameCamera` and `Camera.main` are null, the off-screen check passes everything. Wire `gameCamera` explicitly or tag your camera as MainCamera.
- **Editor-mode config mutations persist.** Writing to `config.numberOfTrials` at runtime modifies the `.asset` even after exiting Play mode — treat configs as read-only at runtime to avoid accidental git churn.

## Dependencies

- `com.labstreaminglayer.lsl4unity` (Git: `labstreaminglayer/LSL4Unity`) — LSL bindings
- `com.unity.inputsystem` 1.7.0 — gamepad support
- `com.unity.textmeshpro` 3.0.6 — UI text
- Standard Unity 2022.3 LTS modules

See `Packages/manifest.json` for the full list.
