# Background worker for the SessionEnd safety net.
# If a session ended without /wrap and had enough real messages since the last
# save, a headless Claude writes an auto session log and updates STATE.md.
#
# Manual test without calling Claude:
#   powershell -ExecutionPolicy Bypass -File .claude/hooks/auto-wrap-worker.ps1 -TranscriptPath <file.jsonl> -DryRun
param(
  [Parameter(Mandatory = $true)][string]$TranscriptPath,
  [string]$SessionId = 'unknown',
  [string]$Vault = 'D:\OBDISIAN VAULTS\ZombieGame-Vault',
  [int]$MinMessages = 5,
  [string]$Model = 'sonnet',
  [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$WorkDir = Join-Path $Vault '.autowrap'
$LogFile = Join-Path $WorkDir 'autowrap.log'
$Utf8 = New-Object System.Text.UTF8Encoding($false)

function Write-Log([string]$msg) {
  $line = '{0}  {1}' -f (Get-Date -Format 'yyyy-MM-dd HH:mm:ss'), $msg
  [System.IO.File]::AppendAllText($LogFile, $line + [Environment]::NewLine, $Utf8)
  if ($DryRun) { Write-Output $line }
}
function Cut([string]$s, [int]$n) {
  if ($null -eq $s) { return '' }
  if ($s.Length -gt $n) { return $s.Substring(0, $n) + ' ...[cut]' }
  return $s
}

New-Item -ItemType Directory -Force -Path $WorkDir | Out-Null

try {
  if (-not $DryRun) { Start-Sleep -Seconds 5 }  # the transcript file is written asynchronously
  if (-not (Test-Path -LiteralPath $TranscriptPath)) { Write-Log "skip: no transcript at $TranscriptPath"; exit 0 }

  $entries = New-Object System.Collections.Generic.List[string]
  $userCount = 0

  foreach ($line in [System.IO.File]::ReadLines($TranscriptPath, $Utf8)) {
    if (-not $line.Trim()) { continue }
    try { $e = $line | ConvertFrom-Json } catch { continue }
    if ($e.isMeta -or $e.isSidechain -or -not $e.message) { continue }
    $content = $e.message.content

    if ($e.type -eq 'user') {
      if ($content -is [string]) { $texts = @($content) }
      else { $texts = @($content | Where-Object { $_.type -eq 'text' } | ForEach-Object { $_.text }) }
      # One user entry can hold several text blocks (e.g. an IDE notice plus the
      # real prompt), so count the entry once, not once per block.
      $kept = @()
      foreach ($t in $texts) {
        if ($t -match '<command-name>/wrap</command-name>') {
          # Everything before a /wrap is already saved; start over from here.
          $entries.Clear(); $userCount = 0; $kept = @()
          $entries.Add('[/wrap was run here; everything earlier is already saved]')
          continue
        }
        if ($t -match '^\s*<(command-|local-command|system-reminder|ide_)' -or $t -match '^\s*Caveat:') { continue }
        if ($t -match '^\s*\[Request interrupted by user') { continue }
        $kept += $t
      }
      if ($kept.Count -gt 0) {
        $userCount++
        $entries.Add("### Tanay`n" + (Cut ($kept -join "`n`n") 2000))
      }
    }
    elseif ($e.type -eq 'assistant') {
      foreach ($b in @($content)) {
        if ($b.type -eq 'text' -and $b.text -and $b.text.Trim()) {
          $entries.Add("### Claude`n" + (Cut $b.text 2500))
        }
        elseif ($b.type -eq 'tool_use') {
          $path = $b.input.file_path
          if (@('Write', 'Edit', 'MultiEdit') -contains $b.name -and $path) {
            if ($path -match 'ZombieGame-Vault[\\/]+sessions[\\/]') {
              # Claude wrote a session log itself (e.g. "let's wrap up"), so treat it like /wrap.
              $entries.Clear(); $userCount = 0
              $entries.Add('[a session log was saved here; everything earlier is already saved]')
              continue
            }
            $entries.Add("[$($b.name): $path]")
          }
          elseif (@('Bash', 'PowerShell') -contains $b.name -and $b.input.command) {
            $entries.Add("[$($b.name): " + (Cut $b.input.command 200) + ']')
          }
        }
      }
    }
  }

  if ($userCount -lt $MinMessages) {
    Write-Log "skip: session $SessionId had $userCount unsaved messages (minimum $MinMessages)"
    exit 0
  }

  $text = $entries -join "`n`n"
  $maxChars = 200000
  if ($text.Length -gt $maxChars) {
    $text = "[...earlier part cut for length...]`n`n" + $text.Substring($text.Length - $maxChars)
  }

  $date = Get-Date -Format 'yyyy-MM-dd'
  $file = Get-Date -Format 'yyyy-MM-dd-HHmm'
  if (Test-Path -LiteralPath (Join-Path $Vault "sessions\$file.md")) { $file = Get-Date -Format 'yyyy-MM-dd-HHmmss' }

  [System.IO.File]::WriteAllText((Join-Path $WorkDir 'transcript.md'), $text, $Utf8)

  $instructions = @'
# Auto-wrap instructions

A Claude Code session on Tanay's Unity zombie game just ended without /wrap. You are running unattended. Nobody will read your reply; only the files you write matter.

Input: .autowrap/transcript.md is a condensed transcript of the part of that session that was never saved: Tanay's messages, Claude's replies, files edited and commands run.

Steps:
1. Read CLAUDE.md (if it exists), STATE.md, _templates/Session.md, the newest file in sessions/, then .autowrap/transcript.md.
2. If the transcript has no real project work (only general questions or chit-chat), stop and write nothing.
3. Create sessions/{{FILE}}.md following _templates/Session.md.
   - Frontmatter: date: {{DATE}}, tags: [session], auto: true
   - Title: Session {{DATE}}: <short topic> (auto)
   - Record only what the transcript shows. Never guess or fill gaps.
   - Under Decisions, list possible decisions and mark each one (unconfirmed).
   - Link game systems with [[wikilinks]]. Keep it under 40 lines.
4. Rewrite STATE.md so it reflects where things stand at the end of the transcript.
   - Keep existing items the transcript does not touch.
   - Stay under 40 lines.
   - In its frontmatter, set updated: {{DATE}} and auto_updated_from: {{FILE}}
5. Add this line to the Sessions list in HOME.md: - [[{{FILE}}]] - <short topic> (auto)
6. Do not edit DESIGN.md, DECISIONS.md, or anything in systems/, _templates/ or .obsidian/.
'@
  $instructions = $instructions.Replace('{{FILE}}', $file).Replace('{{DATE}}', $date)
  [System.IO.File]::WriteAllText((Join-Path $WorkDir 'instructions.md'), $instructions, $Utf8)

  if ($DryRun) {
    Write-Log "dry run: would write sessions/$file.md from $userCount unsaved messages ($($text.Length) chars). See .autowrap/transcript.md"
    exit 0
  }

  $claude = (Get-Command claude -ErrorAction SilentlyContinue).Source
  if (-not $claude) { Write-Log 'error: claude is not on PATH'; exit 1 }

  $env:ZOMBIE_AUTOWRAP = '1'
  Set-Location -LiteralPath $Vault
  Write-Log "running: session $SessionId, $userCount unsaved messages -> sessions/$file.md"

  $ErrorActionPreference = 'Continue'  # Windows PowerShell treats native stderr as errors otherwise
  $out = & $claude -p 'Follow the instructions in .autowrap/instructions.md exactly.' --model $Model --permission-mode acceptEdits --allowedTools Read Write Edit Glob Grep 2>&1
  $summary = (($out | Out-String) -replace '\s+', ' ').Trim()
  Write-Log ('done: exit {0}. {1}' -f $LASTEXITCODE, (Cut $summary 400))
}
catch {
  Write-Log "error: $($_.Exception.Message)"
  exit 1
}
