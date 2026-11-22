
#!/usr/bin/env bash
set -euo pipefail

create_certificate() {
    local NODE="$1"
    local CERT_NAME="$2"
    local MASTER_KEY_PASSWORD="$3"
    local SUBJECT="$4"
    local CERT_FILE="/var/opt/mssql/data/${CERT_NAME}.cer"

    echo "Preparing AG Certificate on ${NODE}..."

    docker exec "$NODE" bash -c "
        set -a
        source /etc/sqlserver-secrets/sqlserver.env
        set +a

        # Wait until SQL Server is ready.
        for attempt in {1..60}; do
            if /opt/mssql-tools18/bin/sqlcmd \
                -S localhost \
                -U sa \
                -P \"\$MSSQL_SA_PASSWORD\" \
                -C \
                -d master \
                -b \
                -Q 'SELECT 1' \
                > /dev/null 2>&1; then
                break
            fi

            if [ \"\$attempt\" -eq 60 ]; then
                echo 'SQL Server did not become ready.'
                exit 1
            fi

            sleep 2
        done

        /opt/mssql-tools18/bin/sqlcmd \
            -S localhost \
            -U sa \
            -P \"\$MSSQL_SA_PASSWORD\" \
            -C \
            -d master \
            -b \
            -Q \"
                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.symmetric_keys
                    WHERE name = '##MS_DatabaseMasterKey##'
                )
                BEGIN
                    CREATE MASTER KEY
                    ENCRYPTION BY PASSWORD = '${MASTER_KEY_PASSWORD}';
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.certificates
                    WHERE name = '${CERT_NAME}'
                )
                BEGIN
                    CREATE CERTIFICATE ${CERT_NAME}
                    WITH SUBJECT = '${SUBJECT}';
                END;

                IF NOT EXISTS (
                    SELECT 1
                    FROM sys.certificates
                    WHERE name = '${CERT_NAME}'
                )
                    THROW 50001, 'AG Certificate was not created.', 1;
            \"

        if [ ! -f '${CERT_FILE}' ]; then
            /opt/mssql-tools18/bin/sqlcmd \
                -S localhost \
                -U sa \
                -P \"\$MSSQL_SA_PASSWORD\" \
                -C \
                -d master \
                -b \
                -Q \"
                    BACKUP CERTIFICATE ${CERT_NAME}
                    TO FILE = '${CERT_FILE}';
                \"
        fi

        test -s '${CERT_FILE}'

        echo 'Certificate and backup verified on ${NODE}.'
    "

    echo "--------------------------------------------------------------------"
}

echo "Creating AG Certificates..."

create_certificate \
    "sqlserver-1" \
    "sqlserver1_ag_cert" \
    "SqlServer1MasterKey" \
    "sqlserver-1 AG Certificate"

create_certificate \
    "sqlserver-2" \
    "sqlserver2_ag_cert" \
    "SqlServer2MasterKey" \
    "sqlserver-2 AG Certificate"

create_certificate \
    "sqlserver-3" \
    "sqlserver3_ag_cert" \
    "SqlServer3MasterKey" \
    "sqlserver-3 AG Certificate"

echo "AG Certificates setup completed successfully."
