#!/usr/bin/env bash
set -euo pipefail

# Resolve project directory
SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd -- "$SCRIPT_DIR/../../../../" && pwd)"

# Load database credentials and configuration
ENV_FILE="${ENV_FILE:-$PROJECT_DIR/Infrastructure/SqlServerHaCluster/secrets/sqlserver.env}"

if [[ -f "$ENV_FILE" ]]; then
    set -a
    # shellcheck disable=SC1090
    source "$ENV_FILE"
    set +a
else
    echo "Warning: Secrets file not found at '$ENV_FILE', relying on pre-exported environment variables."
fi

# Define migration container configuration
MIGRATION_IMAGE="${MIGRATION_IMAGE:-medical-migration:latest}"
DOCKER_NETWORK="${DOCKER_NETWORK:-host}"

# Define SQL Server connection parameters
SQL_HOST="${SQL_HOST:-172.32.0.100}"
SQL_PORT="${SQL_PORT:-1433}"
SQL_DB="${SQL_DB:-MedicalDB}"
SQL_USER="${SQL_USER:-sa}"
SQL_PASSWORD="${MSSQL_SA_PASSWORD:?MSSQL_SA_PASSWORD is required}"

# Build SQL Server connection string
CONN_STR="Server=${SQL_HOST},${SQL_PORT};Database=${SQL_DB};User Id=${SQL_USER};Password=${SQL_PASSWORD};TrustServerCertificate=True;MultiSubnetFailover=True;"

echo "Starting Database Migration Container..."

echo "-----------------------------------------------------------------------"

# Verify that the migration image exists locally
if ! docker image inspect "$MIGRATION_IMAGE" >/dev/null 2>&1; then
    echo "Error: Migration image '$MIGRATION_IMAGE' not found!"
    echo "Build or pull the migration image before running this script."
    exit 1
fi

echo "Migration Image: $MIGRATION_IMAGE"
echo "SQL Server: ${SQL_HOST}:${SQL_PORT}"
echo "Database: $SQL_DB"
echo "Docker Network: $DOCKER_NETWORK"

echo "-----------------------------------------------------------------------"

# Run the migration bundle inside an isolated container
docker run --rm \
    --network "$DOCKER_NETWORK" \
    --env "DB_CONNECTION=$CONN_STR" \
    --entrypoint /bin/sh \
    "$MIGRATION_IMAGE" \
    -c 'exec /app/efbundle --connection "$DB_CONNECTION"'

echo "-----------------------------------------------------------------------"

echo "Database migrations completed successfully."
