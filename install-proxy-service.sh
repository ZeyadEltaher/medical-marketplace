#!/usr/bin/env bash

set -Eeuo pipefail

SERVICE_NAME="medical-tcp-proxy"
SERVICE_FILE="/etc/systemd/system/${SERVICE_NAME}.service"

PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

PROXY_SCRIPT="${PROJECT_ROOT}/Infrastructure/SqlServerHaCluster/proxy/tcp_proxy.py"

echo "============================================================"
echo " Medical Docker TCP Proxy - Systemd Installation"
echo "============================================================"

# 1. Validate prerequisites
echo "[1/5] Validating prerequisites..."

if [[ ! -f "$PROXY_SCRIPT" ]]; then
    echo "ERROR: TCP Proxy Python script not found:"
    echo "$PROXY_SCRIPT"
    exit 1
fi

if ! command -v python3 >/dev/null 2>&1; then
    echo "ERROR: Python 3 is not installed."
    exit 1
fi

PYTHON_PATH="$(command -v python3)"

# 2. Stop previous service
echo "[2/5] Stopping previous service if running..."

sudo systemctl stop "${SERVICE_NAME}.service" || true

# 3. Create systemd service
echo "[3/5] Creating systemd service..."

sudo tee "$SERVICE_FILE" > /dev/null <<EOF
[Unit]
Description=Medical Docker TCP Proxy
After=docker.service
Requires=docker.service

[Service]
Type=simple
WorkingDirectory=${PROJECT_ROOT}
ExecStart=${PYTHON_PATH} ${PROXY_SCRIPT}
Restart=on-failure
RestartSec=5

[Install]
WantedBy=multi-user.target
EOF

# 4. Reload systemd and enable service
echo "[4/5] Reloading systemd and enabling service..."

sudo systemctl daemon-reload
sudo systemctl enable "${SERVICE_NAME}.service"

# 5. Finish
echo "[5/5] Installation completed."

echo
echo "============================================================"
echo " TCP Proxy systemd service installed successfully."
echo "============================================================"

sudo systemctl cat "${SERVICE_NAME}.service"
