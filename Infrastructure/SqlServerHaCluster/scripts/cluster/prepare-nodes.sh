#!/usr/bin/env bash
set -euo pipefail

: "${HACLUSTER_PASSWORD:?HACLUSTER_PASSWORD is required}"

NODES=("sqlserver-1" "sqlserver-2" "sqlserver-3")

echo "Preparing hacluster accounts and pcsd services..."

for node in "${NODES[@]}"; do
    echo "Checking hacluster account on $node..."

    docker exec "$node" id hacluster >/dev/null
    echo "hacluster account verified successfully on $node."
    echo "Setting hacluster password non-interactively on $node..."

    echo "hacluster:${HACLUSTER_PASSWORD}" | docker exec -i "$node" chpasswd
    echo "hacluster password configured successfully on $node."
    echo "Ensuring pcsd service is active on $node..."

    docker exec "$node" systemctl enable --now pcsd
    echo "pcsd service enabled and started successfully on $node."

    echo "--------------------------------------------------------------------"
done

echo "All cluster nodes prepared successfully."

