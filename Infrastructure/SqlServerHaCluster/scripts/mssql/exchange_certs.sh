#!/usr/bin/env bash
set -euo pipefail

CERT_DIR="${CERT_DIR:-/tmp/sqlserver-ag-certs}"

mkdir -p "$CERT_DIR"

echo "Copying AG Certificates from SQL Server Nodes..."

docker cp sqlserver-1:/var/opt/mssql/data/sqlserver1_ag_cert.cer "$CERT_DIR/"
docker cp sqlserver-2:/var/opt/mssql/data/sqlserver2_ag_cert.cer "$CERT_DIR/"
docker cp sqlserver-3:/var/opt/mssql/data/sqlserver3_ag_cert.cer "$CERT_DIR/"

echo "AG Certificates collected successfully."

ls -lh "$CERT_DIR/"

echo "--------------------------------------------------------------------"

echo "Distributing AG Certificates to All SQL Server Nodes..."

echo "Copying sqlserver-2 and sqlserver-3 certificates to sqlserver-1..."

docker cp "$CERT_DIR/sqlserver2_ag_cert.cer" sqlserver-1:/var/opt/mssql/data/
docker cp "$CERT_DIR/sqlserver3_ag_cert.cer" sqlserver-1:/var/opt/mssql/data/

echo "Certificates copied to sqlserver-1 successfully."

echo "--------------------------------------------------------------------"

echo "Copying sqlserver-1 and sqlserver-3 certificates to sqlserver-2..."

docker cp "$CERT_DIR/sqlserver1_ag_cert.cer" sqlserver-2:/var/opt/mssql/data/
docker cp "$CERT_DIR/sqlserver3_ag_cert.cer" sqlserver-2:/var/opt/mssql/data/

echo "Certificates copied to sqlserver-2 successfully."

echo "--------------------------------------------------------------------"

echo "Copying sqlserver-1 and sqlserver-2 certificates to sqlserver-3..."

docker cp "$CERT_DIR/sqlserver1_ag_cert.cer" sqlserver-3:/var/opt/mssql/data/
docker cp "$CERT_DIR/sqlserver2_ag_cert.cer" sqlserver-3:/var/opt/mssql/data/

echo "Certificates copied to sqlserver-3 successfully."

echo "--------------------------------------------------------------------"

for node in sqlserver-1 sqlserver-2 sqlserver-3; do
    echo "Setting AG Certificate Permissions on $node..."

    docker exec -u root "$node" chown mssql:mssql \
        /var/opt/mssql/data/sqlserver1_ag_cert.cer \
        /var/opt/mssql/data/sqlserver2_ag_cert.cer \
        /var/opt/mssql/data/sqlserver3_ag_cert.cer

    docker exec -u root "$node" chmod 640 \
        /var/opt/mssql/data/sqlserver1_ag_cert.cer \
        /var/opt/mssql/data/sqlserver2_ag_cert.cer \
        /var/opt/mssql/data/sqlserver3_ag_cert.cer

    echo "AG Certificate permissions configured on $node successfully."

    echo "--------------------------------------------------------------------"
done

echo "AG Certificate distribution and permissions setup completed."