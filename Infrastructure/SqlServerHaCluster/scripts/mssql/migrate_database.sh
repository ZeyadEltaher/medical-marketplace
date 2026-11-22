#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd -- "$SCRIPT_DIR/../../../../" && pwd)"

ENV_FILE="${ENV_FILE:-$PROJECT_DIR/Infrastructure/SqlServerHaCluster/secrets/sqlserver.env}"

if [[ -f "$ENV_FILE" ]]; then
    set -a
    # shellcheck disable=SC1090
    source "$ENV_FILE"
    set +a
else
    echo "Warning: Secrets file not found at '$ENV_FILE', relying on pre-exported environment variables."
fi

SQLCMD_BIN="${SQLCMD_BIN:-$(command -v sqlcmd || echo /opt/mssql-tools18/bin/sqlcmd)}"
SQL_HOST="${SQL_HOST:-172.32.0.100}"
SQL_PORT="${SQL_PORT:-1433}"
SQL_DB="${SQL_DB:-MedicalDB}"
SQL_USER="${SQL_USER:-sa}"
SQL_PASSWORD="${MSSQL_SA_PASSWORD:?MSSQL_SA_PASSWORD is required}"
CHECK_TABLE="${CHECK_TABLE:-__EFMigrationsHistory}"

PROJECT_PATH="${PROJECT_PATH:-$PROJECT_DIR/Medical.DAL/Medical.DAL.csproj}"
STARTUP_PROJECT="${STARTUP_PROJECT:-$PROJECT_DIR/Medical.PL/Medical.PL.csproj}"

CONN_STR="Server=${SQL_HOST},${SQL_PORT};Database=${SQL_DB};User Id=${SQL_USER};Password=${SQL_PASSWORD};TrustServerCertificate=True;MultiSubnetFailover=True;"

echo "Checking SQL Server connectivity on ${SQL_HOST}:${SQL_PORT}..."

"$SQLCMD_BIN" \
    -S "${SQL_HOST},${SQL_PORT}" \
    -U "$SQL_USER" \
    -P "$SQL_PASSWORD" \
    -d "$SQL_DB" \
    -C \
    -b \
    -Q "SELECT 1;" \
    >/dev/null

echo "SQL Server connectivity verified successfully."

echo "--------------------------------------------------------------------"

echo "Checking whether '${CHECK_TABLE}' exists in ${SQL_DB}..."

TABLE_EXISTS=$(
    "$SQLCMD_BIN" \
        -S "${SQL_HOST},${SQL_PORT}" \
        -U "$SQL_USER" \
        -P "$SQL_PASSWORD" \
        -d "$SQL_DB" \
        -C \
        -b \
        -h -1 \
        -W \
        -Q "
            SET NOCOUNT ON;

            SELECT
                CASE
                    WHEN OBJECT_ID(N'dbo.${CHECK_TABLE}', N'U') IS NOT NULL
                    THEN 1
                    ELSE 0
                END;
        " | tr -d '[:space:]'
)

if [[ "$TABLE_EXISTS" != "0" && "$TABLE_EXISTS" != "1" ]]; then
    echo "Error: Unexpected table existence result: '${TABLE_EXISTS}'"
    exit 1
fi

echo "Table existence check completed successfully."

echo "--------------------------------------------------------------------"

if [[ "$TABLE_EXISTS" == "1" ]]; then
    echo "Table '${CHECK_TABLE}' already exists in ${SQL_DB}."
    echo "Skipping initial migration."

    echo "--------------------------------------------------------------------"

    exit 0
fi

echo "Restoring NuGet packages..."

dotnet restore "$PROJECT_PATH"
dotnet restore "$STARTUP_PROJECT"

echo "--------------------------------------------------------------------"

echo "Running EF Core database migrations..."

dotnet ef database update \
    --project "$PROJECT_PATH" \
    --startup-project "$STARTUP_PROJECT" \
    --connection "$CONN_STR"

echo "EF Core database migrations completed successfully."