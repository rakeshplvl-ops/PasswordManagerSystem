<#
.SYNOPSIS
    Enterprise Password Manager - Automated PostgreSQL Backup Script (PowerShell)
.DESCRIPTION
    Creates a timestamped PostgreSQL backup from a running Docker container or local instance,
    and applies a retention policy to delete older archives.
.PARAMETER BackupDir
    Directory path to save the backup files. Default is '.\backups'.
.PARAMETER RetentionDays
    Number of days to keep backup files before rotating. Default is 7 days.
#>
[CmdletBinding()]
param (
    [string]$BackupDir = ".\backups",
    [int]$RetentionDays = 7
)

$ErrorActionPreference = "Stop"

$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$ContainerName = if ($env:DB_CONTAINER) { $env:DB_CONTAINER } else { "passwordmanager-db" }
$DbName = if ($env:POSTGRES_DB) { $env:POSTGRES_DB } else { "passwordmanager_db" }
$DbUser = if ($env:POSTGRES_USER) { $env:POSTGRES_USER } else { "pm_user" }

if (-not (Test-Path -Path $BackupDir)) {
    New-Item -ItemType Directory -Path $BackupDir -Force | Out-Null
}

$BackupFile = Join-Path -Path $BackupDir -ChildPath "passwordmanager_db_${Timestamp}.dump"

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host " Starting Database Backup: $DbName" -ForegroundColor Cyan
Write-Host " Timestamp:   $Timestamp" -ForegroundColor Cyan
Write-Host " Destination: $BackupFile" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# Check if Docker container is running
$dockerRunning = docker ps --format '{{.Names}}' 2>$null | Select-String -Pattern "^${ContainerName}$"

if ($dockerRunning) {
    Write-Host "[+] Running pg_dump inside Docker container '$ContainerName'..." -ForegroundColor Green
    docker exec $ContainerName pg_dump -U $DbUser -d $DbName -F c -b | Set-Content -Path $BackupFile -AsByteStream
} else {
    Write-Host "[!] Container '$ContainerName' not found running; attempting local pg_dump..." -ForegroundColor Yellow
    & pg_dump -U $DbUser -d $DbName -F c -b -f $BackupFile
}

if (Test-Path -Path $BackupFile) {
    $fileSize = (Get-Item -Path $BackupFile).Length / 1MB
    Write-Host ("[+] Backup completed successfully! Size: {0:N2} MB" -f $fileSize) -ForegroundColor Green
} else {
    Write-Error "Backup file was not created."
}

# Rotate old backups
Write-Host "[+] Purging backups older than $RetentionDays days..." -ForegroundColor Gray
$cutoffDate = (Get-Date).AddDays(-$RetentionDays)
Get-ChildItem -Path $BackupDir -Filter "passwordmanager_db_*.dump" |
    Where-Object { $_.LastWriteTime -lt $cutoffDate } |
    ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-Host "[-] Removed expired backup: $($_.Name)" -ForegroundColor DarkGray
    }

Write-Host "[+] Backup routine complete." -ForegroundColor Cyan
