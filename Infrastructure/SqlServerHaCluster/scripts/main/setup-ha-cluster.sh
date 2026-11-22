#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(cd -- "$SCRIPT_DIR/../../" && pwd)"

ENV_FILE="$PROJECT_DIR/secrets/sqlserver.env"

if [[ ! -f "$ENV_FILE" ]]; then
    echo "Error: Environment file '$ENV_FILE' not found!"
    exit 1
fi

source "$ENV_FILE"

: "${HACLUSTER_PASSWORD:?HACLUSTER_PASSWORD is required}"

export HACLUSTER_PASSWORD

HOST_SCRIPTS_DIR="$PROJECT_DIR/scripts/host"
CLUSTER_SCRIPTS_DIR="$PROJECT_DIR/scripts/cluster"

echo "======================================================================================================"
echo "🚀 Starting Pacemaker and Corosync HA Cluster Setup..."
echo "======================================================================================================"

for DIR in "$HOST_SCRIPTS_DIR" "$CLUSTER_SCRIPTS_DIR"; do
    if [[ ! -d "$DIR" ]]; then
        echo "Error: Directory '$DIR' not found!"
        exit 1
    fi
done

echo "======================================================================="
echo " [1/10] Starting Docker TCP Proxy..."
echo "======================================================================="
bash "$HOST_SCRIPTS_DIR/start-tcp-proxy.sh"

echo "======================================================================="
echo " [2/10] Verifying Docker API..."
echo "======================================================================="
bash "$HOST_SCRIPTS_DIR/verify-docker-api.sh"

echo "======================================================================="
echo " [3/10] Starting Docker Containers (Compose Up)..."
echo "======================================================================="
docker compose -f "$PROJECT_DIR/docker-compose.yml" up -d --build

echo "======================================================================="
echo " [4/10] Waiting for containers to be fully running via Docker API..."
echo "======================================================================="
bash "$HOST_SCRIPTS_DIR/verify-containers-api.sh"

echo "======================================================================="
echo " [5/10] Waiting for systemd inside containers to be fully ready..."
echo "======================================================================="
bash "$CLUSTER_SCRIPTS_DIR/verify-nodes.sh"

echo "========================================================================"
echo " [6/10] Preparing Nodes (hacluster passwords & pcsd)..."
echo "======================================================================="
bash "$CLUSTER_SCRIPTS_DIR/prepare-nodes.sh"

echo "======================================================================="
echo " [7/10] Bootstrapping Pacemaker Cluster..."
echo "======================================================================="
bash "$CLUSTER_SCRIPTS_DIR/bootstrap-cluster.sh"

echo "======================================================================="
echo " [8/10] Configuring STONITH (fence_docker)..."
echo "======================================================================="
bash "$CLUSTER_SCRIPTS_DIR/configure-stonith.sh"

echo "======================================================================="
echo " [9/10] Starting Cluster Services..."
echo "======================================================================="
bash "$CLUSTER_SCRIPTS_DIR/start-cluster.sh"

echo "======================================================================="
echo " [10/10] Verify Cluster Configurations..."
echo "======================================================================="
bash "$CLUSTER_SCRIPTS_DIR/verify-cluster.sh"

echo "======================================================================="
echo " ✅ HA Cluster deployed and verified successfully!"
echo "======================================================================="
