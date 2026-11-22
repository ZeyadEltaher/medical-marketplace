#!/usr/bin/env bash
set -euo pipefail

NODE="sqlserver-1"

echo "Checking cluster status..."

docker exec "$NODE" pcs status

echo "Cluster status retrieved successfully."

echo "--------------------------------------------------------------------"

echo "Checking cluster properties..."

docker exec "$NODE" pcs property config

echo "Cluster properties retrieved successfully."

echo "--------------------------------------------------------------------"

echo "Checking STONITH configuration..."

docker exec "$NODE" pcs stonith config docker_fence

echo "STONITH configuration retrieved successfully."
