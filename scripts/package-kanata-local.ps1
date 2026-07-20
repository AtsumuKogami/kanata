param(
    [string]$Configuration = "Debug",
    [string]$TargetFramework = "net10.0",
    [string]$OutputDirectory = "artifacts/packages"
)

$ErrorActionPreference = "Stop"

function Copy-KanataArtifact {
    param(
        [string]$SourcePath,
        [string]$DestinationDirectory
    )

    if (!(Test-Path $SourcePath)) {
        throw "Required artifact was not found: $SourcePath"
    }

    New-Item -ItemType Directory -Force -Path $DestinationDirectory | Out-Null
    Copy-Item -Force -Path $SourcePath -Destination $DestinationDirectory
}

function Clear-PackageArtifacts {
    param([string]$PackageDirectory)

    $artifacts = Join-Path $PackageDirectory "artifacts"
    if (Test-Path $artifacts) {
        Remove-Item -Recurse -Force $artifacts
    }
}

function Pack-KanataPackage {
    param(
        [string]$PackageDirectory,
        [string]$OutputPath
    )

    dotnet run --project src/Tools/Kanata.Cli -- package pack $PackageDirectory -o $OutputPath --force
    dotnet run --project src/Tools/Kanata.Cli -- package verify $OutputPath
}

Write-Host "Building Kanata solution..."
dotnet build Kanata.slnx -c $Configuration

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

$corePackage = "samples/packages/kanata.core-runtime"
$coreArtifactDirectory = Join-Path $corePackage "artifacts/lib/$TargetFramework"
Clear-PackageArtifacts $corePackage
Copy-KanataArtifact `
    -SourcePath "src/Engine/Kanata.Core/bin/$Configuration/$TargetFramework/Kanata.Core.dll" `
    -DestinationDirectory $coreArtifactDirectory
Pack-KanataPackage `
    -PackageDirectory $corePackage `
    -OutputPath (Join-Path $OutputDirectory "kanata.core-0.1.0.kpkg")

$monoGamePackage = "samples/packages/kanata.backend.monogame"
$monoGameArtifactDirectory = Join-Path $monoGamePackage "artifacts/lib/$TargetFramework"
Clear-PackageArtifacts $monoGamePackage
Copy-KanataArtifact `
    -SourcePath "src/Backends/Kanata.Backend.MonoGame/bin/$Configuration/$TargetFramework/Kanata.Backend.MonoGame.dll" `
    -DestinationDirectory $monoGameArtifactDirectory
Pack-KanataPackage `
    -PackageDirectory $monoGamePackage `
    -OutputPath (Join-Path $OutputDirectory "kanata.backend.monogame-0.1.0.kpkg")

$engineerPackage = "samples/packages/example.engineer-tool"
$engineerArtifactDirectory = Join-Path $engineerPackage "artifacts/tools/example.engineer"
New-Item -ItemType Directory -Force -Path $engineerArtifactDirectory | Out-Null
Set-Content `
    -Path (Join-Path $engineerArtifactDirectory "example.engineer.txt") `
    -Value "Example Engineer tool placeholder artifact." `
    -NoNewline
Pack-KanataPackage `
    -PackageDirectory $engineerPackage `
    -OutputPath (Join-Path $OutputDirectory "example.engineer-0.1.0.kpkg")

Write-Host "Packages written to $OutputDirectory"
