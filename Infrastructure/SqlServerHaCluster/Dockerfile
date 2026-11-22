FROM sqlserver:2025

ENV DEBIAN_FRONTEND=noninteractive

# --------------------------------------------------
# 1. Install SQL Server HA stack and system utilities
# --------------------------------------------------

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
        systemd \
        systemd-sysv \
        pacemaker \
        pacemaker-cli-utils \
        corosync \
        pcs \
        mssql-server-ha \
        chrony \
        iproute2 \
        iputils-ping \
        dnsutils \
        netcat-openbsd \
        jq \
        curl \
        wget \
        vim \
        net-tools \
        tcpdump \
        rsync \
        openssh-server \
        python3 \
        ca-certificates \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

# --------------------------------------------------
# 2. Prepare directories
# --------------------------------------------------

RUN mkdir -p \
        /etc/sqlserver-secrets \
        /etc/systemd/system \
        /usr/local/sbin

# --------------------------------------------------
# 3. Install custom Docker STONITH agent
# --------------------------------------------------

COPY fencing/fence_docker /usr/sbin/fence_docker

RUN chown root:root /usr/sbin/fence_docker && \
    chmod 755 /usr/sbin/fence_docker

# --------------------------------------------------
# 4. Enable PCS management service
# --------------------------------------------------

RUN systemctl enable pcsd

# Corosync and Pacemaker are deliberately not enabled here.
# Cluster startup is controlled by PCS after cluster setup.

RUN systemctl disable corosync pacemaker || true

# --------------------------------------------------
# 5. Container runtime configuration
# --------------------------------------------------

STOPSIGNAL SIGRTMIN+3

CMD ["/sbin/init"]
