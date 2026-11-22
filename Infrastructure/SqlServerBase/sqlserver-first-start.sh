#!/bin/bash
set -euo pipefail

SETUP_MARKER="/var/opt/mssql/.setup-complete"

if [ ! -f "$SETUP_MARKER" ]; then

    echo "Configuring SQL Server for the first time..."

    : "${ACCEPT_EULA:?ACCEPT_EULA is required}"
    : "${MSSQL_PID:?MSSQL_PID is required}"
    : "${MSSQL_SA_PASSWORD:?MSSQL_SA_PASSWORD is required}"

    chown -R mssql:mssql /var/opt/mssql

    echo "Running SQL Server setup..."

    if /opt/mssql/bin/mssql-conf -n setup; then
        echo "SQL Server setup command completed successfully."
    else
        EXIT_CODE=$?

        echo "ERROR: SQL Server setup failed with exit code: $EXIT_CODE"
        exit "$EXIT_CODE"
    fi

    # Create the marker only after successful setup.
    touch "$SETUP_MARKER"

    echo "SQL Server setup completed successfully."

else
    echo "SQL Server is already configured."
fi

