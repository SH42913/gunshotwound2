#Requires -Version 5.1
<#
.SYNOPSIS
  Pins a ScriptHookVDotNet release in Build/Shvdn.props (tag + SHA256).

.DESCRIPTION
  Downloads the release .zip for Enhanced or Nightly, computes SHA256, and updates
  Build/Shvdn.props. Use after publishing or when bumping the reference SHVDN version.

.PARAMETER Target
  Enhanced (Chiheb-Bacha/ScriptHookVDotNetEnhanced) or Nightly (scripthookvdotnet-nightly).

.PARAMETER Tag
  GitHub release tag, e.g. v1.1.0.6 or v3.6.0-nightly.89 (must match the release tag exactly).

.PARAMETER SkipPropsUpdate
  Download and print hash only; do not modify Shvdn.props.

.EXAMPLE
  .\tools\update-shvdn.ps1 -Target Enhanced -Tag v1.1.0.6

.EXAMPLE
  .\tools\update-shvdn.ps1 -Target Nightly -Tag v3.6.0-nightly.89
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Enhanced', 'Nightly')]
    [string] $Target,

    [Parameter(Mandatory = $true)]
    [string] $Tag,

    [switch] $SkipPropsUpdate
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$propsPath = Join-Path $repoRoot 'Build\Shvdn.props'

if (-not (Test-Path -LiteralPath $propsPath)) {
    throw "Not found: $propsPath"
}

$config = switch ($Target) {
    'Enhanced' {
        @{
            ZipName     = "ScriptHookVDotNetEnhanced-$Tag.zip"
            DownloadUrl = "https://github.com/Chiheb-Bacha/ScriptHookVDotNetEnhanced/releases/download/$Tag/ScriptHookVDotNetEnhanced-$Tag.zip"
            ApiUrl      = "https://api.github.com/repos/Chiheb-Bacha/ScriptHookVDotNetEnhanced/releases/tags/$Tag"
            TagElement  = 'ShvdnEnhancedTag'
            ShaElement  = 'ShvdnEnhancedSha256'
        }
    }
    'Nightly' {
        @{
            ZipName     = "ScriptHookVDotNet-$Tag.zip"
            DownloadUrl = "https://github.com/scripthookvdotnet/scripthookvdotnet-nightly/releases/download/$Tag/ScriptHookVDotNet-$Tag.zip"
            ApiUrl      = "https://api.github.com/repos/scripthookvdotnet/scripthookvdotnet-nightly/releases/tags/$Tag"
            TagElement  = 'ShvdnNightlyTag'
            ShaElement  = 'ShvdnNightlySha256'
        }
    }
}

$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) "gsw2-update-shvdn"
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null
$zipPath = Join-Path $tempDir $config.ZipName

Write-Host "Target: $Target"
Write-Host "Tag:    $Tag"
Write-Host "URL:    $($config.DownloadUrl)"
Write-Host "Saving: $zipPath"

try {
    Invoke-WebRequest -Uri $config.DownloadUrl -OutFile $zipPath -UseBasicParsing
}
catch {
    throw "Download failed: $($config.DownloadUrl). $_"
}

$sha256 = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
Write-Host "SHA256: $sha256"

$githubDigest = $null
try {
    $release = Invoke-RestMethod -Uri $config.ApiUrl -Headers @{ 'User-Agent' = 'gunshotwound2-update-shvdn' }
    $asset = $release.assets | Where-Object { $_.name -eq $config.ZipName } | Select-Object -First 1
    if ($asset -and $asset.digest -match '^sha256:(?<h>[a-fA-F0-9]{64})$') {
        $githubDigest = $Matches['h'].ToLowerInvariant()
        if ($githubDigest -ne $sha256) {
            throw "Computed SHA256 does not match GitHub asset digest. Local: $sha256 GitHub: $githubDigest"
        }
        Write-Host "GitHub release digest matches."
    }
}
catch {
    if ($_.Exception.Message -match 'Computed SHA256') {
        throw
    }
    Write-Warning "Could not verify against GitHub API (optional): $($_.Exception.Message)"
}

if ($SkipPropsUpdate) {
    Write-Host 'SkipPropsUpdate set; Shvdn.props unchanged.'
    exit 0
}

$content = Get-Content -LiteralPath $propsPath -Raw -Encoding UTF8
$tagPattern = "(<$($config.TagElement)>)[^<]*(</$($config.TagElement)>)"
$shaPattern = "(<$($config.ShaElement)>)[^<]*(</$($config.ShaElement)>)"

if ($content -notmatch $tagPattern) {
    throw "Element '$($config.TagElement)' not found in Shvdn.props"
}
if ($content -notmatch $shaPattern) {
    throw "Element '$($config.ShaElement)' not found in Shvdn.props"
}

$content = [regex]::Replace($content, $tagPattern, "`${1}$Tag`${2}")
$content = [regex]::Replace($content, $shaPattern, "`${1}$sha256`${2}")

if ($Target -eq 'Enhanced') {
    $commentPattern = '<!-- GitHub release asset digest \([^)]+\) -->'
    $newComment = "<!-- GitHub release asset digest ($($config.ZipName)) -->"
    if ($content -match $commentPattern) {
        $content = [regex]::Replace($content, $commentPattern, $newComment)
    }
}

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($propsPath, $content, $utf8NoBom)

Write-Host "Updated: $propsPath"
Write-Host ""
Write-Host "Clear cached restore if you changed version:"
Write-Host "  Remove-Item -Recurse -Force (Join-Path '$repoRoot' '.shvdn\$Target')"
Write-Host "  (or only the old tag subfolder under .shvdn\$Target\)"
