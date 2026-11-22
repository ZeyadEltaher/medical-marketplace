#!/usr/bin/env bash
set -euo pipefail

API="http://127.0.0.1:2375"

echo "Verifying Docker API connectivity..."

response=$(curl -fsS "$API/_ping")

if [[ "$response" != "OK" ]]; then
    echo "Docker API verification failed."
    exit 1
fi

echo "Docker API is reachable successfully."
