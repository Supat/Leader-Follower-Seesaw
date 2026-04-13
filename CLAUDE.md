# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Workspace Structure

This workspace contains two top-level directories:

- **`Seesaw/`** — Unity 2022.3.22f1 (LTS) project. A cooperative two-player physics-based seesaw game designed for research, with Lab Streaming Layer (LSL) integration for synchronized data capture. See `Seesaw/CLAUDE.md` for detailed architecture and development guidance.
- **`Analysis/`** — Python-based data analysis. Contains a Jupyter notebook (`Seesaw_XDF_sample.ipynb`) and sample XDF recording for post-hoc analysis of experiment data. Uses `pyxdf`, `numpy`, and `matplotlib` to load and visualize LSL streams recorded during gameplay.

## Quick Reference

### Unity Project (Seesaw/)

- **Open:** Unity Hub → Add → select `Seesaw/` → open with Unity 2022.3.22f1
- **Play:** Open `Assets/Scenes/MainMenuScene.unity` → press Play
- **Run tests:** Window → General → Test Runner → EditMode → Run All
- **Source code:** `Seesaw/Assets/Scripts/` (assembly: `Seesaw.asmdef`)
- **Tests:** `Seesaw/Assets/Tests/EditMode/` (assembly: `Seesaw.EditModeTests.asmdef`)

### Analysis (Analysis/)

- **Dependencies:** `pyxdf`, `numpy`, `matplotlib`
- **Data format:** XDF files containing LSL streams (`Unity.GameManagerState`, `Unity.Position.*`, `Unity.ClientPlayerDistance`)
- **Usage:** `pyxdf.load_xdf("filename.xdf")` returns `(data, header)` where `data` is a list of stream dicts with `time_series`, `time_stamps`, and `info` keys

## Cross-Cutting Concerns

The LSL stream names and channel layouts are shared between the Unity project and the analysis code. If you modify stream names or channels in `Seesaw/Assets/Scripts/` (outlet/inlet classes), the analysis notebook's stream name lookups will need updating to match.
