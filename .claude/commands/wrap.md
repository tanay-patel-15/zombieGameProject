---
description: Save this session into the Obsidian vault (session log, STATE, decisions, systems)
---
Wrap up this session into the vault at `D:\OBDISIAN VAULTS\ZombieGame-Vault`. Follow the formats in its `_templates/` folder.

0. **Auto logs first:** if any session logs in `sessions/` have `auto: true` in their frontmatter, they were written automatically after a session ended without /wrap. Read them, check them against what you know, add any real decisions to DECISIONS.md and any systems to systems/, then change `auto: true` to `auto: reviewed`.
1. **Session log:** create `sessions/YYYY-MM-DD-HHmm.md` using the current local date and time. Cover the goal, what we did, what broke and how it was fixed, and what's left. Link every system touched with [[wikilinks]]. Keep it under about 40 lines.
2. **STATE.md:** rewrite it to reflect where things stand *now*, update the `updated:` date, and remove any `auto_updated_from:` line. Keep it under 40 lines; history belongs in the session log, not in STATE.
2b. **PLAN.md:** tick (`- [x]`) the Schedule and Test checklist items that were actually finished and verified this session. Don't rewrite anything else in PLAN; if the plan or schedule needs to change, propose it at the end of your reply. Never edit BRIEF.md.
3. **DECISIONS.md:** append an entry for each real design or architecture choice made this session (decision, why, what was rejected). Skip this if there were none. Never edit past entries; if a decision is reversed, add a new entry saying so.
4. **systems/:** for every script or system we created or meaningfully changed, create or update `systems/<Name>.md`: what it does, file paths, Inspector fields and public API, which systems it talks to, known issues.
5. **HOME.md:** add links for any new system notes and for the new session log.
6. **DESIGN.md:** don't edit it. If something we did changes the design, list the proposed change at the end of your reply.

Finish with a 3-5 line summary of what you wrote.
