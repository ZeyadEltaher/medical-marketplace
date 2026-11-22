#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
COMPOSE_FILE="$SCRIPT_DIR/../../docker-compose.yml"
PROXY_SCRIPT="$SCRIPT_DIR/../../proxy/tcp_proxy.py"

SERVICE_NAME="medical-tcp-proxy.service"
LISTEN_HOST="172.32.0.1"
LISTEN_PORT="2375"

echo "Preparing Docker Compose network..."
docker compose -f "$COMPOSE_FILE" up --no-recreate --no-start
echo "Docker Compose network prepared successfully."

echo "--------------------------------------------------------------------"

echo "Checking whether the Docker TCP Proxy script exists..."
if [[ ! -f "$PROXY_SCRIPT" ]]; then
    echo "Docker TCP Proxy script not found: $PROXY_SCRIPT"
    exit 1
fi
echo "Docker TCP Proxy script found successfully."

echo "--------------------------------------------------------------------"

echo "Checking whether the Docker bridge IP $LISTEN_HOST is available..."
if ! ip -4 addr | grep -q "$LISTEN_HOST"; then
    echo "Docker bridge IP $LISTEN_HOST is not available."
    exit 1
fi
echo "Docker bridge IP $LISTEN_HOST is available."

echo "--------------------------------------------------------------------"

echo "Ensuring systemd service $SERVICE_NAME is running..."
if ! sudo systemctl is-active --quiet "$SERVICE_NAME"; then
    sudo systemctl start "$SERVICE_NAME"
fi

echo "Waiting for Docker TCP Proxy to listen on $LISTEN_HOST:$LISTEN_PORT..."
for attempt in {1..15}; do
    if sudo systemctl is-active --quiet "$SERVICE_NAME" \
        && ss -lnt | grep -qE "${LISTEN_HOST//./\\.}:$LISTEN_PORT\b"; then
        echo "Docker TCP Proxy is active and listening on $LISTEN_HOST:$LISTEN_PORT."
        echo "--------------------------------------------------------------------"
        exit 0
    fi
    sleep 1
done

echo "Error: Docker TCP Proxy service failed to become ready on $LISTEN_HOST:$LISTEN_PORT in time."
sudo systemctl status "$SERVICE_NAME" --no-pager || true
exit 1
