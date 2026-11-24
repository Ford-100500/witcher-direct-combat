$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $csc)) { throw 'Не найден csc.exe из .NET Framework 4.' }

$outDir = Join-Path $root 'dist'
New-Item -ItemType Directory -Path $outDir -Force | Out-Null
$target = Join-Path $outDir 'WitcherDirectCombatSetup.exe'

& $csc /nologo /codepage:65001 /target:winexe /platform:x64 "/out:$target" `
    /r:System.Windows.Forms.dll /r:System.Drawing.dll `
    "/resource:$root\Payload\DirectCombatController.exe,Controller.exe" `
    "/resource:$root\Payload\Play.cmd,Play.cmd" `
    "/resource:$root\Payload\testtools.lua,testtools.lua" `
    "$PSScriptRoot\Setup.cs"

if ($LASTEXITCODE -ne 0) { throw "Сборка установщика завершилась с кодом $LASTEXITCODE" }
Write-Host "Собрано: $target"
