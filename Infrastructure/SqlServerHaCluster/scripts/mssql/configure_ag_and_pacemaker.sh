#!/usr/bin/env bash
set -euo pipefail

PM_PASSWORD="$(openssl rand -base64 36 | tr -d '\n')"
TEMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TEMP_DIR"; unset PM_PASSWORD' EXIT


echo "Preparing Pacemaker Login SQL Script..."

cat > "$TEMP_DIR/create-login.sql" <<SQL
USE [master];

IF SUSER_ID(N'pacemakerLogin') IS NULL
BEGIN
    CREATE LOGIN [pacemakerLogin] WITH PASSWORD = N'${PM_PASSWORD}';
END;

ALTER SERVER ROLE [sysadmin] ADD MEMBER [pacemakerLogin];
GO
SQL

echo "Pacemaker Login SQL Script created."

echo "--------------------------------------------------------------------"

echo "Preparing Pacemaker Credentials File..."

printf 'pacemakerLogin\n%s\n' "$PM_PASSWORD" > "$TEMP_DIR/passwd"

chmod 600 "$TEMP_DIR"/*

echo "Pacemaker Credentials File created."

echo "--------------------------------------------------------------------"

for node in sqlserver-1 sqlserver-2 sqlserver-3; do
    echo "Setting up Pacemaker Login on $node..."

    docker cp "$TEMP_DIR/create-login.sql" \
        "$node":/tmp/pacemaker-create-login.sql

    docker exec "$node" bash -c '
        source /etc/sqlserver-secrets/sqlserver.env

        /opt/mssql-tools18/bin/sqlcmd \
            -S localhost \
            -U sa \
            -P "$MSSQL_SA_PASSWORD" \
            -C \
            -d master \
            -b \
            -i /tmp/pacemaker-create-login.sql

        rm -f /tmp/pacemaker-create-login.sql
    '

    echo "Pacemaker SQL Login created on $node."

    docker cp "$TEMP_DIR/passwd" \
        "$node":/var/opt/mssql/secrets/passwd

    docker exec "$node" bash -c '
        chown root:root /var/opt/mssql/secrets/passwd
        chmod 400 /var/opt/mssql/secrets/passwd
        stat -c "%U:%G %a %n" /var/opt/mssql/secrets/passwd
    '

    echo "Pacemaker Credentials File configured on $node."
    echo "--------------------------------------------------------------------"
done

echo "Pacemaker login setup completed successfully."

echo "--------------------------------------------------------------------"

echo "Creating MedicalAG on sqlserver-1..."

docker exec -i sqlserver-1 bash -c '
source /etc/sqlserver-secrets/sqlserver.env

/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -d master \
    -b
' <<'SQL'

CREATE AVAILABILITY GROUP [MedicalAG]
WITH
(
    CLUSTER_TYPE = EXTERNAL
)
FOR DATABASE [MedicalDB]
REPLICA ON

N'sqlserver-1' WITH
(
    ENDPOINT_URL = N'TCP://172.32.0.10:5022',
    AVAILABILITY_MODE = SYNCHRONOUS_COMMIT,
    FAILOVER_MODE = EXTERNAL,
    SEEDING_MODE = MANUAL
),

N'sqlserver-2' WITH
(
    ENDPOINT_URL = N'TCP://172.32.0.11:5022',
    AVAILABILITY_MODE = SYNCHRONOUS_COMMIT,
    FAILOVER_MODE = EXTERNAL,
    SEEDING_MODE = MANUAL
),

N'sqlserver-3' WITH
(
    ENDPOINT_URL = N'TCP://172.32.0.12:5022',
    AVAILABILITY_MODE = SYNCHRONOUS_COMMIT,
    FAILOVER_MODE = EXTERNAL,
    SEEDING_MODE = MANUAL
);

GO

SQL

echo "MedicalAG created successfully on sqlserver-1."

echo "--------------------------------------------------------------------"

echo "Joining sqlserver-2 to MedicalAG..."

docker exec sqlserver-2 bash -c '
source /etc/sqlserver-secrets/sqlserver.env

/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -d master \
    -b \
    -Q "
        ALTER AVAILABILITY GROUP [MedicalAG]
        JOIN WITH (CLUSTER_TYPE = EXTERNAL);
    "
'

echo "sqlserver-2 joined MedicalAG successfully."

echo "--------------------------------------------------------------------"

echo "Adding MedicalDB to MedicalAG on sqlserver-2..."

docker exec sqlserver-2 bash -c '
source /etc/sqlserver-secrets/sqlserver.env

/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -d master \
    -b \
    -Q "
        ALTER DATABASE [MedicalDB]
        SET HADR AVAILABILITY GROUP = [MedicalAG];
    "
'

echo "MedicalDB added to MedicalAG on sqlserver-2 successfully."

echo "--------------------------------------------------------------------"

echo "Joining sqlserver-3 to MedicalAG..."

docker exec sqlserver-3 bash -c '
source /etc/sqlserver-secrets/sqlserver.env

/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -d master \
    -b \
    -Q "
        ALTER AVAILABILITY GROUP [MedicalAG]
        JOIN WITH (CLUSTER_TYPE = EXTERNAL);
    "
'

echo "sqlserver-3 joined MedicalAG successfully."

echo "--------------------------------------------------------------------"

echo "Adding MedicalDB to MedicalAG on sqlserver-3..."

docker exec sqlserver-3 bash -c '
source /etc/sqlserver-secrets/sqlserver.env

/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -d master \
    -b \
    -Q "
        ALTER DATABASE [MedicalDB]
        SET HADR AVAILABILITY GROUP = [MedicalAG];
    "
'

echo "MedicalDB added to MedicalAG on sqlserver-3 successfully."

echo "--------------------------------------------------------------------"

echo "Pacemaker Login and MedicalAG setup completed successfully."
