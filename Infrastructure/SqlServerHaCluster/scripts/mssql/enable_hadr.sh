#!/usr/bin/env bash
set -euo pipefail

NODES=("sqlserver-1" "sqlserver-2" "sqlserver-3")

for node in "${NODES[@]}"; do
    echo "Enabling Always On Availability Groups on $node..."

    docker exec "$node" /opt/mssql/bin/mssql-conf set hadr.hadrenabled 1

    echo "Always On Availability Groups enabled on $node successfully."
    echo "--------------------------------------------------------------------"
done

echo "Always On configuration completed on all nodes."

echo "--------------------------------------------------------------------"

for node in "${NODES[@]}"; do
    echo "Stopping SQL Server on $node..."
    docker exec "$node" systemctl stop mssql-server || true

    # Wait until no leftover sqlservr process is running
    for _ in $(seq 1 15); do
        if ! docker exec "$node" pgrep -f "/opt/mssql/bin/sqlservr" >/dev/null 2>&1; then
            break
        fi
        sleep 1
    done
    docker exec "$node" pkill -9 -f "/opt/mssql/bin/sqlservr" >/dev/null 2>&1 || true
    docker exec "$node" systemctl reset-failed mssql-server >/dev/null 2>&1 || true

    echo "Starting SQL Server on $node..."
    docker exec "$node" systemctl start mssql-server

    status=""
    for _ in $(seq 1 30); do
        status="$(docker exec "$node" systemctl is-active mssql-server 2>/dev/null || true)"
        if [[ "$status" == "active" ]]; then
            break
        fi
        sleep 2
    done

    if [[ "$status" != "active" ]]; then
        echo "Error: mssql-server on $node did not become active (last status: '${status:-unknown}')."
        docker exec "$node" journalctl -u mssql-server -n 25 --no-pager || true
        exit 1
    fi

    echo "Waiting for SQL Server on $node to accept connections (port 1433)..."
    ready=0
    for _ in $(seq 1 60); do
        if docker exec "$node" bash -c "exec 3<>/dev/tcp/127.0.0.1/1433" 2>/dev/null; then
            ready=1
            break
        fi
        sleep 2
    done

    if [[ "$ready" -ne 1 ]]; then
        echo "Error: SQL Server on $node did not open port 1433 in time."
        docker exec "$node" tail -n 30 /var/opt/mssql/log/errorlog 2>/dev/null || true
        exit 1
    fi

    echo "SQL Server stopped and started cleanly on $node."
    echo "--------------------------------------------------------------------"
done

echo "SQL Server stop/start completed on all nodes successfully."

echo "SQL Server restart completed on all nodes successfully."