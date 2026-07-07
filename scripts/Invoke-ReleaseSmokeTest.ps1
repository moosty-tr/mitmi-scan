Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$solutionPath = Join-Path $repoRoot 'Mitmi.Scan.slnx'
$cliProjectPath = Join-Path $repoRoot 'src\Mitmi.Scan.Cli\Mitmi.Scan.Cli.csproj'

function Invoke-Checked {
    param(
        [Parameter(Mandatory = $true)]
        [string] $FilePath,

        [Parameter(Mandatory = $true)]
        [string[]] $Arguments
    )

    & $FilePath @Arguments

    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code $LASTEXITCODE`: $FilePath $($Arguments -join ' ')"
    }
}

Push-Location $repoRoot
try {
    Invoke-Checked 'dotnet' @('build', $solutionPath, '--configuration', 'Release')
    Invoke-Checked 'dotnet' @('test', $solutionPath, '--configuration', 'Release', '--no-build')
    Invoke-Checked 'dotnet' @('run', '--project', $cliProjectPath, '--configuration', 'Release', '--no-build', '--', '--help')
}
finally {
    Pop-Location
}
