# SessionEnd hook. Hands the finished session's transcript to a background
# worker and exits right away (SessionEnd hooks only get a few seconds).
$ErrorActionPreference = 'SilentlyContinue'

# The auto-wrap Claude sets this, so its own session end doesn't start another one.
if ($env:ZOMBIE_AUTOWRAP -eq '1') { exit 0 }

$raw = [Console]::In.ReadToEnd()
try { $data = $raw | ConvertFrom-Json } catch { exit 0 }
if (-not $data.transcript_path) { exit 0 }

$worker = Join-Path $PSScriptRoot 'auto-wrap-worker.ps1'
$exe = (Get-Command pwsh -ErrorAction SilentlyContinue).Source
if (-not $exe) { $exe = 'powershell.exe' }

Start-Process -FilePath $exe -WindowStyle Hidden -ArgumentList @(
  '-NoProfile', '-ExecutionPolicy', 'Bypass',
  '-File', "`"$worker`"",
  '-TranscriptPath', "`"$($data.transcript_path)`"",
  '-SessionId', "$($data.session_id)"
)
exit 0
