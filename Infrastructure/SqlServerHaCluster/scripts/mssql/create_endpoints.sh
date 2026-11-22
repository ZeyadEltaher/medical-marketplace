
#!/usr/bin/env bash
set -euo pipefail

echo "Creating AG Endpoint and Granting Permissions on sqlserver-1..."

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
        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.endpoints
            WHERE name = N'\''AG_endpoint'\''
        )
        BEGIN
            CREATE ENDPOINT [AG_endpoint]
            STATE = STARTED
            AS TCP
            (
                LISTENER_PORT = 5022,
                LISTENER_IP = ALL
            )
            FOR DATABASE_MIRRORING
            (
                AUTHENTICATION = CERTIFICATE sqlserver1_ag_cert,
                ENCRYPTION = REQUIRED ALGORITHM AES,
                ROLE = ALL
            );
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.server_permissions p
            INNER JOIN sys.endpoints e
                ON p.major_id = e.endpoint_id
            INNER JOIN sys.server_principals sp
                ON p.grantee_principal_id = sp.principal_id
            WHERE e.name = N'\''AG_endpoint'\''
              AND sp.name = N'\''sqlserver2_ag_login'\''
              AND p.permission_name = N'\''CONNECT'\''
              AND p.state IN (N'\''G'\'', N'\''W'\'')
        )
        BEGIN
            GRANT CONNECT ON ENDPOINT::[AG_endpoint]
            TO [sqlserver2_ag_login];
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.server_permissions p
            INNER JOIN sys.endpoints e
                ON p.major_id = e.endpoint_id
            INNER JOIN sys.server_principals sp
                ON p.grantee_principal_id = sp.principal_id
            WHERE e.name = N'\''AG_endpoint'\''
              AND sp.name = N'\''sqlserver3_ag_login'\''
              AND p.permission_name = N'\''CONNECT'\''
              AND p.state IN (N'\''G'\'', N'\''W'\'')
        )
        BEGIN
            GRANT CONNECT ON ENDPOINT::[AG_endpoint]
            TO [sqlserver3_ag_login];
        END;
    "
'

echo "AG Endpoint created and permissions granted on sqlserver-1 successfully."

echo "--------------------------------------------------------------------"

echo "Creating AG Endpoint and Granting Permissions on sqlserver-2..."

docker exec sqlserver-2 bash -c '
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
        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.endpoints
            WHERE name = N'\''AG_endpoint'\''
        )
        BEGIN
            CREATE ENDPOINT [AG_endpoint]
            STATE = STARTED
            AS TCP
            (
                LISTENER_PORT = 5022,
                LISTENER_IP = ALL
            )
            FOR DATABASE_MIRRORING
            (
                AUTHENTICATION = CERTIFICATE sqlserver2_ag_cert,
                ENCRYPTION = REQUIRED ALGORITHM AES,
                ROLE = ALL
            );
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.server_permissions p
            INNER JOIN sys.endpoints e
                ON p.major_id = e.endpoint_id
            INNER JOIN sys.server_principals sp
                ON p.grantee_principal_id = sp.principal_id
            WHERE e.name = N'\''AG_endpoint'\''
              AND sp.name = N'\''sqlserver1_ag_login'\''
              AND p.permission_name = N'\''CONNECT'\''
              AND p.state IN (N'\''G'\'', N'\''W'\'')
        )
        BEGIN
            GRANT CONNECT ON ENDPOINT::[AG_endpoint]
            TO [sqlserver1_ag_login];
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.server_permissions p
            INNER JOIN sys.endpoints e
                ON p.major_id = e.endpoint_id
            INNER JOIN sys.server_principals sp
                ON p.grantee_principal_id = sp.principal_id
            WHERE e.name = N'\''AG_endpoint'\''
              AND sp.name = N'\''sqlserver3_ag_login'\''
              AND p.permission_name = N'\''CONNECT'\''
              AND p.state IN (N'\''G'\'', N'\''W'\'')
        )
        BEGIN
            GRANT CONNECT ON ENDPOINT::[AG_endpoint]
            TO [sqlserver3_ag_login];
        END;
    "
'

echo "AG Endpoint created and permissions granted on sqlserver-2 successfully."

echo "--------------------------------------------------------------------"

echo "Creating AG Endpoint and Granting Permissions on sqlserver-3..."

docker exec sqlserver-3 bash -c '
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
        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.endpoints
            WHERE name = N'\''AG_endpoint'\''
        )
        BEGIN
            CREATE ENDPOINT [AG_endpoint]
            STATE = STARTED
            AS TCP
            (
                LISTENER_PORT = 5022,
                LISTENER_IP = ALL
            )
            FOR DATABASE_MIRRORING
            (
                AUTHENTICATION = CERTIFICATE sqlserver3_ag_cert,
                ENCRYPTION = REQUIRED ALGORITHM AES,
                ROLE = ALL
            );
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.server_permissions p
            INNER JOIN sys.endpoints e
                ON p.major_id = e.endpoint_id
            INNER JOIN sys.server_principals sp
                ON p.grantee_principal_id = sp.principal_id
            WHERE e.name = N'\''AG_endpoint'\''
              AND sp.name = N'\''sqlserver1_ag_login'\''
              AND p.permission_name = N'\''CONNECT'\''
              AND p.state IN (N'\''G'\'', N'\''W'\'')
        )
        BEGIN
            GRANT CONNECT ON ENDPOINT::[AG_endpoint]
            TO [sqlserver1_ag_login];
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.server_permissions p
            INNER JOIN sys.endpoints e
                ON p.major_id = e.endpoint_id
            INNER JOIN sys.server_principals sp
                ON p.grantee_principal_id = sp.principal_id
            WHERE e.name = N'\''AG_endpoint'\''
              AND sp.name = N'\''sqlserver2_ag_login'\''
              AND p.permission_name = N'\''CONNECT'\''
              AND p.state IN (N'\''G'\'', N'\''W'\'')
        )
        BEGIN
            GRANT CONNECT ON ENDPOINT::[AG_endpoint]
            TO [sqlserver2_ag_login];
        END;
    "
'

echo "AG Endpoint created and permissions granted on sqlserver-3 successfully."

echo "--------------------------------------------------------------------"

echo "AG Endpoints setup completed successfully."
