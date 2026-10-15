#!/usr/bin/env bash
# Prints the always-loaded vault notes; Claude Code adds this output to context.
VAULT="/d/OBDISIAN VAULTS/ZombieGame-Vault"

show() {
  if [ -f "$2" ]; then
    echo "===== $1 ====="
    cat "$2"
    echo
  fi
}

echo "Project memory from the Obsidian vault at D:\\OBDISIAN VAULTS\\ZombieGame-Vault"
echo
show "STATE.md" "$VAULT/STATE.md"
show "DESIGN.md" "$VAULT/DESIGN.md"

# Session logs are named YYYY-MM-DD-HHmm.md, so the last one alphabetically is the newest.
last=$(ls "$VAULT/sessions/"*.md 2>/dev/null | sort | tail -n 1)
if [ -n "$last" ]; then
  show "LAST SESSION: $(basename "$last")" "$last"
fi
exit 0
