#!/usr/bin/env bash
set -euo pipefail

PROJECT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

clear

cat <<'EOF'

        +------------------------------------------+
        |                                          |
        |          +------------------+            |
        |          |        +         |            |
        |          |     MEDICAL      |            |
        |          |  INFRASTRUCTURE  |            |
        |          +------------------+            |
        |                                          |
        |               DEPLOYMENT                 |
        +------------------------------------------+

EOF

echo "Medical Website Application is being installed."
echo "--------------------------------------------------------------------"


# --------------------------------------------------------------------
# SQL SERVER HA CLUSTER
# --------------------------------------------------------------------

cat <<'EOF'

              SQL SERVER HA CLUSTER
          ==============================

             +----------+     +----------+
             | SQL NODE |<--->| SQL NODE |
             |    1     |     |    2     |
             +----------+     +----------+
                   \             /
                    \           /
                     \         /
                      +-------+
                      | SQL   |
                      | NODE  |
                      |   3   |
                      +-------+

                +-------------------+
                |   PACEMAKER       |
                |   COROSYNC        |
                |   STONITH         |
                +-------------------+

              ALWAYS ON AVAILABILITY GROUP

EOF

cd "$PROJECT_DIR/Infrastructure/SqlServerHaCluster"
bash ./scripts/main/setup-ha-cluster.sh
bash ./scripts/main/setup-sql-ha.sh

echo "SQL Server HA Cluster started successfully."
echo "--------------------------------------------------------------------"


# --------------------------------------------------------------------
# REDIS CLUSTER
# --------------------------------------------------------------------

cat <<'EOF'

                 REDIS CLUSTER
          ===========================

                  ___________
                 /_________/ |
                |  REDIS   | |
                |__________|/
                |  REDIS   | |
                |__________|/
                |  REDIS   | |
                |__________|/

              CACHE  |  PUB/SUB
              DISTRIBUTED LOCKS

EOF

cd "$PROJECT_DIR/Infrastructure/RedisCluster"
docker compose up -d

echo yes | docker exec -i redis1 redis-cli --cluster create \
  172.31.0.20:6379 172.31.0.21:6379 172.31.0.22:6379 \
  172.31.0.23:6379 172.31.0.24:6379 172.31.0.25:6379 \
  --cluster-replicas 1

echo "Redis Cluster started successfully."
echo "--------------------------------------------------------------------"

# --------------------------------------------------------------------
# MEDICAL APPLICATIONS
# --------------------------------------------------------------------

cat <<'EOF'

                MEDICAL APPLICATIONS
          ==============================

              .----------------------.
              |  MEDICAL APPLICATION |
              |----------------------|
              |                      |
              |          +           |
              |       +--+--+        |
              |       |  +  |        |
              |       +-----+        |
              |                      |
              |  USERS  |  SERVICES  |
              '----------------------'

                    ASP.NET CORE
                  APPLICATION NODES

EOF

cd "$PROJECT_DIR/Infrastructure/MedicalApps"
docker compose up -d

echo "Medical Applications started successfully."
echo "--------------------------------------------------------------------"


# --------------------------------------------------------------------
# NGINX & KEEPALIVED
# --------------------------------------------------------------------

cat <<'EOF'

                NGINX & KEEPALIVED
          ==============================

                       CLIENT
                         |
                         v
                   +-----------+
                   |    VIP    |
                   +-----------+
                         |
                +--------+--------+
                |                 |
                v                 v
          +-----------+     +-----------+
          |   NGINX   |     |   NGINX   |
          |  ACTIVE   |     |  PASSIVE  |
          +-----------+     +-----------+
                |                 |
                +--------+--------+
                         |
                    KEEPALIVED
                   VRRP FAILOVER

EOF

cd "$PROJECT_DIR/Infrastructure/NginxKeepalived"
docker compose up -d --build

echo "Nginx and Keepalived started successfully."
echo "--------------------------------------------------------------------"


cat <<'EOF'

        +------------------------------------------+
        |                                          |
        |       MEDICAL INFRASTRUCTURE             |
        |                                          |
        |          DEPLOYMENT COMPLETE             |
        |                                          |
        +------------------------------------------+

EOF

echo "Medical Infrastructure deployed successfully."
