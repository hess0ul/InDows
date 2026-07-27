# Regenerates the bundled module catalog the Build screen reads.
#
# Reads the curated modules under public/modules (each folder = one module: snippet.xml or *.ps1,
# plus a README carrying its [InDows:module] anchor) and CATALOG.md (category / risk / description),
# and writes src/InDows.Gui/data/modules.catalog.json. Modules with no anchor (windowsPE-only or
# manual) are skipped — only anchor-graftable modules belong in the checklist.
#
# Modules source: the modules live in THIS repo, so the default ../modules is the normal case (reproducible;
# a CI drift --check can regenerate and diff). Override only if they're elsewhere; no hardcoded path.
#   pwsh ./tools/gen-catalog.ps1 [-ModulesDir <path-to-your-modules-folder>]
param([string]$ModulesDir = (Join-Path $PSScriptRoot '..\modules'))

$ErrorActionPreference = 'Stop'
$modulesDir = $ModulesDir
$catalogMd  = Join-Path $modulesDir 'CATALOG.md'
$copyPath   = Join-Path $PSScriptRoot 'catalog-copy.json'
$tweaksPath = Join-Path $PSScriptRoot 'catalog-tweaks.json'
$paramsPath = Join-Path $PSScriptRoot 'catalog-params.json'
$descPath   = Join-Path $PSScriptRoot 'catalog-tweak-descriptions.json'
$outPath    = Join-Path $PSScriptRoot '..\src\InDows.Gui\data\modules.catalog.json'

# Authored "why" / "risk" copy for the hover panel, keyed by module name (see catalog-copy.json).
$copy = Get-Content $copyPath -Raw | ConvertFrom-Json
# Individual settings for decomposed modules, keyed by module name (see catalog-tweaks.json).
$tweaksByModule = Get-Content $tweaksPath -Raw | ConvertFrom-Json
# User-filled fields for modules with __TOKEN__ placeholders, keyed by module name (see catalog-params.json).
$paramsByModule = Get-Content $paramsPath -Raw | ConvertFrom-Json
# Plain-language hover descriptions, keyed module -> tweak-id (see catalog-tweak-descriptions.json).
$descByModule = if (Test-Path $descPath) { Get-Content $descPath -Raw | ConvertFrom-Json } else { $null }
function Get-TweakDesc([string]$module, [string]$id) {
    if ($descByModule -and $descByModule.PSObject.Properties[$module]) {
        $mod = $descByModule.$module
        if ($mod.PSObject.Properties[$id]) { return [string]$mod.$id }
    }
    return ''
}

function Format-Label([string]$s) {
    $s = $s.Trim()
    $s = $s -replace '\s*\(comment out[^)]*\)', ''
    $s = $s -replace '\s*\(these [^)]*\)', ''
    $s = $s -replace '^-?\d+\s*=\s*0x[0-9A-Fa-f]+\s*=\s*', ''
    $s = $s -replace '^-?\d+\s*=\s*', ''
    $s = $s.Trim()
    if ($s.Length -gt 0) { $s = $s.Substring(0, 1).ToUpper() + $s.Substring(1) }
    return $s
}

function New-Id([string]$label, $used) {
    $id = ($label.ToLower() -replace '[^a-z0-9]+', '-').Trim('-')
    if ($id.Length -gt 40) { $id = $id.Substring(0, 40).Trim('-') }
    if ($id -eq '') { $id = 'tweak' }
    $base = $id; $i = 2
    while ($used.Contains($id)) { $id = "$base-$i"; $i++ }
    [void]$used.Add($id)
    return $id
}

