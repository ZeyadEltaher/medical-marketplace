#!/usr/bin/env bash
set -euo pipefail

echo "Creating Remote SQL Logins and Users on sqlserver-1..."

docker exec sqlserver-1 bash -c '
source /etc/sqlserver-secrets/sqlserver.env

/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -d master \
    -b \
    -s "|" \
    -W \
    -w 200 \
    -Q "
        IF NOT EXISTS (
            SELECT 1
            FROM sys.server_principals
            WHERE name = N'\''sqlserver2_ag_login'\''
        )
        BEGIN
            CREATE LOGIN sqlserver2_ag_login
            WITH PASSWORD = '\''SqlServer2Login'\'';
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.database_principals
            WHERE name = N'\''sqlserver2_ag_user'\''
        )
        BEGIN
            CREATE USER sqlserver2_ag_user
            FOR LOGIN sqlserver2_ag_login;
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.server_principals
            WHERE name = N'\''sqlserver3_ag_login'\''
        )
        BEGIN
            CREATE LOGIN sqlserver3_ag_login
            WITH PASSWORD = '\''SqlServer3Login'\'';
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.database_principals
            WHERE name = N'\''sqlserver3_ag_user'\''
        )
        BEGIN
            CREATE USER sqlserver3_ag_user
            FOR LOGIN sqlserver3_ag_login;
        END;
    "
'

echo "Remote SQL Logins and Users configured on sqlserver-1 successfully."

echo "--------------------------------------------------------------------"

echo "Creating Remote SQL Logins and Users on sqlserver-2..."

docker exec sqlserver-2 bash -c '
source /etc/sqlserver-secrets/sqlserver.env

/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -d master \
    -b \
    -s "|" \
    -W \
    -w 200 \
    -Q "
        IF NOT EXISTS (
            SELECT 1
            FROM sys.server_principals
            WHERE name = N'\''sqlserver1_ag_login'\''
        )
        BEGIN
            CREATE LOGIN sqlserver1_ag_login
            WITH PASSWORD = '\''SqlServer1Login'\'';
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.database_principals
            WHERE name = N'\''sqlserver1_ag_user'\''
        )
        BEGIN
            CREATE USER sqlserver1_ag_user
            FOR LOGIN sqlserver1_ag_login;
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.server_principals
            WHERE name = N'\''sqlserver3_ag_login'\''
        )
        BEGIN
            CREATE LOGIN sqlserver3_ag_login
            WITH PASSWORD = '\''SqlServer3Login'\'';
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.database_principals
            WHERE name = N'\''sqlserver3_ag_user'\''
        )
        BEGIN
            CREATE USER sqlserver3_ag_user
            FOR LOGIN sqlserver3_ag_login;
        END;
    "
'

echo "Remote SQL Logins and Users configured on sqlserver-2 successfully."

echo "--------------------------------------------------------------------"

echo "Creating Remote SQL Logins and Users on sqlserver-3..."

docker exec sqlserver-3 bash -c '
source /etc/sqlserver-secrets/sqlserver.env

/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -d master \
    -b \
    -s "|" \
    -W \
    -w 200 \
    -Q "
        IF NOT EXISTS (
            SELECT 1
            FROM sys.server_principals
            WHERE name = N'\''sqlserver1_ag_login'\''
        )
        BEGIN
            CREATE LOGIN sqlserver1_ag_login
            WITH PASSWORD = '\''SqlServer1Login'\'';
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.database_principals
            WHERE name = N'\''sqlserver1_ag_user'\''
        )
        BEGIN
            CREATE USER sqlserver1_ag_user
            FOR LOGIN sqlserver1_ag_login;
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.server_principals
            WHERE name = N'\''sqlserver2_ag_login'\''
        )
        BEGIN
            CREATE LOGIN sqlserver2_ag_login
            WITH PASSWORD = '\''SqlServer2Login'\'';
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.database_principals
            WHERE name = N'\''sqlserver2_ag_user'\''
        )
        BEGIN
            CREATE USER sqlserver2_ag_user
            FOR LOGIN sqlserver2_ag_login;
        END;
    "
