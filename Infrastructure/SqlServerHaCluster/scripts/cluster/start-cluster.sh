#!/usr/bin/env bash
set -euo pipefail

NODE="sqlserver-1"

echo "Starting cluster across all nodes..."

docker exec "$NODE" pcs cluster start --all

echo "Cluster started successfully across all nodes."

echo "--------------------------------------------------------------------"

echo "Enabling cluster services on boot..."

docker exec "$NODE" pcs cluster enable --all

echo "Cluster services enabled successfully on all nodes."

echo "--------------------------------------------------------------------"

echo "Waiting 5 seconds for cluster quorum..."

sleep 5

echo "Cluster quorum wait completed."

echo "--------------------------------------------------------------------"

echo "Checking cluster status..."

docker exec "$NODE" pcs status

echo "Cluster status retrieved successfully."
