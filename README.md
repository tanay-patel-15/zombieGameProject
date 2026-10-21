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
| Demo City By Versatile Studio | Versatile Studio | `Assets/Versatile Studio Assets/` | Radio Relay Tower city streets |

The Radio Relay Tower also uses Kenney's [Retro Urban Kit](https://kenney.nl/assets/retro-urban-kit) (CC0). Download it and unzip it to `Assets/kenney_retro-urban-kit/`.

The player and infected animations and the two characters (the player and one infected look) are from [Mixamo](https://www.mixamo.com) and are included in `Assets/_Project/Animations/`. Their textures are extracted at 1024 px into `Assets/_Project/Materials/Textures/`.

## Credits

These third-party assets are included in the repo under their own licenses:

- "[Bolt Cutter Low-poly](https://skfb.ly/prrXU)" by MaX3Dd, licensed under [CC BY 4.0](http://creativecommons.org/licenses/by/4.0/). Files are in `Assets/bolt-cutter-low-poly/`. Changes: the metallic, ambient occlusion and roughness maps were packed into one URP texture (`Assets/_Project/Materials/Items/T_BoltCutters_Mask.png`) for the `Mat_BoltCutters` material.

## Project layout

All original code and content is in `Assets/_Project/` (Scripts, Prefabs, Scenes, Animations, Data, Materials, Audio).
