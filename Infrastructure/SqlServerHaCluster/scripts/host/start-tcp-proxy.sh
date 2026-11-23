
#!/usr/bin/env bash

set -Eeuo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

COMPOSE_FILE="${SCRIPT_DIR}/../../docker-compose.yml"
SERVICE_NAME="medical-tcp-proxy.service"

LISTEN_HOST="172.32.0.1"
LISTEN_PORT="2375"

echo "Preparing Docker Compose network..."

docker compose -f "$COMPOSE_FILE" up --no-recreate --no-start

echo "Docker Compose network prepared successfully."

echo "--------------------------------------------------------------------"

echo "Checking whether the Docker TCP Proxy script exists..."

PROXY_SCRIPT="${SCRIPT_DIR}/../../proxy/tcp_proxy.py"

if [[ ! -f "$PROXY_SCRIPT" ]]; then
    echo "ERROR: Docker TCP Proxy script not found: $PROXY_SCRIPT"
    exit 1
fi

echo "Docker TCP Proxy script found successfully."

echo "--------------------------------------------------------------------"

echo "Checking Docker bridge IP..."

if ! ip -4 addr | grep -q "$LISTEN_HOST"; then
    echo "ERROR: Docker bridge IP $LISTEN_HOST is not available."
    exit 1
fi

echo "Docker bridge IP is available."

echo "--------------------------------------------------------------------"

echo "Starting systemd service..."

sudo systemctl start "$SERVICE_NAME"

echo "Waiting for Docker TCP Proxy readiness..."

for attempt in {1..15}; do

    if systemctl is-active --quiet "$SERVICE_NAME" \
        && ss -lnt | grep -Fq "${LISTEN_HOST}:${LISTEN_PORT}"; then

        echo "Docker TCP Proxy is active and listening on ${LISTEN_HOST}:${LISTEN_PORT}."

        exit 0
    fi

    sleep 1
done

echo "ERROR: Docker TCP Proxy failed to become ready."

sudo journalctl -u "$SERVICE_NAME" -n 30 --no-pager || true

exit 1
