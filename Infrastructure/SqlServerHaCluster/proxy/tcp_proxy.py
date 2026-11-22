#!/usr/bin/env python3
import socket
import threading
import logging

# Python Proxy: accepts connections from containers
LISTEN_HOST = "172.32.0.1"
LISTEN_PORT = 2375

# Docker Daemon: accepts connections from the local proxy
DOCKER_HOST = "127.0.0.1"
DOCKER_PORT = 2375

BUFFER_SIZE = 64 * 1024


logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s"
)


def forward(source, destination):
    """
    Forward bytes from source to destination.
    """

    try:
        while True:
            data = source.recv(BUFFER_SIZE)

            if not data:
                break

            destination.sendall(data)

    except (ConnectionError, OSError):
        pass

    finally:
        try:
            destination.shutdown(socket.SHUT_WR)
        except OSError:
            pass


def handle_client(client_socket, client_address):

    logging.info("New connection from %s:%s", *client_address)

    docker_socket = socket.socket(
        socket.AF_INET,
        socket.SOCK_STREAM
    )

    try:
        # Connect to Docker daemon
        docker_socket.connect((DOCKER_HOST, DOCKER_PORT))

        # Bidirectional forwarding
        client_to_docker = threading.Thread(
            target=forward,
            args=(client_socket, docker_socket)
        )

        docker_to_client = threading.Thread(
            target=forward,
            args=(docker_socket, client_socket)
        )

        client_to_docker.start()
        docker_to_client.start()

        client_to_docker.join()
        docker_to_client.join()

    except Exception:
        logging.exception("Connection error")

    finally:
        client_socket.close()
        docker_socket.close()

        logging.info("Connection closed: %s:%s", *client_address)


def main():

    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server.bind((LISTEN_HOST, LISTEN_PORT))
    server.listen(50)

    logging.info("Docker API Proxy listening on %s:%s", LISTEN_HOST, LISTEN_PORT)

    try:
        while True:

            client_socket, client_address = server.accept()

            threading.Thread(
                target=handle_client,
                args=(client_socket, client_address),
                daemon=True
            ).start()

    except KeyboardInterrupt:
        logging.info("Proxy shutting down")

    finally:
        server.close()


if __name__ == "__main__":
    main()
