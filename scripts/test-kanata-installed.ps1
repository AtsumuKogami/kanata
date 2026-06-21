param(
    [string]$Configuration = "Debug",
    [string]$SmokeRoot
)

$ErrorActionPreference = "Stop"

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

    throw "The 'kanata' command was not found. Install it with scripts/install-kanata-dev.ps1."
}

function Invoke-Kanata {
    param(
        [string[]]$Arguments,
        [string]$WorkingDirectory
    )

    Push-Location $WorkingDirectory
    try {
        Write-Host "kanata $($Arguments -join ' ')"
        & $script:KanataCommand @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "kanata $($Arguments -join ' ') failed with exit code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}

$script:KanataCommand = Resolve-KanataCommand

$repoRoot = $env:KANATA_REPOSITORY_ROOT
if ([string]::IsNullOrWhiteSpace($repoRoot)) {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
}

if ([string]::IsNullOrWhiteSpace($SmokeRoot)) {
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $SmokeRoot = Join-Path $repoRoot ".kanata/dev-smoke-tests/$timestamp"
}

New-Item -ItemType Directory -Force -Path $SmokeRoot | Out-Null

Write-Host "Smoke test root: $SmokeRoot"

Invoke-Kanata -Arguments @("version") -WorkingDirectory $SmokeRoot
Invoke-Kanata -Arguments @("engine", "build", $Configuration) -WorkingDirectory $SmokeRoot
Invoke-Kanata -Arguments @("create", "SmokeGame") -WorkingDirectory $SmokeRoot

$projectRoot = Join-Path $SmokeRoot "SmokeGame"

Invoke-Kanata -Arguments @("validate") -WorkingDirectory $projectRoot
Invoke-Kanata -Arguments @("generate", "desktop", $Configuration) -WorkingDirectory $projectRoot
Invoke-Kanata -Arguments @("build", "desktop", $Configuration) -WorkingDirectory $projectRoot
Invoke-Kanata -Arguments @("play", "desktop", $Configuration) -WorkingDirectory $projectRoot

Write-Host "Installed Kanata smoke test passed."
