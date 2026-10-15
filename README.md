# Last Light Runner

Two-scene third-person zombie game for CSCI 43700. Retrieve a UV Power Core from an abandoned warehouse, then restore the citywide UV broadcast from a radio relay tower. Avoid the infected.

Built with Unity 6000.6.0f1 (URP, Input System, AI Navigation).

## Setup

This repo does **not** include third-party Asset Store packs, because their license doesn't allow redistribution. After cloning:

1. Open the project in Unity 6000.6.0f1.
2. Import the packs below from the Unity Asset Store (Window → Package Manager → My Assets). Keep each one at its default import location.
3. Open `Assets/_Project/Scenes/Warehouse.unity`.

Until the packs are imported, models and prefabs that use them show up as missing.

| Pack | Publisher | Import folder | Used for |
|---|---|---|---|
| Sci-Fi Warehouse Construction Kit | Sickhead Games | `Assets/SciFi Warehouse Kit/` | Warehouse level |
| Zombie Mutant | TODO | `Assets/Zombie_Mutant/` | Infected model |
| HiTech Energy Cell | TODO | `Assets/HiTech/` | UV Power Core |

Player animations are from [Mixamo](https://www.mixamo.com) and are included in `Assets/_Project/Animations/`.

## Project layout

All original code and content is in `Assets/_Project/` (Scripts, Prefabs, Scenes, Animations, Data, Materials, Audio).
