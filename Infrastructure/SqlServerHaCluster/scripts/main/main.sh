#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

bash "$SCRIPT_DIR/setup-ha-cluster.sh"

bash "$SCRIPT_DIR/setup-sql-ha.sh"