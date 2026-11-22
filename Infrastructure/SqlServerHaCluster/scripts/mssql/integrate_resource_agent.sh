#!/usr/bin/env bash
set -euo pipefail

echo "Creating MedicalAG Pacemaker resource..."

docker exec sqlserver-1 \
    pcs resource create MedicalAG \
        ocf:mssql:ag \
        ag_name=MedicalAG \
        meta failure-timeout=30s promotable notify=true

echo "MedicalAG Pacemaker resource created successfully."
