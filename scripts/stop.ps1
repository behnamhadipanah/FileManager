#Requires -Version 5.1

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$RunDir = Join-Path $RepoRoot ".run"
$PidFile = Join-Path $RunDir "api.pid"
$Port = 5066
$ProjectMarker = "FileManager.Api"

function Get-ListenerProcessId {
    param([int]$ListenPort)

    $connection = Get-NetTCPConnection -LocalPort $ListenPort -State Listen -ErrorAction SilentlyContinue |
        Select-Object -First 1

    if ($null -eq $connection) {
        return $null
    }

    return $connection.OwningProcess
}

function Stop-ProcessTree {
    param([int]$ProcessId)

    if ($ProcessId -le 0) {
        return $false
    }

    $children = Get-CimInstance Win32_Process |
        Where-Object { $_.ParentProcessId -eq $ProcessId } |
        Select-Object -ExpandProperty ProcessId

    foreach ($childId in $children) {
        Stop-ProcessTree -ProcessId $childId
    }

    $process = Get-Process -Id $ProcessId -ErrorAction SilentlyContinue
    if ($null -eq $process) {
        return $false
    }

    Stop-Process -Id $ProcessId -Force
    return $true
}

$stopped = $false
$candidatePids = New-Object System.Collections.Generic.List[int]

if (Test-Path $PidFile) {
    $savedPid = [int](Get-Content $PidFile -Raw).Trim()
    if ($savedPid -gt 0) {
        $candidatePids.Add($savedPid) | Out-Null
    }
}

$listenerPid = Get-ListenerProcessId -ListenPort $Port
if ($null -ne $listenerPid) {
    $candidatePids.Add($listenerPid) | Out-Null
}

Get-CimInstance Win32_Process -Filter "Name = 'FileManager.Api.exe'" |
    Where-Object { $_.CommandLine -like "*$ProjectMarker*" } |
    ForEach-Object { $candidatePids.Add($_.ProcessId) | Out-Null }

Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
    Where-Object { $_.CommandLine -like "*$ProjectMarker*" } |
    ForEach-Object { $candidatePids.Add($_.ProcessId) | Out-Null }

foreach ($processId in ($candidatePids | Select-Object -Unique)) {
    if (Stop-ProcessTree -ProcessId $processId) {
        $stopped = $true
        Write-Host "Stopped process $processId."
    }
}

if (Test-Path $PidFile) {
    Remove-Item $PidFile -Force
}

if (-not $stopped) {
    Write-Host "FileManager API is not running."
    exit 0
}

Write-Host "FileManager API stopped."
