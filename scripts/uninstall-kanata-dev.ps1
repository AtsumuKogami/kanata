param(
    [switch]$KeepRepositoryRootVariable,
    [switch]$KeepDevConfig
)

$ErrorActionPreference = "Stop"

Write-Host "Uninstalling Kanata dev tool..."
& dotnet tool uninstall --global Kanata.Build | Out-Null

if (-not $KeepRepositoryRootVariable) {
    [Environment]::SetEnvironmentVariable("KANATA_REPOSITORY_ROOT", $null, "User")
    Remove-Item Env:KANATA_REPOSITORY_ROOT -ErrorAction SilentlyContinue
    Write-Host "Removed user environment variable KANATA_REPOSITORY_ROOT."
}

if (-not $KeepDevConfig) {
    $devConfigPath = Join-Path (Join-Path $HOME ".kanata") "dev-install.json"
    if (Test-Path $devConfigPath) {
        Remove-Item -Force $devConfigPath
        Write-Host "Removed dev install config: $devConfigPath"
    }
}

Write-Host "Kanata dev tool uninstalled."
