#Requires -Version 5.1

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
$ProjectPath = Join-Path $RepoRoot "src\FileManager.Api\FileManager.Api.csproj"
$RunDir = Join-Path $RepoRoot ".run"
$PidFile = Join-Path $RunDir "api.pid"
$LogFile = Join-Path $RunDir "api.log"
$Port = 5066
$Url = "http://localhost:$Port"

if (-not (Test-Path $ProjectPath)) {
    Write-Error "Project not found: $ProjectPath"
}

function Get-ListenerProcessId {
    param([int]$ListenPort)

    $connection = Get-NetTCPConnection -LocalPort $ListenPort -State Listen -ErrorAction SilentlyContinue |
        Select-Object -First 1

    if ($null -eq $connection) {
        return $null
    }

    return $connection.OwningProcess
}

function Test-ApiRunning {
    $listenerPid = Get-ListenerProcessId -ListenPort $Port
    return $null -ne $listenerPid
}

if (Test-ApiRunning) {
    $existingPid = Get-ListenerProcessId -ListenPort $Port
    Set-Content -Path $PidFile -Value $existingPid -NoNewline
    Write-Host "FileManager API is already running."
    Write-Host "  PID: $existingPid"
    Write-Host "  URL: $Url"
    exit 0
}

New-Item -ItemType Directory -Path $RunDir -Force | Out-Null

Write-Host "Starting FileManager API..."

$process = Start-Process `
    -FilePath "cmd.exe" `
    -ArgumentList @("/c", "dotnet run --project `"$ProjectPath`" > `"$LogFile`" 2>&1") `
    -WorkingDirectory $RepoRoot `
    -PassThru `
    -WindowStyle Hidden

$deadline = (Get-Date).AddSeconds(60)
$listenerPid = $null

while ((Get-Date) -lt $deadline) {
    $listenerPid = Get-ListenerProcessId -ListenPort $Port
    if ($null -ne $listenerPid) {
        break
    }

    if ($process.HasExited) {
        Write-Error "FileManager API failed to start. See log: $LogFile"
    }

    Start-Sleep -Milliseconds 500
}

if ($null -eq $listenerPid) {
    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    Write-Error "Timed out waiting for FileManager API on port $Port. See log: $LogFile"
}

Set-Content -Path $PidFile -Value $listenerPid -NoNewline

Write-Host "FileManager API started."
Write-Host "  PID: $listenerPid"
Write-Host "  URL: $Url"
Write-Host "  Scalar: $Url/scalar/v1"
Write-Host "  Log: $LogFile"
