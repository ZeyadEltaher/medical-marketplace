#!/usr/bin/env bash
set -euo pipefail

CONTAINERS=("sqlserver-1" "sqlserver-2" "sqlserver-3")

echo "Waiting for systemd to initialize inside all containers..."

for container in "${CONTAINERS[@]}"; do
    echo "Waiting for systemd inside $container..."

    attempt=0
    max_attempts=30

    while true; do
        # Check if systemd is responding via systemctl ping or checking pid 1.
        if docker exec "$container" systemctl is-system-running &>/dev/null; then
            sys_state=$(docker exec "$container" systemctl is-system-running 2>/dev/null || echo "unknown")
            echo "Systemd is ready inside $container. State: $sys_state"
            break
        fi

        # Fallback check: If PID 1 is active and pcsd or basic services are up.
        if docker exec "$container" pgrep -f /sbin/init &>/dev/null; then
            # Give it a small grace period for services to settle.
            sleep 2
            echo "Systemd initialized via PID 1 inside $container."
            break
        fi

        attempt=$((attempt + 1))

        if [ "$attempt" -ge "$max_attempts" ]; then
            echo "Timed out waiting for systemd inside $container. Forcing continuation..."
            break
        fi

        echo "Still waiting for systemd inside $container..."
        sleep 2
    done

    echo "Systemd initialization check completed for $container."

    echo "--------------------------------------------------------------------"
done

echo "Systemd initialization checks completed for all containers."
