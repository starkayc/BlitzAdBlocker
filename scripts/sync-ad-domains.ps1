# Scans an extracted Blitz app.asar for ad-network domains and appends any
# newly seen ones to BlockedDomains.cs. Used by .github/workflows/ad-scan.yml.
#
#   pwsh scripts/sync-ad-domains.ps1 -AppDir path/to/asar-extracted-root
#
# Only hosts whose registrable suffix is a known ad-infrastructure suffix are
# considered, so app/CDN/analytics domains never get blocked.
param(
    [Parameter(Mandatory)][string]$AppDir,
    [string]$ListFile = "src/BlitzAdBlocker/BlockedDomains.cs",
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'

# Registered-domain suffixes of ad infrastructure. Curated, matches the
# original live-capture list plus known partners.
$AdSuffixes = @(
    'aditude.io', 'aditude.cloud', 'doubleclick.net', 'googleadservices.com',
    'googlesyndication.com', '2mdn.net', 'kueezrtb.com', 'quantserve.com',
    'quantcount.com', 'rlcdn.com', 'criteo.com', 'criteo.net',
    'amazon-adsystem.com', 'id5-sync.com', 'ad.gt', 'openwebmp.com',
    'bidswitch.net', '3lift.com', 'crwdcntrl.net', 'omnitagjs.com', 'a-mo.net',
    'fastclick.net', '33across.com', 'hadronid.net', 'ipredictive.com',
    'fwmrm.net', 'optable.co', 'everesttech.net', 'inmobi.com',
    'privacymanager.io', 'prebid.cloud', 'pubmatic.com',
    'openx.net', 'adsrvr.org', 'casalemedia.com', 'sascdn.com',
    'smartadserver.com', 'rubiconproject.com', 'primis.tech', 'viralize.tv',
    '360yield.com', 'nexverse.ai', 'blismedia.com', 'kargo.com',
    'admanmedia.com', 'adkernel.com', '1rx.io', 'resetdigital.co',
    'rtbwise.com', 'vindicosuite.com', 'activemetering.com', 'iionads.com',
    'unrulymedia.com', 'doubleverify.com', 'contextweb.com', 'intentiq.com',
    'nexx360.io', 'dxkulture.com', 'servenobid.com', 'bidr.io', 'avocet.io',
    'yieldmo.com', 'taboola.com', 'outbrain.com', 'mgid.com', 'media.net',
    'oxygenidl.com', 'zeotap.com', 'vidazoo.com', 'playstream.media'
) | Sort-Object -Unique

# Never block, even if a suffix matched.
$Excluded = @(
    'blitz.gg', 'blitzapp.gg', 'google.com'
)

# Existing blocked hosts.
$existing = @()
if (Test-Path $ListFile) {
    $existing = Get-Content $ListFile | ForEach-Object {
        foreach ($m in [regex]::Matches($_, '"(?<h>[^"]+)"')) { $m.Groups['h'].Value.ToLowerInvariant() }
    }
}
$existingSet = [System.Collections.Generic.HashSet[string]]::new([string[]]$existing, [System.StringComparer]::OrdinalIgnoreCase)

# Extract candidate hosts from every JS bundle. A host must be a quoted
# literal to avoid matching random identifier chunks in minified code.
$seen = [System.Collections.Generic.HashSet[string]]::new([string[]]@(), [System.StringComparer]::OrdinalIgnoreCase)
$rx = [regex]'(?:["''/])(?<!\.)([a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?(?:\.[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?)+)\.(?:com|net|io|org|co|cloud|tv|ai|xl|gt|me|gg|xyz|app|dev|tech|media|pro|info|biz|us|de|se|nl|fr|it|es|ru|ca|au|in|uk)["''/]'

$files = Get-ChildItem -Path $AppDir -Recurse -File -Include *.js,*.json,*.html -ErrorAction SilentlyContinue
foreach ($f in $files) {
    foreach ($m in $rx.Matches([string]::Join("`n", (Get-Content $f.FullName -ErrorAction SilentlyContinue)))) {
        $null = $seen.Add($m.Groups[1].Value.ToLowerInvariant())
    }
}

# Keep only ad-infrastructure sound-alikes, minus what we already block.
$added = @()
foreach ($h in $seen) {
    if ($h -in $Excluded) { continue }
    $keep = $false
    foreach ($s in $AdSuffixes) {
        if ($h -eq $s -or $h.EndsWith('.' + $s)) { $keep = $true; break }
    }
    if (-not $keep) { continue }

    # Skip anything already blocked OR a parent/child of something blocked.
    $covered = $false
    foreach ($e in $existingSet) {
        if ($h -eq $e -or $e.EndsWith('.' + $h) -or $h.EndsWith('.' + $e)) { $covered = $true; break }
    }
    if ($covered) { continue }
    $added += $h
}
$added = $added | Sort-Object -Unique

if ($added.Count -eq 0) {
    "NO NEW DOMAINS"
    exit 0
}

"NEW AD DOMAINS: $($added.Count)"
$added | ForEach-Object { "  $_" }

if ($DryRun) { exit 0 }

# Append to the List array, keeping the hand-written entry block intact.
$replace = "`n        // auto-added from Blitz bundle scan"
foreach ($h in $added) { $replace += "`n        `"$h`"," }
$replace += "`n    };"
$content = Get-Content $ListFile -Raw
$content = $content.Replace("`n    };", $replace)
[System.IO.File]::WriteAllText(
    (Resolve-Path $ListFile),
    $content.Replace("`r`n", "`n"),
    [System.Text.UTF8Encoding]::new($false))
"BlockedDomains.cs updated: $($existing.Count) -> $($existing.Count + $added.Count) hosts"