#!/usr/bin/env bash
set -euo pipefail

BASE_DIR="$HOME/Desktop/Medical"
SQL_DIR="$BASE_DIR/Infrastructure/SqlServerHaCluster"
REDIS_DIR="$BASE_DIR/Infrastructure/RedisCluster"

cleanup_compose_and_data() {
  local compose_dir="$1"
  local data_dir="$2"

  if [ -d "$compose_dir" ]; then
    echo "Cleaning up compose stack in: $compose_dir"
    docker compose -f "$compose_dir/docker-compose.yml" down -v --remove-orphans || true
  fi

  if [ -d "$data_dir" ]; then
    echo "Deleting contents of: $data_dir"
    sudo find "$data_dir" -mindepth 1 -delete
  fi
}

# 1. Clean Redis Cluster (redis1..redis6 + data)
cleanup_compose_and_data "$REDIS_DIR" "$REDIS_DIR/data"

# 2. Clean SQL Server HA Cluster (sqlserver-1..3 + data)
cleanup_compose_and_data "$SQL_DIR" "$SQL_DIR/data"

# 3. Clean main Medical stack (medical-app-1..3, medical-nginx-1..2, medical-keepalived-1..2) if compose exists at root
if [ -f "$BASE_DIR/docker-compose.yml" ]; then
  echo "Cleaning up main stack in: $BASE_DIR"
  docker compose -f "$BASE_DIR/docker-compose.yml" down -v --remove-orphans || true
fi

# Fallback: if any containers/volumes from the project are outside those compose files
REMAINING_CONTAINERS=$(docker ps -aq --filter "name=redis" --filter "name=sqlserver-" --filter "name=medical-" || true)
if [ -n "$REMAINING_CONTAINERS" ]; then
  echo "Removing remaining project containers..."
  docker rm -f $REMAINING_CONTAINERS
fi

echo "Cleanup completed."