# Extract individual settings from a pure-registry module. Only lines between the '=== start/done ==='
# markers count; each RegDword/RegString is labelled by its trailing comment or the group header above it.
# If any active line can't be captured (loops, powercfg, appx, an unlabelled reg line...), 'unhandled' is
# non-empty and the caller keeps the module as one checkbox rather than shipping a partial decomposition.
function Get-RegTweaks {
    param([string[]]$lines)
    $tweaks = @()
    $group = $null
    $pending = @()
    $unhandled = @()
    $active = $false

    foreach ($raw in $lines) {
        $t = $raw.Trim()

        if ($t -match '===\s*.*\bstart\b') { $active = $true;  continue }
        if ($t -match '===\s*.*\bdone\b')  { $active = $false; continue }
        if (-not $active -or $t -eq '') { continue }

        if ($t -match '^(function\b|\{|\}|New-Item\b|New-ItemProperty\b|Set-Item\b|Out-Null\b|\$ErrorActionPreference\b|Log\b)') { continue }
        if ($t -match '^\$[A-Za-z_]+\s*=') { continue }

        if ($t.StartsWith('#')) {
            $c  = $t.TrimStart('#').Trim()
            $c2 = ($c.Trim('-', ' ', '=', '#', '!')).Trim()
            if ($c -match '^#*\s*(Reg(Dword|String|Expand)|Log)\b') { continue }
            if ($c2 -eq '' -or $c2 -match '^(optional|ADVANCED|DANGER|RISK:|Option [AB]|Source:|NOTE:|\()') {
                if ($group -and $pending.Count) { $tweaks += @{ label = $group; content = ($pending -join "`n") } }
                $pending = @(); $group = $null; continue
            }
            if ($group -and $pending.Count) { $tweaks += @{ label = $group; content = ($pending -join "`n") } }
            $pending = @(); $group = $c2; continue
        }

        if ($t -match '^Reg(Dword|String|Expand)\b') {
            $body = $t; $trail = $null
            if ($t -match '^(.*?\S)\s+#\s*(.+)$') { $body = $Matches[1].Trim(); $trail = $Matches[2].Trim() }
            if ($trail) {
                if ($group -and $pending.Count) { $tweaks += @{ label = $group; content = ($pending -join "`n") }; $pending = @(); $group = $null }
                $tweaks += @{ label = $trail; content = $body }
            } elseif ($group) {
                $pending += $body
            } else {
                $unhandled += $body
            }
            continue
        }

        $unhandled += $t
    }
    if ($group -and $pending.Count) { $tweaks += @{ label = $group; content = ($pending -join "`n") } }
    return [pscustomobject]@{ tweaks = $tweaks; unhandled = $unhandled }
}

$green  = [char]::ConvertFromUtf32(0x1F7E2)
$yellow = [char]::ConvertFromUtf32(0x1F7E1)
$red    = [char]::ConvertFromUtf32(0x1F534)

function Get-CleanCategory([string]$c) {
    switch -Regex ($c) {
        'Privacy'     { return 'Privacy' }
        'UI'          { return 'UI & shell' }
        'Performance' { return 'Performance & gaming' }
        'Debloat'     { return 'Debloat & apps' }
        'Setup'       { return 'Identity & setup' }
        'System'      { return 'System' }
        default       { return (($c -replace '\s*\(.*?\)', '') -replace '\s+', ' ').Trim() }
    }
}

# --- Parse CATALOG.md: module -> {category, risk, description} ---
$meta = @{}
$currentCat = 'Other'
$catLines = Get-Content $catalogMd
for ($i = 0; $i -lt $catLines.Count; $i++) {
    $line = $catLines[$i]
    if ($line -match '^##\s+([^#].+?)\s*$') { $currentCat = Get-CleanCategory $Matches[1].Trim(); continue }
    if ($line -match '^###\s') {
        $names = [regex]::Matches($line, '`([^`]+)`') | ForEach-Object { $_.Groups[1].Value.Trim() }
        if ($names.Count -eq 0) { continue }
        $risk = 'safe'
        if ($line.Contains($red)) { $risk = 'risky' } elseif ($line.Contains($yellow)) { $risk = 'advanced' } elseif ($line.Contains($green)) { $risk = 'safe' }
        $desc = ''
        for ($j = $i + 1; $j -lt $catLines.Count; $j++) {
            $t = $catLines[$j].Trim()
            if ($t -eq '') { continue }
            if ($t.StartsWith('#') -or $t.StartsWith('|') -or $t.StartsWith('---')) { break }
            $desc = $t; break
        }
        foreach ($n in $names) { $meta[$n] = @{ category = $currentCat; risk = $risk; description = $desc } }
    }
}

