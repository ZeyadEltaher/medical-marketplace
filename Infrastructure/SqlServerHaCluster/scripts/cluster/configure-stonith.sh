#!/usr/bin/env bash
set -euo pipefail

NODE="sqlserver-1"
HOST_MAP="sqlserver-1=http://172.32.0.1:2375,sqlserver-2=http://172.32.0.1:2375,sqlserver-3=http://172.32.0.1:2375"
HOST_LIST="sqlserver-1,sqlserver-2,sqlserver-3"

echo "Starting Pacemaker and Corosync on all cluster nodes..."

for node in sqlserver-1 sqlserver-2 sqlserver-3; do
    echo "Starting Pacemaker and Corosync on $node..."

    docker exec "$node" systemctl start pacemaker corosync

    echo "Pacemaker and Corosync started successfully on $node."

    echo "--------------------------------------------------------------------"
done

echo "Pacemaker and Corosync started successfully on all cluster nodes."

echo "--------------------------------------------------------------------"

echo "Verifying fence_docker resource agent..."

docker exec "$NODE" pcs stonith describe fence_docker >/dev/null
docker exec "$NODE" fence_docker -o metadata >/dev/null

echo "fence_docker resource agent verified successfully."

echo "--------------------------------------------------------------------"

echo "Creating STONITH resource docker_fence..."

docker exec "$NODE" pcs stonith create docker_fence fence_docker \
    host_map="$HOST_MAP" \
    pcmk_host_list="$HOST_LIST" \
    op monitor interval=30s

echo "STONITH resource docker_fence created successfully."

echo "--------------------------------------------------------------------"

echo "Updating STONITH resource timeouts..."

docker exec "$NODE" pcs stonith update docker_fence \
    pcmk_off_timeout=60s \
    pcmk_reboot_timeout=90s \
    pcmk_monitor_timeout=30s

echo "STONITH resource timeouts updated successfully."

echo "--------------------------------------------------------------------"

echo "Enabling STONITH property..."

docker exec "$NODE" pcs property set stonith-enabled=true

echo "STONITH property enabled successfully."

echo "--------------------------------------------------------------------"

echo "Displaying docker_fence STONITH resource configuration..."

docker exec "$NODE" pcs stonith config docker_fence

echo "STONITH resource configuration displayed successfully."

echo "--------------------------------------------------------------------"

echo "Displaying cluster properties..."

docker exec "$NODE" pcs property config

echo "Cluster properties displayed successfully."

echo "--------------------------------------------------------------------"

echo "STONITH configuration completed successfully."
