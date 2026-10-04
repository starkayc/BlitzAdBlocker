# Derives the AdGuard-format blocklist from BlockedDomains.cs (the single
# source of truth). Output is committed to the repo root so it has a stable
# raw URL for AdGuard DNS subscriptions:
#   https://raw.githubusercontent.com/starkayc/BlitzAdBlocker/master/blitz-adblock.txt
param(
    [string]$CsFile = "src/BlitzAdBlocker/BlockedDomains.cs",
    [string]$Out = "blitz-adblock.txt"
)

$root = (Get-Location).Path
# Entries are packed several per line ("a.com", "b.com", ...), so match
# every quoted token on each line rather than anchoring to a single entry.
$hosts = Get-Content (Join-Path $root $CsFile) | ForEach-Object {
    foreach ($m in [regex]::Matches($_, '"(?<h>[^"]+)"')) { $m.Groups['h'].Value }
} | Sort-Object -Unique

$lines = @(
    "! Title: Blitz AdBlocker — Blitz ad-network domains",
    "! Description: Ad domains observed in the Blitz app (Aditude, Google AdX, prebid, video, DMP syncs). Blocks via DNS. Blitz itself is never modified.",
    "! Homepage: https://github.com/starkayc/BlitzAdBlocker",
    "! Expires: 1 day",
    "! Blocked hosts: $($hosts.Count)"
)
foreach ($h in $hosts) { $lines += "||$h^" }

$outPath = if ([System.IO.Path]::IsPathRooted($Out)) { $Out } else { Join-Path $root $Out }
[System.IO.File]::WriteAllLines($outPath, $lines, [System.Text.UTF8Encoding]::new($false))

"wrote $Out — $($hosts.Count) rules"