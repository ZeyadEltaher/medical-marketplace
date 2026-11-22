#!/usr/bin/env bash
set -euo pipefail

DOCKER_API="http://127.0.0.1:2375"

CONTAINERS=("sqlserver-1" "sqlserver-2" "sqlserver-3")

echo "Waiting for SQL Server containers to start..."

for container in "${CONTAINERS[@]}"; do
    echo "Waiting for $container to start..."

    while true; do
        # Query Docker daemon API for container JSON details and extract State.Status.
        status=$(curl -fsS --max-time 3 "$DOCKER_API/containers/$container/json" | jq -r '.State.Status // empty' 2>/dev/null || echo "")

        if [[ "$status" == "running" ]]; then
            echo "$container is running successfully."
            break
        fi

        echo "Waiting for $container..."
        sleep 2
    done

    echo "--------------------------------------------------------------------"
done

echo "All SQL Server containers are running successfully."

echo "--------------------------------------------------------------------"

echo "Updating SQL Server first-time setup script..."

for container in "${CONTAINERS[@]}"; do
    echo "Updating $container..."

    # Ensure /var/opt/mssql exists and is owned by mssql before setup runs
    docker exec "$container" bash -c 'mkdir -p /var/opt/mssql && chown -R mssql:mssql /var/opt/mssql'
    echo "Updated $container successfully."
done

echo "All SQL Server first-time setup scripts updated."

echo "--------------------------------------------------------------------"
echo "Waiting for initial first-time setup and SQL Server startup on all nodes..."

for container in "${CONTAINERS[@]}"; do
    # If first-setup service is active or failed, wait/reset appropriately
    while docker exec "$container" systemctl is-active --quiet sqlserver-first-setup 2>/dev/null; do
        echo "Waiting for sqlserver-first-setup to finish on $container..."
        sleep 2
    done

    # Wait until SQL Server actually listens on 1433 for the first time
    ready=0
    for _ in $(seq 1 90); do
        if docker exec "$container" bash -c "exec 3<>/dev/tcp/127.0.0.1/1433" 2>/dev/null; then
            ready=1
            break
        fi
        sleep 2
    done

    if [[ "$ready" -ne 1 ]]; then
        echo "Error: Initial SQL Server startup did not open port 1433 in time on $container."
        docker exec "$container" systemctl status mssql-server --no-pager || true
        docker exec "$container" ls -la /var/opt/mssql/log/ || true
        exit 1
    fi

    echo "$container initial setup and port 1433 are ready."
done

echo "All nodes completed initial startup cleanly."
