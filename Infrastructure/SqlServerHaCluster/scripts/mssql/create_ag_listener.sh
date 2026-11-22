#!/usr/bin/env bash
set -euo pipefail

SQL_NODE="sqlserver-1"
FALLBACK_PRIMARY="sqlserver-1"

VIP="172.32.0.100"
NETMASK="255.255.255.0"
CIDR="24"

AG_NAME="MedicalAG"
LISTENER_NAME="medical-ag-listener"
LISTENER_PORT="1433"
CLONE_NAME="MedicalAG-clone"
VIP_RESOURCE="sql_vip"

echo "Creating SQL Server VIP Resource..."

docker exec "$SQL_NODE" bash -c '
pcs resource create sql_vip ocf:heartbeat:IPaddr2 \
    ip=172.32.0.100 \
    cidr_netmask=24 \
    op monitor interval=10s
'

echo "SQL Server VIP Resource created successfully."

echo "--------------------------------------------------------------------"

echo "Detecting current PRIMARY replica for $AG_NAME..."

SQL_PRIMARY=$(docker exec "$SQL_NODE" bash -c '
    set -a
    source /etc/sqlserver-secrets/sqlserver.env
    set +a

    /opt/mssql-tools18/bin/sqlcmd \
        -S localhost \
        -U sa \
        -P "$MSSQL_SA_PASSWORD" \
        -C \
        -d master \
        -h -1 -W \
        -Q "SET NOCOUNT ON;
            SELECT TOP 1 ar.replica_server_name
            FROM sys.availability_groups ag
            JOIN sys.availability_replicas ar
                ON ag.group_id = ar.group_id
            JOIN sys.dm_hadr_availability_replica_states ars
                ON ar.replica_id = ars.replica_id
            WHERE ag.name = '\''MedicalAG'\''
              AND ars.role_desc = '\''PRIMARY'\'';"
' 2>/dev/null | tr -d '[:space:]' || true)

SQL_PRIMARY="${SQL_PRIMARY:-$FALLBACK_PRIMARY}"
echo "Target PRIMARY replica: $SQL_PRIMARY"

echo "--------------------------------------------------------------------"

echo "Creating MedicalAG Listener..."

docker exec -i "$SQL_PRIMARY" bash -c '
    set -a
    source /etc/sqlserver-secrets/sqlserver.env
    set +a

    /opt/mssql-tools18/bin/sqlcmd \
        -S localhost \
        -U sa \
        -P "$MSSQL_SA_PASSWORD" \
        -C \
        -d master \
        -b
' <<'SQL'

ALTER AVAILABILITY GROUP [MedicalAG]
ADD LISTENER N'medical-ag-listener' (
    WITH IP (
        (N'172.32.0.100', N'255.255.255.0')
    ),
    PORT=1433
);

GO

SQL

echo "MedicalAG Listener created successfully."

echo "--------------------------------------------------------------------"

echo "Adding VIP Colocation Constraint..."

docker exec "$SQL_NODE" bash -c '
pcs constraint colocation add \
    sql_vip with Promoted MedicalAG-clone INFINITY
'

echo "VIP Colocation Constraint added successfully."

echo "--------------------------------------------------------------------"

echo "Adding VIP Ordering Constraint..."

docker exec "$SQL_NODE" bash -c '
pcs constraint order \
    promote MedicalAG-clone then start sql_vip
'

echo "VIP Ordering Constraint added successfully."

echo "--------------------------------------------------------------------"

echo "Verifying MedicalAG Configuration..."

docker exec "$SQL_NODE" bash -c '
echo "========== CONSTRAINTS =========="
pcs constraint config

echo
echo "========== CLUSTER STATUS =========="
pcs status --full
'

echo "MedicalAG Configuration verified."

echo "--------------------------------------------------------------------"

echo "MedicalAG VIP and Listener setup completed successfully."