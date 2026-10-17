#!/usr/bin/env bash
# ==============================================================================
# Enterprise Password Manager - Automated PostgreSQL Backup Script
# Usage: ./scripts/backup-db.sh [backup_dir]
# ==============================================================================
set -euo pipefail

BACKUP_DIR="${1:-./backups}"
TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="${BACKUP_DIR}/passwordmanager_db_${TIMESTAMP}.dump"
CONTAINER_NAME="${DB_CONTAINER:-passwordmanager-db}"
DB_NAME="${POSTGRES_DB:-passwordmanager_db}"
DB_USER="${POSTGRES_USER:-pm_user}"
RETENTION_DAYS="${BACKUP_RETENTION_DAYS:-7}"

mkdir -p "${BACKUP_DIR}"

echo "============================================================"
echo " Starting Database Backup: ${DB_NAME}"
echo " Timestamp:   ${TIMESTAMP}"
echo " Destination: ${BACKUP_FILE}"
echo "============================================================"

# Check if docker container is running
if docker ps --format '{{.Names}}' | grep -Eq "^${CONTAINER_NAME}\$"; then
    echo "[+] Running pg_dump inside Docker container '${CONTAINER_NAME}'..."
    docker exec -t "${CONTAINER_NAME}" pg_dump -U "${DB_USER}" -d "${DB_NAME}" -F c -b -v > "${BACKUP_FILE}"
else
    echo "[!] Container '${CONTAINER_NAME}' not running; attempting local pg_dump..."
    pg_dump -U "${DB_USER}" -d "${DB_NAME}" -F c -b -v -f "${BACKUP_FILE}"
fi

BACKUP_SIZE=$(du -h "${BACKUP_FILE}" | cut -f1)
echo "[+] Backup successfully completed! File size: ${BACKUP_SIZE}"

# Rotate backups older than retention window
echo "[+] Purging backups older than ${RETENTION_DAYS} days..."
find "${BACKUP_DIR}" -name "passwordmanager_db_*.dump" -mtime +"${RETENTION_DAYS}" -exec rm -f {} \;

echo "[+] Done. Secure your backup file."
