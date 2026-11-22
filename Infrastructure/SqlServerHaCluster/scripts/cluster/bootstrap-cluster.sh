#!/usr/bin/env bash
set -euo pipefail

NODE="sqlserver-1"
: "${HACLUSTER_PASSWORD:?HACLUSTER_PASSWORD is required}"

echo "Authenticating cluster nodes..."

docker exec "$NODE" pcs host auth \
    sqlserver-1 addr=172.32.0.10 \
    sqlserver-2 addr=172.32.0.11 \
    sqlserver-3 addr=172.32.0.12 \
    -u hacluster -p "$HACLUSTER_PASSWORD"

echo "Cluster nodes authenticated successfully."

echo "--------------------------------------------------------------------"

echo "Setting up cluster configuration..."

docker exec "$NODE" pcs cluster setup medical-sql-cluster \
    sqlserver-1 addr=172.32.0.10 \
    sqlserver-2 addr=172.32.0.11 \
    sqlserver-3 addr=172.32.0.12 \
    --force

echo "Cluster configuration created successfully."
