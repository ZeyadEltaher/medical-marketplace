#!/usr/bin/env bash
set -euo pipefail

echo "Creating MedicalDB on sqlserver-1..."

docker exec sqlserver-1 bash -c '
source /etc/sqlserver-secrets/sqlserver.env

/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -d master \
    -b \
    -s "|" -W -w 200 \
    -Q "
        IF DB_ID(N'\''MedicalDB'\'') IS NULL
        BEGIN
            CREATE DATABASE [MedicalDB];
        END;

        SELECT
            name,
            state_desc,
            recovery_model_desc
        FROM sys.databases
        WHERE name = N'\''MedicalDB'\'';
    "
'

echo "MedicalDB created successfully on sqlserver-1."

echo "--------------------------------------------------------------------"

echo "Creating full backup of MedicalDB on sqlserver-1..."

docker exec sqlserver-1 bash -c '
source /etc/sqlserver-secrets/sqlserver.env

/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -d master \
    -b \
    -Q "
        BACKUP DATABASE [MedicalDB]
        TO DISK = N'\''/var/opt/mssql/data/MedicalDB_full.bak'\''
        WITH
            INIT,
            COMPRESSION,
            STATS = 10;
    "
'

echo "Full backup created successfully on sqlserver-1."

echo "--------------------------------------------------------------------"

echo "Creating transaction log backup of MedicalDB on sqlserver-1..."

docker exec sqlserver-1 bash -c '
source /etc/sqlserver-secrets/sqlserver.env

/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -d master \
    -b \
    -Q "
        BACKUP LOG [MedicalDB]
        TO DISK = N'\''/var/opt/mssql/data/MedicalDB_log.trn'\''
        WITH
            INIT,
            COMPRESSION,
            STATS = 10;
    "
'

echo "Transaction log backup created successfully on sqlserver-1."

echo "--------------------------------------------------------------------"

echo "Copying backup files from sqlserver-1 to the host..."

mkdir -p /tmp/medical-ag-backups

docker cp \
    sqlserver-1:/var/opt/mssql/data/MedicalDB_full.bak \
    /tmp/medical-ag-backups/

docker cp \
    sqlserver-1:/var/opt/mssql/data/MedicalDB_log.trn \
    /tmp/medical-ag-backups/

ls -lh /tmp/medical-ag-backups/

echo "Backup files copied successfully to the host."

echo "--------------------------------------------------------------------"

for node in sqlserver-2 sqlserver-3; do
    echo "Copying backup files to $node..."

    docker cp \
        /tmp/medical-ag-backups/MedicalDB_full.bak \
        "$node":/var/opt/mssql/data/

    docker cp \
        /tmp/medical-ag-backups/MedicalDB_log.trn \
        "$node":/var/opt/mssql/data/

    docker exec "$node" chown mssql:mssql \
        /var/opt/mssql/data/MedicalDB_full.bak \
        /var/opt/mssql/data/MedicalDB_log.trn

    docker exec "$node" chmod 640 \
        /var/opt/mssql/data/MedicalDB_full.bak \
        /var/opt/mssql/data/MedicalDB_log.trn

    docker exec "$node" ls -lh \
        /var/opt/mssql/data/MedicalDB_full.bak \
        /var/opt/mssql/data/MedicalDB_log.trn

    echo "Backup files copied and permissions configured successfully on $node."

    echo "--------------------------------------------------------------------"
done

echo "Backup files distributed successfully to all secondary nodes."

echo "--------------------------------------------------------------------"

echo "Restoring MedicalDB full backup on sqlserver-2..."

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
        RESTORE DATABASE [MedicalDB]
        FROM DISK = N'\''/var/opt/mssql/data/MedicalDB_full.bak'\''
        WITH
            MOVE N'\''MedicalDB'\''
                TO N'\''/var/opt/mssql/data/MedicalDB.mdf'\'',
            MOVE N'\''MedicalDB_log'\''
                TO N'\''/var/opt/mssql/data/MedicalDB_log.ldf'\'',
            NORECOVERY,
            STATS = 10;
    "
'

echo "MedicalDB full backup restored successfully on sqlserver-2."

echo "--------------------------------------------------------------------"

echo "Restoring MedicalDB transaction log backup on sqlserver-2..."

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
        RESTORE LOG [MedicalDB]
        FROM DISK = N'\''/var/opt/mssql/data/MedicalDB_log.trn'\''
        WITH NORECOVERY, STATS = 10;
    "
'

echo "MedicalDB transaction log backup restored successfully on sqlserver-2."

echo "--------------------------------------------------------------------"

echo "Restoring MedicalDB full backup on sqlserver-3..."

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
        RESTORE DATABASE [MedicalDB]
        FROM DISK = N'\''/var/opt/mssql/data/MedicalDB_full.bak'\''
        WITH
            MOVE N'\''MedicalDB'\''
                TO N'\''/var/opt/mssql/data/MedicalDB.mdf'\'',
            MOVE N'\''MedicalDB_log'\''
                TO N'\''/var/opt/mssql/data/MedicalDB_log.ldf'\'',
            NORECOVERY,
            STATS = 10;
    "
'

echo "MedicalDB full backup restored successfully on sqlserver-3."

echo "--------------------------------------------------------------------"

echo "Restoring MedicalDB transaction log backup on sqlserver-3..."

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
        RESTORE LOG [MedicalDB]
        FROM DISK = N'\''/var/opt/mssql/data/MedicalDB_log.trn'\''
        WITH NORECOVERY, STATS = 10;
    "
'

echo "MedicalDB transaction log backup restored successfully on sqlserver-3."

echo "--------------------------------------------------------------------"

echo "MedicalDB backup and restore process completed successfully."
