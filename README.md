# Tactical Intuition: Influence Maps in Unity

## Project Overview
This project is an educational module designed to teach **Spatial Reasoning** in Game AI. Unlike standard pathfinding (A*), which answers "How do I get there?", Influence Maps answer "**Where should I go?**" by creating a dynamic utility surface based on threats (Red) and objectives (Green).

**Topic:** Tactical AI & Spatial Analysis
**Engine:** Unity 6 (C#)
**Key Concepts:** Linear Falloff, Hill-Climbing, Hysteresis, Grid Optimization.

## Getting Started

### Prerequisites
* Unity 2022.3 LTS or Unity 6.
* Basic understanding of C# and the Unity Inspector.

### Installation
1. Clone this repository: `git clone https://github.com/[YOUR_USERNAME]/Tactical-Intuition-AI.git`
2. Open Unity Hub and click **Add Project from Disk**.
3. Select the cloned folder.
4. Open the scene `Assets/Scenes/FinalDemo.unity`.

## How to Play (The Demo)
1. **Press Play** in the Unity Editor.
2. Select the **Scene View** tab to see the debug visualization (Spikes).
   * **Red Spikes:** Danger Zones (Enemies).
   * **Green Valleys:** Safety/Goals (Gold).
3. **Interactive Test:** Select a Red Cube (Enemy) in the Hierarchy and drag it towards the Agent. Watch the agent flee before collision occurs.
4. **The Pincer Trap:** Duplicate an enemy to block the path. Observe how the agent finds the "local maxima" (safe path) between threats.

## Core Scripts
* `InfluenceGrid.cs`: Manages the 2D float array, calculates linear falloff, and handles the "Tall Spike" visualization.
* `InfluenceWalker.cs`: The agent logic. Uses a greedy hill-climbing algorithm with **Hysteresis** (threshold checking) to prevent oscillation.

## Assessment & Exercises
See `Pedagogical_Report.pdf` for the full teaching philosophy.
* **Challenge 1:** Modify the Falloff Curve to be Exponential (`strength / distance^2`).
* **Challenge 2:** Implement a "Coward" trait where Agents prioritize avoiding Red over finding Green.

## 📄 License
This project is licensed for educational use.
