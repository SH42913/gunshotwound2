#Requires -Version 5.1
<#
.SYNOPSIS
  Builds GunshotWound2_<Variant>.oiv from Release output and SHVDN (no Installer\...\content\ staging).
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [ValidateSet('Enhanced', 'Legacy')]
    [string] $InstallerVariant,

    [Parameter(Mandatory = $false)]
    [string] $RepoRoot,

    [Parameter(Mandatory = $false)]
    [string] $TargetDir,

    [Parameter(Mandatory = $false)]
    [string] $ShvdnDir,

    [Parameter(Mandatory = $false)]
    [string] $OivOutput
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $InstallerVariant) { $InstallerVariant = $env:GSW2_INSTALLER_VARIANT }
if (-not $RepoRoot) { $RepoRoot = $env:GSW2_REPO_ROOT }
if (-not $TargetDir) { $TargetDir = $env:GSW2_TARGET_DIR }
if (-not $ShvdnDir) { $ShvdnDir = $env:GSW2_SHVDN_DIR }
if (-not $OivOutput) { $OivOutput = $env:GSW2_OIV_OUTPUT }

if ($InstallerVariant -notin @('Enhanced', 'Legacy')) {
    throw 'InstallerVariant must be Enhanced or Legacy.'
}
foreach ($pair in @(
        @{ Name = 'RepoRoot'; Value = $RepoRoot }
        @{ Name = 'TargetDir'; Value = $TargetDir }
        @{ Name = 'ShvdnDir'; Value = $ShvdnDir }
    )) {
    if ([string]::IsNullOrWhiteSpace($pair.Value)) {
        throw "$($pair.Name) is required."
    }
}

function Resolve-DirectoryPath([string]$path) {
    $trimmed = $path.Trim().Trim('"').TrimEnd('\', '/')
    return [System.IO.Path]::GetFullPath($trimmed)
}

function Resolve-FilePath([string]$path) {
    $trimmed = $path.Trim().Trim('"')
    return [System.IO.Path]::GetFullPath($trimmed)
}

function Assert-FileExists([string]$path, [string]$label) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing $label`: $path"
    }
}

function Get-ContentZipEntryName([string]$relativeUnderContent) {
    $normalized = $relativeUnderContent.TrimStart('\', '/').Replace('/', '\')
    return "content\$normalized"
}

$repoRootFull = Resolve-DirectoryPath $RepoRoot
$targetDirFull = Resolve-DirectoryPath $TargetDir
$shvdnDirFull = Resolve-DirectoryPath $ShvdnDir
$installerDir = Join-Path $repoRootFull "Installer\$InstallerVariant"

$assemblyPath = Join-Path $installerDir 'assembly.xml'
$iconPath = Join-Path $installerDir 'icon.png'
Assert-FileExists $assemblyPath 'assembly.xml'
Assert-FileExists $iconPath 'icon.png'

if ([string]::IsNullOrWhiteSpace($OivOutput)) {
    $OivOutput = Join-Path $installerDir "GunshotWound2_$InstallerVariant.oiv"
}
$oivOutputFull = Resolve-FilePath $OivOutput
$oivParent = Split-Path -Parent $oivOutputFull
if (-not (Test-Path -LiteralPath $oivParent)) {
    New-Item -ItemType Directory -Path $oivParent -Force | Out-Null
}

$excludeLeafNames = @('GunshotWound2.pdb', 'Morpeh.pdb', 'WeightedRandomizer.xml', 'WeightedRandomizer.pdb')
$shipFiles = @(Get-ChildItem -LiteralPath $targetDirFull -Recurse -File |
    Where-Object { $excludeLeafNames -notcontains $_.Name })

foreach ($shvdnFile in @('ScriptHookVDotNet.asi', 'ScriptHookVDotNet3.dll')) {
    Assert-FileExists (Join-Path $shvdnDirFull $shvdnFile) "SHVDN $shvdnFile"
}

if (Test-Path -LiteralPath $oivOutputFull) {
    Remove-Item -LiteralPath $oivOutputFull -Force
}

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem

$compressionLevel = [System.IO.Compression.CompressionLevel]::Optimal
$entryCount = 0
$zip = [System.IO.Compression.ZipFile]::Open($oivOutputFull, [System.IO.Compression.ZipArchiveMode]::Create)

try {
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
        $zip, $assemblyPath, 'assembly.xml', $compressionLevel) | Out-Null
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
        $zip, $iconPath, 'icon.png', $compressionLevel) | Out-Null
    $entryCount += 2

    foreach ($file in $shipFiles) {
        $relative = $file.FullName.Substring($targetDirFull.Length).TrimStart('\', '/')
        $entryName = Get-ContentZipEntryName $relative
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $zip, $file.FullName, $entryName, $compressionLevel) | Out-Null
        $entryCount++
    }

    foreach ($shvdnFile in @('ScriptHookVDotNet.asi', 'ScriptHookVDotNet3.dll')) {
        $src = Join-Path $shvdnDirFull $shvdnFile
        $entryName = Get-ContentZipEntryName $shvdnFile
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
            $zip, $src, $entryName, $compressionLevel) | Out-Null
        $entryCount++
    }
}
finally {
    $zip.Dispose()
}

if (-not (Test-Path -LiteralPath $oivOutputFull -PathType Leaf)) {
    throw "Pack failed: $oivOutputFull was not created."
}
$sizeBytes = (Get-Item -LiteralPath $oivOutputFull).Length
if ($sizeBytes -le 0) {
    throw "Pack failed: $oivOutputFull is empty."
}

Write-Host "Packed $entryCount entries ($sizeBytes bytes) -> $oivOutputFull"
