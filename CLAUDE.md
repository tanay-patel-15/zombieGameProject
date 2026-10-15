# Zombie Game (Unity)

## Project
- Unity 6000.6.0f1, URP, new Input System (`Assets/InputSystem_Actions.inputactions`), AI Navigation (NavMesh) installed.
- GitHub: tanay-patel-15/zombieGameProject. Git LFS is configured.
- My code and content go in `Assets/_Project/` (Scripts, Prefabs, Scenes, Animations, Data, Materials, Audio). Scripts are grouped by feature: Core, Player, Zombies, Combat, Interaction, Camera, UI.
- Never edit Asset Store or other third-party folders. If something needs changing, wrap or extend it from `_Project/`.
- I download assets and Mixamo animations myself. When a step needs one, tell me exactly what to get and where to put it; don't assume I have it.
- Build one prefab or system at a time. You can't click in the Unity Editor, so tell me exactly what to set up there (components, references, layers, tags).

## Memory (Obsidian vault)
This project's long-term memory is the Obsidian vault at `D:\OBDISIAN VAULTS\ZombieGame-Vault`.
- STATE.md, DESIGN.md and the newest session log are injected at session start by a hook. Treat them as the current truth.
- Before guessing why something was built a certain way, search the vault: `DECISIONS.md`, `systems/`, then older `sessions/`.
- DESIGN.md is mine. Don't edit it; propose changes instead.
- When I say we're done or wrapping up, or run /wrap, update the vault as described in `.claude/commands/wrap.md`. In long sessions, offer to /wrap before the context gets compacted.
- Vault notes use Obsidian wikilinks; link systems to each other, like [[ZombieAI]].
