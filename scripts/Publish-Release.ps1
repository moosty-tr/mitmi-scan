param(
    [string] $Version = '0.1.0',
    [string] $RuntimeIdentifier = 'win-x64',
    [switch] $SelfContained,
    [switch] $FrameworkDependent,
    [string] $ReleaseRoot,
    [string] $OutputRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$cliProjectPath = Join-Path $repoRoot 'src\Mitmi.Scan.Cli\Mitmi.Scan.Cli.csproj'

if ($SelfContained.IsPresent -and $FrameworkDependent.IsPresent) {
    throw 'Use either -SelfContained or -FrameworkDependent, not both.'
}

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot 'artifacts\publish'
}

if ([string]::IsNullOrWhiteSpace($ReleaseRoot)) {
    $ReleaseRoot = Join-Path $repoRoot 'artifacts\release'
}

$repoRootFull = [System.IO.Path]::GetFullPath($repoRoot)
$outputRootFull = [System.IO.Path]::GetFullPath($OutputRoot)
$releaseRootFull = [System.IO.Path]::GetFullPath($ReleaseRoot)

if (-not $outputRootFull.StartsWith($repoRootFull, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Output root must stay inside the repository: $outputRootFull"
}

if (-not $releaseRootFull.StartsWith($repoRootFull, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Release root must stay inside the repository: $releaseRootFull"
}

$packageName = "mitmi-scan-v$Version-$RuntimeIdentifier"
$outputDirectory = Join-Path $outputRootFull $packageName
$outputDirectoryFull = [System.IO.Path]::GetFullPath($outputDirectory)
$zipPath = Join-Path $releaseRootFull "$packageName.zip"
$zipPathFull = [System.IO.Path]::GetFullPath($zipPath)

if (-not $outputDirectoryFull.StartsWith($outputRootFull, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Output directory must stay inside the publish output root: $outputDirectoryFull"
}

if (-not $zipPathFull.StartsWith($releaseRootFull, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Release archive must stay inside the release output root: $zipPathFull"
}

if (Test-Path -LiteralPath $outputDirectoryFull) {
    Remove-Item -LiteralPath $outputDirectoryFull -Recurse -Force
}

if (Test-Path -LiteralPath $zipPathFull) {
    Remove-Item -LiteralPath $zipPathFull -Force
}

New-Item -ItemType Directory -Path $releaseRootFull -Force | Out-Null

$selfContainedValue = if ($FrameworkDependent.IsPresent) { 'false' } else { 'true' }

$publishArguments = @(
    'publish',
    $cliProjectPath,
    '--configuration',
    'Release',
    '--runtime',
    $RuntimeIdentifier,
    '--self-contained',
    $selfContainedValue,
    '--output',
    $outputDirectoryFull,
    "-p:Version=$Version"
)

& dotnet @publishArguments

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE"
}

$executableName = if ($RuntimeIdentifier -like 'win-*') { 'mitmi-scan.exe' } else { 'mitmi-scan' }
$executablePath = Join-Path $outputDirectoryFull $executableName

if (-not (Test-Path -LiteralPath $executablePath)) {
    throw "Expected executable was not produced: $executablePath"
}

Copy-Item -LiteralPath (Join-Path $repoRoot 'README.md') -Destination $outputDirectoryFull
Copy-Item -LiteralPath (Join-Path $repoRoot 'LICENSE') -Destination $outputDirectoryFull
Copy-Item -LiteralPath (Join-Path $repoRoot 'THIRD_PARTY.md') -Destination $outputDirectoryFull

Compress-Archive -Path (Join-Path $outputDirectoryFull '*') -DestinationPath $zipPathFull -CompressionLevel Optimal

Write-Host "Published mitmi-scan to $outputDirectoryFull"
Write-Host "Run $executablePath --help"
Write-Host "Created release archive $zipPathFull"
