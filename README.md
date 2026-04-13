# Leader-Follower Seesaw

A cooperative two-player physics-based experiment built in Unity, designed for behavioral research. Two players move vertically on opposite sides of a seesaw, working together to land in target zones while keeping a ball balanced. The project includes Lab Streaming Layer (LSL) integration for synchronized multi-machine data capture and a Python analysis pipeline for post-hoc visualization of recorded sessions.

## Repository Structure

```
Leader-Follower-Seesaw/
├── Seesaw/       Unity 2022.3.22f1 project (game + LSL networking)
├── Analysis/     Python notebook + sample data for post-hoc analysis
└── README.md     This file
```

## Seesaw (Unity Project)

### Requirements

- **Unity** 2022.3.22f1 (LTS)
- **Platform:** macOS build target; Editor runs on macOS / Windows / Linux
- **LSL4Unity** — fetched automatically via `Packages/manifest.json`

### Quick Start

1. Clone this repository.
2. In Unity Hub, click **Add** and select the `Seesaw/` folder.
3. Open with Unity 2022.3.22f1 (first import fetches LSL4Unity from Git).
4. Create an experiment config: **Project window → Create → Seesaw → Experiment Config**.
5. Wire the config into each scene's manager component (`Config` slot in the Inspector).
6. Open `Assets/Scenes/MainMenuScene.unity` and press **Play**.

A pre-built macOS standalone (`Seesaw.app`) is also included.

### Game Modes

| Scene | Description |
|---|---|
| BaseScene | Local two-player on one machine |
| ServerScene | Networked — runs authoritative game loop, broadcasts via LSL |
| ClientScene | Networked — receives state from server, renders locally |
| PassiveServerScene | Server-side observer (no input) |
| PassiveClientScene | Client-side observer (no input) |

### Controls

| Action | Player 1 | Player 2 |
|---|---|---|
| Move up | `W` | `O` |
| Move down | `S` | `L` |
| Gamepad | Left stick Y | Right stick Y |
| Pause / Resume | `Space` or gamepad A | `Space` or gamepad A |
| Return to menu | `Esc` | `Esc` |

### How It Works

- **Trials and blocks:** Each block consists of a configurable number of trials. A trial is won when both players simultaneously occupy their target zones; it is lost if the ball touches a stopper. After each block, the leading player flips, changing which player's target is visible.
- **Perturbation:** Each trial assigns a random movement multiplier to each player, varying the difficulty.
- **Acceleration:** Holding an input key ramps movement speed up to a maximum; releasing resets it.
- **Experiment configuration:** All tunables (trial count, target placement bounds, perturbation range, etc.) are stored in ScriptableObject assets — swap configs in the Inspector to change conditions without editing code.

### LSL Data Streams

Three stream types enable networked play and research recording:

| Stream | Channels | Purpose |
|---|---|---|
| `Unity.ClientPlayerDistance` | 1 | Per-frame player movement delta |
| `Unity.GameManagerState` | 3 | Game pause state, leading player, score |
| `Unity.Position.<ObjectName>` | 3 | Object position (X, Y, Z) |

These streams can be recorded with [LabRecorder](https://github.com/labstreaminglayer/App-LabRecorder) into XDF files for offline analysis.

### Running Tests

In the Unity Editor: **Window → General → Test Runner → EditMode → Run All**.

Tests cover pure helper logic (`FixedSizedQueue`, `LSLStreamHelper`, `PlayerHelper`). See `Seesaw/Assets/Tests/EditMode/` for details.

For full project documentation — architecture, manager hierarchy, assemblies, common tasks, and known quirks — see [`Seesaw/README.md`](Seesaw/README.md).

## Analysis (Python)

### Contents

| File | Description |
|---|---|
| `Seesaw_XDF_sample.ipynb` | Jupyter notebook that loads and visualizes recorded LSL streams |
| `sub-P001_ses-S001_task-Default_run-001_eeg.xdf` | Sample XDF recording from a gameplay session |

### Dependencies

```
pip install pyxdf numpy matplotlib
```

### Usage

```python
import pyxdf

data, header = pyxdf.load_xdf("sub-P001_ses-S001_task-Default_run-001_eeg.xdf")

for stream in data:
    name = stream["info"]["name"][0]
    print(f"{name}: {stream['time_series'].shape}")
```

The notebook demonstrates how to extract individual streams by name, plot position traces for players/targets/ball, and inspect game state transitions over time.

### Available Streams in the Sample

- `Unity.GameManagerState` — pause flag, leading player ID, score
- `Unity.Position.Player1`, `Unity.Position.Player2` — player positions
- `Unity.Position.Target1`, `Unity.Position.Target2` — target positions
- `Unity.Position.Ball` — ball position
- `Unity.ClientPlayerDistance` — per-frame movement input

## Cross-Cutting Concerns

LSL stream names and channel layouts are shared between the Unity project and the analysis code. If you modify stream names or channels in `Seesaw/Assets/Scripts/` (outlet/inlet classes), update the analysis notebook's stream name lookups to match.
