<#
.SYNOPSIS
    Enterprise Password Manager - Database Disaster Recovery Restore Script (PowerShell)
.DESCRIPTION
    Restores a PostgreSQL backup archive (.dump) into the target database.
.PARAMETER BackupFile
    Path to the backup file to restore.
.PARAMETER Force
    Skip interactive confirmation prompt.
#>
[CmdletBinding()]
param (
    [Parameter(Mandatory = $true, Position = 0)]
    [string]$BackupFile,

    [switch]$Force
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path -Path $BackupFile)) {
    Write-Error "Backup file not found: $BackupFile"
}

$ContainerName = if ($env:DB_CONTAINER) { $env:DB_CONTAINER } else { "passwordmanager-db" }
$DbName = if ($env:POSTGRES_DB) { $env:POSTGRES_DB } else { "passwordmanager_db" }
$DbUser = if ($env:POSTGRES_USER) { $env:POSTGRES_USER } else { "pm_user" }

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " Database Restore Target: $DbName" -ForegroundColor Cyan
Write-Host " Source Backup File:     $BackupFile" -ForegroundColor Cyan
Write-Host " Container Name:         $ContainerName" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

if (-not $Force) {
    $confirm = Read-Host "Are you sure you want to restore? This will overwrite existing database data! (yes/no)"
    if ($confirm -ne "yes") {
        Write-Host "Restore cancelled by user." -ForegroundColor Yellow
        exit 0
    }
}

$dockerRunning = docker ps --format '{{.Names}}' 2>$null | Select-String -Pattern "^${ContainerName}$"

if ($dockerRunning) {
    Write-Host "[+] Restoring via Docker container '$ContainerName'..." -ForegroundColor Green
    Get-Content -Path $BackupFile -AsByteStream | docker exec -i $ContainerName pg_restore -U $DbUser -d $DbName --clean --if-exists -v
} else {
    Write-Host "[!] Container '$ContainerName' not found running; attempting local pg_restore..." -ForegroundColor Yellow
    & pg_restore -U $DbUser -d $DbName --clean --if-exists -v $BackupFile
}

Write-Host "[+] Database restore completed successfully!" -ForegroundColor Green
