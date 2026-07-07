param(
    [string] $RuntimeIdentifier = 'win-x64',
    [switch] $SelfContained,
    [string] $OutputRoot
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$cliProjectPath = Join-Path $repoRoot 'src\Mitmi.Scan.Cli\Mitmi.Scan.Cli.csproj'

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot 'artifacts\publish'
}

$repoRootFull = [System.IO.Path]::GetFullPath($repoRoot)
$outputRootFull = [System.IO.Path]::GetFullPath($OutputRoot)

if (-not $outputRootFull.StartsWith($repoRootFull, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Output root must stay inside the repository: $outputRootFull"
}

$outputDirectory = Join-Path $outputRootFull "mitmi-scan-$RuntimeIdentifier"
$outputDirectoryFull = [System.IO.Path]::GetFullPath($outputDirectory)

if (-not $outputDirectoryFull.StartsWith($outputRootFull, [System.StringComparison]::OrdinalIgnoreCase)) {
    throw "Output directory must stay inside the publish output root: $outputDirectoryFull"
}

if (Test-Path -LiteralPath $outputDirectoryFull) {
    Remove-Item -LiteralPath $outputDirectoryFull -Recurse -Force
}

$selfContainedValue = if ($SelfContained.IsPresent) { 'true' } else { 'false' }

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
    $outputDirectoryFull
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

Write-Host "Published mitmi-scan to $outputDirectoryFull"
Write-Host "Run $executablePath --help"