'

echo "Remote SQL Logins and Users configured on sqlserver-3 successfully."

echo "--------------------------------------------------------------------"

echo "Importing Remote AG Certificates on sqlserver-1..."

docker exec sqlserver-1 bash -c '
source /etc/sqlserver-secrets/sqlserver.env

/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -d master \
    -b \
    -s "|" \
    -W \
    -w 200 \
    -Q "
        IF NOT EXISTS (
            SELECT 1
            FROM sys.certificates
            WHERE name = N'\''sqlserver2_ag_cert'\''
        )
        BEGIN
            CREATE CERTIFICATE sqlserver2_ag_cert
            AUTHORIZATION sqlserver2_ag_user
            FROM FILE = '\''/var/opt/mssql/data/sqlserver2_ag_cert.cer'\'';
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.certificates
            WHERE name = N'\''sqlserver3_ag_cert'\''
        )
        BEGIN
            CREATE CERTIFICATE sqlserver3_ag_cert
            AUTHORIZATION sqlserver3_ag_user
            FROM FILE = '\''/var/opt/mssql/data/sqlserver3_ag_cert.cer'\'';
        END;
    "
'

echo "Remote AG Certificates configured on sqlserver-1 successfully."

echo "--------------------------------------------------------------------"

echo "Importing Remote AG Certificates on sqlserver-2..."

docker exec sqlserver-2 bash -c '
source /etc/sqlserver-secrets/sqlserver.env

/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -d master \
    -b \
    -s "|" \
    -W \
    -w 200 \
    -Q "
        IF NOT EXISTS (
            SELECT 1
            FROM sys.certificates
            WHERE name = N'\''sqlserver1_ag_cert'\''
        )
        BEGIN
            CREATE CERTIFICATE sqlserver1_ag_cert
            AUTHORIZATION sqlserver1_ag_user
            FROM FILE = '\''/var/opt/mssql/data/sqlserver1_ag_cert.cer'\'';
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.certificates
            WHERE name = N'\''sqlserver3_ag_cert'\''
        )
        BEGIN
            CREATE CERTIFICATE sqlserver3_ag_cert
            AUTHORIZATION sqlserver3_ag_user
            FROM FILE = '\''/var/opt/mssql/data/sqlserver3_ag_cert.cer'\'';
        END;
    "
'

echo "Remote AG Certificates configured on sqlserver-2 successfully."

echo "--------------------------------------------------------------------"

echo "Importing Remote AG Certificates on sqlserver-3..."

docker exec sqlserver-3 bash -c '
source /etc/sqlserver-secrets/sqlserver.env

/opt/mssql-tools18/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -d master \
    -b \
    -s "|" \
    -W \
    -w 200 \
    -Q "
        IF NOT EXISTS (
            SELECT 1
            FROM sys.certificates
            WHERE name = N'\''sqlserver1_ag_cert'\''
        )
        BEGIN
            CREATE CERTIFICATE sqlserver1_ag_cert
            AUTHORIZATION sqlserver1_ag_user
            FROM FILE = '\''/var/opt/mssql/data/sqlserver1_ag_cert.cer'\'';
        END;

        IF NOT EXISTS (
            SELECT 1
            FROM sys.certificates
            WHERE name = N'\''sqlserver2_ag_cert'\''
        )
        BEGIN
            CREATE CERTIFICATE sqlserver2_ag_cert
            AUTHORIZATION sqlserver2_ag_user
            FROM FILE = '\''/var/opt/mssql/data/sqlserver2_ag_cert.cer'\'';
        END;
    "
'

echo "Remote AG Certificates configured on sqlserver-3 successfully."

echo "--------------------------------------------------------------------"

echo "AG Certificate exchange and configuration completed successfully."