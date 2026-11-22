#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
MSSQL_SCRIPTS_DIR="$(cd -- "$SCRIPT_DIR/../mssql" && pwd)"

echo "======================================================================================================"
echo "🚀 Starting SQL Server HA Cluster Setup..."
echo "======================================================================================================"

if [ ! -d "$MSSQL_SCRIPTS_DIR" ]; then
    echo "Error: Directory '$MSSQL_SCRIPTS_DIR' not found!"
    exit 1
fi

echo "======================================================================="
echo " [1/9] Enabling HADR..."
echo "======================================================================="
bash "$MSSQL_SCRIPTS_DIR/enable_hadr.sh"

echo "======================================================================="
echo " [2/9] Creating Master Keys and Certificates..."
echo "======================================================================="
bash "$MSSQL_SCRIPTS_DIR/create_certificates.sh"

echo "======================================================================="
echo " [3/9] Exchanging Certificates and Configuring Logins/Users..."
echo "======================================================================="
bash "$MSSQL_SCRIPTS_DIR/exchange_certs_and_logins.sh"

echo "======================================================================="
echo " [4/9] Creating Mirroring Endpoints..."
echo "======================================================================="
bash "$MSSQL_SCRIPTS_DIR/create_endpoints.sh"

echo "======================================================================="
echo " [5/9] Backing Up and Restoring Database..."
echo "======================================================================="
bash "$MSSQL_SCRIPTS_DIR/restore_database_backup.sh"

echo "======================================================================="
echo " [6/9] Configuring Pacemaker and Joining Availability Group..."
echo "======================================================================="
bash "$MSSQL_SCRIPTS_DIR/configure_ag_and_pacemaker.sh"

echo "======================================================================="
echo " [7/9] Integrating SQL Server AG with Pacemaker..."
echo "======================================================================="
bash "$MSSQL_SCRIPTS_DIR/integrate_resource_agent.sh"

echo "======================================================================="
echo " [8/9] Creating SQL Server VIP Resource & AG Listener..."
echo "======================================================================="
bash "$MSSQL_SCRIPTS_DIR/create_ag_listener.sh"

echo "======================================================================="
echo " [9/9] Migrating SQL Server Database by ASP.NET Local App..."
echo "======================================================================="
bash "$MSSQL_SCRIPTS_DIR/migrate_database.sh"

echo "======================================================================="
echo " ✅ SQL Server HA Cluster Setup Completed Successfully!"
echo "======================================================================="
