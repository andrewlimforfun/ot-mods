<#
.SYNOPSIS
    Syncs [package.dependencies] versions in a thunderstore.toml from Directory.Build.props values.

.DESCRIPTION
    Called by the Directory.Build.targets SyncThunderstoreDeps target before tcli publish.
    Can also be run standalone to update all thunderstore.toml files at once:
        .\sync-thunderstore-deps.ps1

.PARAMETER TomlFile
    Path to the specific thunderstore.toml to update. If omitted, updates all toml files under the repo root.

.PARAMETER BepInExVersion
    Override for BepInEx-BepInExPack version (defaults to value in Directory.Build.props).

.PARAMETER AlphaVersion
    Override for AndrewLin-Alpha version (defaults to value in Directory.Build.props).

.PARAMETER FomoVersion
    Override for AndrewLin-Fomo version (defaults to value in Directory.Build.props).
#>
param(
    [string]$TomlFile,
    [string]$BepInExVersion,
    [string]$AlphaVersion,
    [string]$FomoVersion
)

$repoRoot = $PSScriptRoot

# Read versions from Directory.Build.props if not supplied as parameters
if (-not $BepInExVersion -or -not $AlphaVersion -or -not $FomoVersion) {
    $propsPath = Join-Path $repoRoot "Directory.Build.props"
    [xml]$props = Get-Content $propsPath
    $pg = $props.Project.PropertyGroup | Where-Object { $_.BepInExVersion -or $_.AlphaVersion -or $_.FomoVersion }
    if (-not $BepInExVersion) { $BepInExVersion = ($props.Project.PropertyGroup | ForEach-Object { $_.BepInExVersion } | Where-Object { $_ }) -join "" }
    if (-not $AlphaVersion)   { $AlphaVersion   = ($props.Project.PropertyGroup | ForEach-Object { $_.AlphaVersion }   | Where-Object { $_ }) -join "" }
    if (-not $FomoVersion)    { $FomoVersion     = ($props.Project.PropertyGroup | ForEach-Object { $_.FomoVersion }    | Where-Object { $_ }) -join "" }
}

function Update-TomlDeps {
    param([string]$Path)

    $content = Get-Content $Path -Raw
    $original = $content

    if ($BepInExVersion) {
        $content = $content -replace 'BepInEx-BepInExPack = "[^"]+"', "BepInEx-BepInExPack = `"$BepInExVersion`""
    }
    if ($AlphaVersion) {
        $content = $content -replace 'AndrewLin-Alpha = "[^"]+"', "AndrewLin-Alpha = `"$AlphaVersion`""
    }
    if ($FomoVersion) {
        $content = $content -replace 'AndrewLin-Fomo = "[^"]+"', "AndrewLin-Fomo = `"$FomoVersion`""
    }

    if ($content -ne $original) {
        $utf8NoBom = New-Object System.Text.UTF8Encoding $false
        [System.IO.File]::WriteAllText([System.IO.Path]::GetFullPath($Path), $content, $utf8NoBom)
        Write-Host "Updated: $Path"
    }
}

if ($TomlFile) {
    Update-TomlDeps -Path $TomlFile
} else {
    # Standalone mode: update all thunderstore.toml files in the repo
    Get-ChildItem -Path $repoRoot -Recurse -Filter "thunderstore.toml" | ForEach-Object {
        Update-TomlDeps -Path $_.FullName
    }
}
