# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Seesaw is a cooperative two-player physics-based Unity game/research experiment. Two players move vertically on either side of a seesaw, working together to land in their respective targets while keeping a ball from rolling off. The project supports local play and networked play via LSL (Lab Streaming Layer) for research data streaming.

**Unity Version:** 2022.3.22f1 (LTS)
**Platform Target:** macOS Standalone
**Language:** C# 9.0 / .NET Standard 2.1

## Build & Run

Open in Unity Editor 2022.3.22f1. No custom build scripts or CI/CD — standard Unity build pipeline. A pre-built `Seesaw.app` macOS application is included.

Tests: EditMode tests live in `Assets/Tests/EditMode/` (covering `FixedSizedQueue`, `LSLStreamHelper`, `PlayerHelper`). Run via **Window → General → Test Runner → EditMode → Run All**.

## Architecture

### Game Modes & Scenes

Six scenes selectable from `MainMenuScene`:
- **BaseScene** — Local two-player mode (both players on same machine)
- **ServerScene / ClientScene** — Networked mode (one player per machine, communicating via LSL)
- **PassiveServerScene / PassiveClientScene** — Observer/passive modes

### Core Manager Hierarchy

`GameManager` (abstract base) owns the entire game loop — `Update()`, `PauseGame()`, `UnpauseGame()`, trial/block progression, scoring, and component caching all live in the base class. Concrete subclasses:
- `LocalManager` — empty subclass; uses base game loop for `BaseScene`
- `ServerManager` — empty subclass; uses base game loop for `ServerScene`. Referenced by `ManagerOutlet` to broadcast state via LSL
- `ClientManager` — overrides `Start()`/`Update()` to **skip** the base game loop; receives authoritative state via `ManagerInlet` and only renders targets locally

**Important:** any subclass that overrides `Start()` MUST call `CacheComponents()` first, otherwise the cached `Rigidbody`/`MeshRenderer`/etc. references will be null. `ClientManager` does this.

### ExperimentConfig (ScriptableObject)

All experiment tunables live in `ExperimentConfig.cs`, a `ScriptableObject` with `[CreateAssetMenu]`. `GameManager` has a required `public ExperimentConfig config;` field — if it's not assigned, `Start()` logs an error and disables the component.

Fields exposed by `ExperimentConfig`:
- **Trial Structure:** `numberOfTrials` (per block)
- **Target Placement:** `targetShiftMin`, `targetShiftMax` (random Y shift range, inclusive/exclusive), `minTargetPlayerDistance`, `placementMaxAttempts` (retry cap)
- **Player Perturbation:** `perturbationMin`, `perturbationMax`

To create a config: **Project window → Create → Seesaw → Experiment Config**. The asset is plain YAML, diffable in git, and editable in either the Inspector or a text editor. Each scene's manager component must have its `Config` slot wired to a config asset.

### Player Input (Strategy Pattern)

`PlayerController` is a `public interface` with `IsFreeze` and `Perturbation` properties. Implementations:
- `PlayerMovementController` — single configurable component for both local players. Inspector fields: `upKey`, `downKey`, `stick` (Left/Right enum).
- `ClientControllerInlet` — receives remote input via LSL and applies it to the transform
- `ClientControllerOutlet` — reads local input and broadcasts it via LSL. Same `upKey`/`downKey`/`stick` configuration as `PlayerMovementController`

### LSL Communication

Lab Streaming Layer streams for networked play and data recording:
- `Unity.ClientPlayerDistance` — per-frame player movement delta (1 channel)
- `Unity.GameManagerState` — tuple of `(IsPause, LeadingPlayerID, PlayScore)` (3 channels)
- `Unity.Position.[ObjectName]` — per-object position tracking (3 channels: PosX, PosY, PosZ)

Inlet classes (`ClientControllerInlet`, `ManagerInlet`, `PositionInlet`) receive data; Outlet classes (`ClientControllerOutlet`, `ManagerOutlet`, `PositionOutlet`) send data.

### Game Mechanics