# --- Each module folder ---
$modules = @()
foreach ($dir in (Get-ChildItem $modulesDir -Directory | Sort-Object Name)) {
    $name = $dir.Name
    $readmePath = Join-Path $dir.FullName 'README.md'
    $readme = if (Test-Path $readmePath) { Get-Content $readmePath -Raw } else { '' }

    $anchor = $null
    if ($readme -match '\[InDows:module\]\s+([a-z][a-z-]+)') { $anchor = $Matches[1] }
    # Only anchor-graftable modules belong in the checklist (disk = windowsPE, gpu-tuning = manual are skipped).
    if (-not $anchor) { Write-Host ("  skip (no anchor): {0}" -f $name); continue }

    $snippet = Join-Path $dir.FullName 'snippet.xml'
    if (Test-Path $snippet) {
        $kind = 'snippet'; $content = (Get-Content $snippet -Raw).TrimEnd()
    } else {
        $ps1 = Get-ChildItem $dir.FullName -Filter *.ps1 | Select-Object -First 1
        if (-not $ps1) { continue }
        $kind = 'script'; $content = (Get-Content $ps1.FullName -Raw).TrimEnd()
    }

    $m = if ($meta.ContainsKey($name)) { $meta[$name] } else { @{ category = 'Other'; risk = 'safe'; description = '' } }

    $why = ''; $riskNote = ''
    if ($copy.PSObject.Properties[$name]) {
        $why = $copy.$name.why; $riskNote = $copy.$name.risk
        # Optional category override (e.g. move a risky module into the Advanced category without moving its CATALOG.md row).
        if ($copy.$name.PSObject.Properties['category']) { $m.category = $copy.$name.category }
    }

    # Decomposed modules carry their individual settings; the rest stay a single whole-module checkbox.
    $tweaks = @()
    if ($tweaksByModule.PSObject.Properties[$name]) {
        # Hand-authored decomposition wins (curated labels, grouping, per-tweak risk).
        foreach ($t in $tweaksByModule.$name) {
            $tweaks += [ordered]@{ id = $t.id; label = $t.label; risk = $t.risk; default = [bool]$t.default; content = $t.content; description = (Get-TweakDesc $name $t.id) }
        }
    } elseif ($kind -eq 'script') {
        # Otherwise auto-extract from pure-registry modules; anything else stays a single checkbox.
        $ex = Get-RegTweaks (Get-Content $ps1.FullName)
        if ($ex.unhandled.Count -eq 0 -and $ex.tweaks.Count -ge 2) {
            $usedIds = New-Object System.Collections.Generic.HashSet[string]
            foreach ($t in $ex.tweaks) {
                $label = Format-Label $t.label
                $id = New-Id $label $usedIds
                $tweaks += [ordered]@{ id = $id; label = $label; risk = $m.risk; default = $true; content = $t.content; description = (Get-TweakDesc $name $id) }
            }
        }
    }

    # Modules whose content has __TOKEN__ placeholders carry the fields to fill them.
    $params = @()
    if ($paramsByModule.PSObject.Properties[$name]) {
        foreach ($p in $paramsByModule.$name) {
            $opts = @()
            if ($p.PSObject.Properties['options']) {
                foreach ($o in $p.options) { $opts += [ordered]@{ label = $o.label; value = $o.value } }
            }
            $params += [ordered]@{ key = $p.key; label = $p.label; kind = $p.kind; default = $p.default; options = $opts }
        }
    }

    $modules += [ordered]@{
        name        = $name
        category    = $m.category
        risk        = $m.risk
        kind        = $kind
        anchor      = $anchor
        description = $m.description
        why         = $why
        riskNote    = $riskNote
        content     = $content
        tweaks      = $tweaks
        params      = $params
    }
}

New-Item -ItemType Directory -Force (Split-Path $outPath) | Out-Null
$json = [ordered]@{ modules = $modules } | ConvertTo-Json -Depth 8
[System.IO.File]::WriteAllText($outPath, $json, (New-Object System.Text.UTF8Encoding($false)))
Write-Host ("Wrote {0} modules -> {1}" -f $modules.Count, $outPath)

# Bundle the anchored base autounattend.xml (the Build screen grafts the selected modules onto it).
$basePath = Join-Path (Split-Path $modulesDir) 'autounattend.xml'
$baseOut  = Join-Path (Split-Path $outPath) 'autounattend.base.xml'
Copy-Item $basePath $baseOut -Force
Write-Host ("Copied base   -> {0}" -f $baseOut)
Write-Host "=== sample (name | category | risk | kind | anchor) ==="
$modules | ForEach-Object { "{0,-20} {1,-28} {2,-9} {3,-8} {4}" -f $_.name, $_.category, $_.risk, $_.kind, $_.anchor }
