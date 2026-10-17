#!/usr/bin/env bash
# ==============================================================================
# Enterprise Password Manager - Database Disaster Recovery Restore Script
# Usage: ./scripts/restore-db.sh <path_to_backup_file> [--force]
# ==============================================================================
set -euo pipefail

BACKUP_FILE="${1:-}"
FORCE="${2:-}"
CONTAINER_NAME="${DB_CONTAINER:-passwordmanager-db}"
DB_NAME="${POSTGRES_DB:-passwordmanager_db}"
DB_USER="${POSTGRES_USER:-pm_user}"

if [ -z "${BACKUP_FILE}" ] || [ ! -f "${BACKUP_FILE}" ]; then
    echo "[-] Error: Please specify a valid backup file to restore."
    echo "    Usage: ./scripts/restore-db.sh <path_to_backup_file.dump> [--force]"
    exit 1
fi

echo "============================================================"
echo " Database Restore Target: ${DB_NAME}"
echo " Source Backup File:     ${BACKUP_FILE}"
echo " Container Name:         ${CONTAINER_NAME}"
echo "============================================================"

if [ "${FORCE}" != "--force" ]; then
    read -rp "Are you sure you want to restore? This will overwrite existing database data! (yes/no): " CONFIRM
    if [ "${CONFIRM}" != "yes" ]; then
        echo "Restore cancelled by user."
        exit 0
    fi
fi

if docker ps --format '{{.Names}}' | grep -Eq "^${CONTAINER_NAME}\$"; then
    echo "[+] Restoring via Docker container '${CONTAINER_NAME}'..."
    cat "${BACKUP_FILE}" | docker exec -i "${CONTAINER_NAME}" pg_restore -U "${DB_USER}" -d "${DB_NAME}" --clean --if-exists -v
else
    echo "[!] Container '${CONTAINER_NAME}' not running; attempting local pg_restore..."
    pg_restore -U "${DB_USER}" -d "${DB_NAME}" --clean --if-exists -v "${BACKUP_FILE}"
fi

echo "[+] Database restore completed successfully!"
