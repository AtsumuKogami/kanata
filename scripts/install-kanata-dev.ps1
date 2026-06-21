param(
    [string]$Configuration = "Debug",
    [switch]$SkipSmokeTest,
    [switch]$SkipEngineBuild
)

$ErrorActionPreference = "Stop"

function Invoke-DotNet {
    param(
        [string[]]$Arguments,
        [string]$WorkingDirectory
    )

    Push-Location $WorkingDirectory
    try {
        & dotnet @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet $($Arguments -join ' ') failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}

function Invoke-ToolOptional {
    param([string[]]$Arguments)

    & dotnet @Arguments | Out-Null
    return $LASTEXITCODE
}

function Get-KanataVersion {
    param([string]$RepoRoot)

    $propsPath = Join-Path $RepoRoot "Directory.Build.props"
    if (-not (Test-Path $propsPath)) {
        return "0.1.0"
    }

    [xml]$props = Get-Content $propsPath
    $version = $props.Project.PropertyGroup.Version
    if ([string]::IsNullOrWhiteSpace($version)) {
        return "0.1.0"
    }

    return $version
}

function Get-NuGetGlobalPackagesPath {
    $output = & dotnet nuget locals global-packages --list
    foreach ($line in $output) {
        if ($line -match "^global-packages:\s*(.+)$") {
            return $Matches[1].Trim()
        }
    }

    return $null
}

function Resolve-KanataCommand {
    $command = Get-Command "kanata" -ErrorAction SilentlyContinue
    if ($null -ne $command) {
        return $command.Source
    }

    $isWindows = [System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform(
        [System.Runtime.InteropServices.OSPlatform]::Windows)
    $fileName = if ($isWindows) { "kanata.exe" } else { "kanata" }
    $fallback = Join-Path (Join-Path $HOME ".dotnet") (Join-Path "tools" $fileName)

    if (Test-Path $fallback) {
        return $fallback
    }

    throw "Kanata tool was installed, but the 'kanata' command was not found. Make sure ~/.dotnet/tools is in PATH."
}

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$packageOutput = Join-Path $repoRoot ".kanata/tool-packages"
$version = Get-KanataVersion -RepoRoot $repoRoot

Write-Host "Kanata repository: $repoRoot"
Write-Host "Configuration: $Configuration"
Write-Host "Version: $version"

New-Item -ItemType Directory -Force -Path $packageOutput | Out-Null

$solutionPath = Join-Path $repoRoot "Kanata.sln"
if (-not (Test-Path $solutionPath)) {
    $solutionPath = Join-Path $repoRoot "Kanata.slnx"
}
if (-not (Test-Path $solutionPath)) {
    throw "Kanata solution file was not found in $repoRoot"
}

Write-Host ""
Write-Host "==> Building repository"
Invoke-DotNet -Arguments @("build", $solutionPath, "-c", $Configuration) -WorkingDirectory $repoRoot

Write-Host ""
Write-Host "==> Packing Kanata CLI tool"
Invoke-DotNet -Arguments @(
    "pack",
    (Join-Path $repoRoot "src/Tools/Kanata.Build/Kanata.Build.csproj"),
    "-c", $Configuration,
    "-o", $packageOutput,
    "--no-build") -WorkingDirectory $repoRoot

Write-Host ""
Write-Host "==> Removing old global tool"
$null = Invoke-ToolOptional -Arguments @("tool", "uninstall", "--global", "Kanata.Build")

$nugetGlobal = Get-NuGetGlobalPackagesPath
if (-not [string]::IsNullOrWhiteSpace($nugetGlobal)) {
    $cachedPackage = Join-Path $nugetGlobal (Join-Path "kanata.build" $version)
    if (Test-Path $cachedPackage) {
        Write-Host "Removing cached package: $cachedPackage"
        Remove-Item -Recurse -Force $cachedPackage
    }
}

Write-Host ""
Write-Host "==> Installing global tool"
Invoke-DotNet -Arguments @(
    "tool",
    "install",
    "--global",
    "Kanata.Build",
    "--add-source", $packageOutput,
    "--version", $version,
    "--ignore-failed-sources") -WorkingDirectory $repoRoot

[Environment]::SetEnvironmentVariable("KANATA_REPOSITORY_ROOT", $repoRoot, "User")
$env:KANATA_REPOSITORY_ROOT = $repoRoot

$devConfigDir = Join-Path $HOME ".kanata"
$devConfigPath = Join-Path $devConfigDir "dev-install.json"
New-Item -ItemType Directory -Force -Path $devConfigDir | Out-Null
$devConfig = [ordered]@{
    repositoryRoot = $repoRoot
    version = $version
    configuration = $Configuration
    installedAt = (Get-Date).ToString("o")
}
$devConfig | ConvertTo-Json | Set-Content -Path $devConfigPath -Encoding UTF8
Write-Host "Saved dev install config: $devConfigPath"

$kanata = Resolve-KanataCommand

Write-Host ""
Write-Host "==> Checking installed command"
& $kanata version
if ($LASTEXITCODE -ne 0) {
    throw "kanata version failed with exit code $LASTEXITCODE"
}

if (-not $SkipEngineBuild) {
    Write-Host ""
    Write-Host "==> Building engine components through installed command"
    & $kanata engine build $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "kanata engine build failed with exit code $LASTEXITCODE"
    }
}

if (-not $SkipSmokeTest) {
    Write-Host ""
    Write-Host "==> Running installed command smoke test"
    & (Join-Path $PSScriptRoot "test-kanata-installed.ps1") -Configuration $Configuration
    if ($LASTEXITCODE -ne 0) {
        throw "Installed Kanata smoke test failed with exit code $LASTEXITCODE"
    }
}

Write-Host ""
Write-Host "Kanata dev tool installed successfully."
Write-Host "Repository root was saved to user environment variable KANATA_REPOSITORY_ROOT."