- **Trials:** `config.numberOfTrials` rounds per block. Each trial assigns a random `Perturbation` multiplier in `[config.perturbationMin, config.perturbationMax)` to each player, scaling their movement.
- **Blocks:** Infinite progression. `LeadingPlayerID` flips sign each block (`+1` ↔ `-1`), determining which player's target is visible (per `CheckAndShowTarget()`).
- **Acceleration:** Player movement accelerates from `1.0` up to `PlayerHelper.MaxAcceleration` (2.5) on sustained input, resetting to `1.0` when input stops. Movement = `BasePlayerSpeed × direction × perturbation × acceleration`.
- **Win/Loss:** A trial is **won** when both **players** simultaneously occupy their target's trigger volume (target1 belongs to player1, target2 to player2). It is **lost** if the ball touches a stopper. The `trialResolved` flag prevents either outcome from firing twice in the same trial.
- **Target placement:** `MoveTargets()` picks a random integer Y shift in `[config.targetShiftMin, config.targetShiftMax)`, retrying up to `config.placementMaxAttempts` times until the candidate is non-zero, different from the previous shift, at least `config.minTargetPlayerDistance` away from both players' current Y, and on-screen for `gameCamera` (or `Camera.main` as fallback). If the retry cap is hit, a warning is logged and the last candidate is used.

### Key Physics Objects

- `BallBehavior` — sets `IsHit` on any trigger collision
- `TargetBehavior` — sets `IsHit` on trigger enter, clears on trigger exit
- `StopperBehavior` — visual barrier that follows a player and looks at a target
- `CylinderBehavior` — visual connector between the two players, scales to match distance

## Constants & Helpers (`SeesawHelper` namespace)

- `PlayerHelper.BasePlayerSpeed` (0.02), `MaxAcceleration` (2.5), `AccelerationRate` (0.03) — static properties (not methods). These are *movement-feel* constants and live in code; *experiment* tunables live in `ExperimentConfig`.
- `LSLStreamHelper.ResolveStreamSuffix(playerID)` — maps `+1` → `"Server"`, `-1` → `"Client"`
- `FixedSizedQueue<T>` — bounded thread-safe queue (currently used for `leadingPlayerHistory`)

## Assemblies

- **`Assets/Scripts/Seesaw.asmdef`** — production code. References `labstreaminglayer.LSL4Unity.Runtime`, `Unity.TextMeshPro`, `Unity.InputSystem`.
- **`Assets/Tests/EditMode/Seesaw.EditModeTests.asmdef`** — test assembly. References `Seesaw`, `nunit.framework.dll`, and the Unity test runner. Editor-only.

When adding a new package dependency, update `Seesaw.asmdef`'s `references` array — auto-referenced packages are not auto-referenced from named asmdefs, only from `Assembly-CSharp`.

## Key Dependency

- **LSL4Unity** (`com.labstreaminglayer.lsl4unity`) — Lab Streaming Layer integration, installed via Git from `labstreaminglayer/LSL4Unity`. All inlet/outlet classes derive from `AFloatInlet` / `AFloatOutlet`.

## Gotchas

- **Config required.** Every scene's manager must have an `ExperimentConfig` asset assigned to its `Config` field. Without it, `Start()` logs an error and disables the manager rather than NRE-ing.
- **Ball kinematic warning** — if the ball's `Rigidbody.isKinematic` is true at scene start, `InitializeStage()` skips velocity zeroing (Unity disallows setting velocity on kinematic bodies).
- **`ClientManager` deliberately does NOT run the base `Update()`** — it would double-process input and game state. If you add new shared logic to `GameManager.Update()`, decide whether `ClientManager` needs it too.
- **`PlayerController` must stay `public`** — `GameManager` exposes `protected` fields of that type, and C# accessibility rules forbid a less-accessible interface there.
- **`gameCamera` fallback is silent** — if both `gameCamera` and `Camera.main` are null, `WouldBeOffScreen` returns `false` and every candidate passes the off-screen check. Wire `gameCamera` explicitly or tag your camera as MainCamera.
- **`ScriptableObject` mutations persist in the Editor.** Writing to `config.numberOfTrials` at runtime modifies the underlying `.asset` file even after exiting Play mode. Treat configs as read-only at runtime.
