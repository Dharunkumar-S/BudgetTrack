# 🐳 Docker Basics — Complete Reference Guide

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Docker Commands](#2-docker-commands)
3. [Docker Run](#3-docker-run)
4. [Docker Images](#4-docker-images)
5. [Docker Compose](#5-docker-compose)
6. [Docker Registry](#6-docker-registry)
7. [Docker Engine](#7-docker-engine)
8. [Docker Storage](#8-docker-storage)
9. [Docker Networking](#9-docker-networking)
10. [Container Orchestration](#10-container-orchestration)

---

## 1. Introduction

### 1.1 What is Docker?

Docker is an open-source platform that lets you **build, package, and run applications** inside lightweight, portable units called **containers**. It solves the classic *"it works on my machine"* problem by bundling the app and all its dependencies together.

- Written in Go, launched in 2013
- Uses OS-level virtualization (not full hardware emulation)
- Works on Linux, macOS, and Windows

> 💡 **Key Idea:** A container = your app code + runtime + libraries + config, all in one self-contained unit.

---

### 1.2 What is a Virtual Machine?

A **Virtual Machine (VM)** is a software emulation of a physical computer. It runs a full operating system on top of a **hypervisor** (e.g., VMware, VirtualBox, Hyper-V).

- Each VM includes a full OS (kernel + libraries)
- Heavy on resources — typically GBs in size
- Slow to start — usually takes minutes

---

### 1.3 Docker vs. Virtual Machine

| Feature         | Docker Container   | Virtual Machine     |
|-----------------|--------------------|---------------------|
| OS              | Shares host kernel | Full OS per VM      |
| Size            | MBs                | GBs                 |
| Startup Time    | Seconds            | Minutes             |
| Isolation       | Process-level      | Full hardware       |
| Performance     | Near-native        | Overhead present    |
| Portability     | Very High          | Medium              |

---

### 1.4 Docker Editions

#### Community Edition (CE)
Free and open-source, ideal for developers and small teams.
- `docker-ce` — core engine
- `docker-ce-cli` — command-line interface
- `containerd.io` — container runtime
- Best for: individual developers, open-source projects

#### Enterprise Edition (EE)
Paid edition for large-scale production deployments.
- Includes security scanning, image signing, role-based access control (RBAC)
- Comes with official Docker support
- Best for: enterprises, regulated industries

---

## 2. Docker Commands

### Basic Commands

#### `docker run` — Create and start a container
```bash
docker run nginx
```

#### `docker ps` — List running containers
```bash
docker ps          # running containers
docker ps -a       # all containers (including stopped)
```

#### `docker stop` — Stop a running container
```bash
docker stop <container_id or name>
```

#### `docker rm` — Remove a stopped container
```bash
docker rm <container_id or name>
```

#### `docker images` — List downloaded images
```bash
docker images
```

#### `docker rmi` — Remove an image
```bash
docker rmi nginx
```

#### `docker pull` — Download an image without running it
```bash
docker pull ubuntu
```

---

### Append a Command
Run a container and execute a specific command instead of the default:
```bash
docker run ubuntu sleep 5
docker run ubuntu cat /etc/hosts
```

---

### `docker exec` — Run a command in a running container
```bash
docker exec <container_id> cat /etc/hosts
docker exec -it <container_id> bash   # interactive shell
```

---

## 3. Docker Run

### How to Use `docker run`
```bash
docker run <image_name>
docker run nginx           # pulls nginx if not present, then starts it
```

---

### Run Under a Specific Name
```bash
docker run --name my-web-server nginx
```

---

### Run in Background (Detached Mode)
```bash
docker run -d nginx
# Returns container ID, runs in background
```

---

### Run Interactively
```bash
docker run -it ubuntu bash
# -i = interactive (keep STDIN open)
# -t = allocate a pseudo-TTY (terminal)
```

---

### Publish Container Ports
Map a container port to a host port:
```bash
docker run -p 8080:80 nginx
# host port 8080 → container port 80
```

---

### Remove Container After Process Completes
```bash
docker run --rm ubuntu echo "Hello World"
# Container is automatically deleted after it exits
```

---

## 4. Docker Images

### What is a Docker Image?
A Docker image is a **read-only template** used to create containers. It contains the application code, runtime, libraries, environment variables, and config files.

---

### Image Layers
Docker images are built in **layers**. Each instruction in a Dockerfile creates a new layer. Layers are cached and reused, making builds faster.

```
Layer 4: COPY app files      ← your changes
Layer 3: RUN npm install     ← dependencies
Layer 2: FROM node:18        ← base OS + Node
Layer 1: Scratch/Base OS
```

---

### Container Layer
When you run an image, Docker adds a thin **writable container layer** on top. Changes made inside the container (files, processes) live in this layer only — the image layers remain unchanged.

---

### Parent Image vs Base Image
- **Base Image** — has no parent (e.g., `FROM scratch`, or `FROM ubuntu`)
- **Parent Image** — an image your image is built on top of (e.g., `FROM node:18`)

---

### Docker Manifest
A manifest is a JSON document describing an image — its layers, architecture, OS, and digests. Used to support multi-architecture images (e.g., `linux/amd64`, `linux/arm64`).

---

### Container Registries & Repositories
- **Registry** — a server that stores Docker images (e.g., Docker Hub, AWS ECR, GitHub Container Registry)
- **Repository** — a collection of related images with different tags inside a registry

```
Registry:   hub.docker.com
Repository: nginx
Tag:        nginx:1.25, nginx:latest
```

---

### How to Create a Docker Image

#### Method 1: Interactive Method
```bash
docker run -it ubuntu bash
# Make changes inside the container, then:
docker commit <container_id> my-custom-image
```

#### Method 2: Dockerfile Method (Recommended)
```dockerfile
# Dockerfile
FROM ubuntu
RUN apt-get update && apt-get install -y python3
COPY app.py /app/app.py
CMD ["python3", "/app/app.py"]
```

```bash
docker build -t my-app:1.0 .
```

---

### The Docker Build Context
When you run `docker build`, Docker sends all files in the current directory (the **build context**) to the Docker daemon.

```bash
docker build -t my-app .
#                      ^ this dot = build context (current directory)
```

> 💡 Use a `.dockerignore` file to exclude files (like `node_modules`, `.git`) from the build context.

---

## 5. Docker Compose

### What is Docker Compose?
Docker Compose is a tool for **defining and running multi-container Docker applications** using a single YAML file (`docker-compose.yml`).

---

### Benefits of Docker Compose
- Define all services in one file
- Start everything with a single command
- Manage networking between containers automatically
- Easy to version-control your entire stack

---

### Basic Commands
```bash
docker compose up          # start all services
docker compose up -d       # start in detached (background) mode
docker compose down        # stop and remove containers
docker compose ps          # list running services
docker compose logs        # view logs
docker compose build       # rebuild images
```

---

### Install Docker Compose
Docker Compose v2 is included with **Docker Desktop**. For Linux:
```bash
sudo apt-get install docker-compose-plugin
docker compose version
```

---

### Create the Compose File
```yaml
# docker-compose.yml
version: "3.9"

services:
  web:
    image: nginx
    ports:
      - "8080:80"

  db:
    image: postgres:15
    environment:
      POSTGRES_PASSWORD: secret
    volumes:
      - db-data:/var/lib/postgresql/data

volumes:
  db-data:
```

---

### YAML Configuration File
Key sections in `docker-compose.yml`:

| Key          | Description                                      |
|--------------|--------------------------------------------------|
| `services`   | Define each container (app, db, cache, etc.)     |
| `image`      | Docker image to use                              |
| `build`      | Path to Dockerfile to build locally              |
| `ports`      | Port mappings `host:container`                   |
| `volumes`    | Mount volumes for persistent data                |
| `environment`| Set environment variables                        |
| `depends_on` | Set startup order between services               |
| `networks`   | Custom networks for service communication        |

---

## 6. Docker Registry

### What is it?
A **Docker Registry** is a storage and distribution system for Docker images. It hosts repositories of images that can be pushed, pulled, and managed.

---

### Why Use a Registry?
- Share images across teams and environments
- Version-control your application images
- Enable CI/CD pipelines to pull and deploy images
- Enforce access control and security scanning

---

### Alternatives to Docker Hub

| Registry              | Provider      | Notes                        |
|-----------------------|---------------|------------------------------|
| Docker Hub            | Docker        | Default, free tier available |
| Amazon ECR            | AWS           | Integrates with ECS/EKS      |
| Google Artifact Registry | Google Cloud | Replaces GCR               |
| GitHub Container Registry | GitHub    | Tied to GitHub repos         |
| Azure Container Registry | Microsoft  | Integrates with Azure        |
| Harbor                | Open-source   | Self-hosted, enterprise-grade|

---

### Basic Commands
```bash
# Login to Docker Hub
docker login

# Tag an image for a registry
docker tag my-app:1.0 myusername/my-app:1.0

# Push image to registry
docker push myusername/my-app:1.0

# Pull image from registry
docker pull myusername/my-app:1.0

# Logout
docker logout
```

---

## 7. Docker Engine

### What is Docker Engine?
Docker Engine is the **core client-server application** that makes Docker work. It consists of three main components:

---

### Docker CLI (Command Line Interface)
The tool users interact with directly. Commands like `docker run`, `docker ps`, `docker build` are all sent via the CLI.

```bash
docker run nginx   # CLI sends this request to the Docker Daemon via REST API
```

---

### REST API
The intermediary that allows the Docker CLI (and other tools) to communicate with the Docker Daemon. Any application can use this API to control Docker programmatically.

```
Docker CLI  ──► REST API  ──► Docker Daemon
```

---

### Docker Daemon (`dockerd`)
The **background service** that does all the heavy lifting:
- Builds images
- Runs and manages containers
- Manages networks and volumes
- Communicates with container runtimes (containerd)

```bash
# Check daemon status (Linux)
sudo systemctl status docker
```

> 💡 The CLI and Daemon can run on the **same machine** or on **different machines** (remote Docker).

---

## 8. Docker Storage

### Storage Drivers
Storage drivers control how image layers and the container writable layer are managed on disk.

| Driver     | OS Support      | Notes                        |
|------------|-----------------|------------------------------|
| `overlay2` | Linux (default) | Best performance, recommended|
| `aufs`     | Linux (legacy)  | Older systems                |
| `devicemapper` | Linux      | Block-level storage          |
| `windowsfilter` | Windows   | Default on Windows           |

---

### Data Volumes
Volumes are the **preferred way to persist data** in Docker. They are managed by Docker and stored outside the container's filesystem.

```bash
# Create a volume
docker volume create my-data

# Use a volume when running a container
docker run -v my-data:/var/lib/mysql mysql

# Anonymous volume (Docker names it automatically)
docker run -v /var/lib/mysql mysql

# Bind mount (map a host directory)
docker run -v /home/user/data:/app/data nginx
```

---

### Changing the Storage Driver for a Container
```bash
docker run --storage-opt size=10G ubuntu
```

To change the default driver system-wide, edit `/etc/docker/daemon.json`:
```json
{
  "storage-driver": "overlay2"
}
```

---

### Creating a Volume
```bash
docker volume create my-volume
```

---

### Listing All Volumes
```bash
docker volume ls

# Inspect a volume
docker volume inspect my-volume

# Remove a volume
docker volume rm my-volume

# Remove all unused volumes
docker volume prune
```

---

## 9. Docker Networking

### Default Networks
Docker creates three networks automatically:

| Network   | Driver  | Description                                       |
|-----------|---------|---------------------------------------------------|
| `bridge`  | bridge  | Default for containers on the same host           |
| `host`    | host    | Container shares the host's network stack         |
| `none`    | null    | No network access                                 |

```bash
docker run --network bridge nginx    # default
docker run --network host nginx      # uses host network directly
docker run --network none nginx      # completely isolated
```

---

### Listing All Docker Networks
```bash
docker network ls
```

---

### Inspecting a Docker Network
```bash
docker network inspect bridge
# Shows: subnet, gateway, connected containers, config
```

---

### Creating Your Own Network
```bash
# Create a custom bridge network
docker network create my-network

# Run containers on the custom network
docker run -d --network my-network --name app1 nginx
docker run -d --network my-network --name app2 nginx

# app1 and app2 can now reach each other by name:
# ping app2   (from inside app1)
```

> 💡 Containers on the **same custom network** can communicate using their **container names** as hostnames — no IP addresses needed.

```bash
# Remove a network
docker network rm my-network
```

---

## 10. Container Orchestration

### What is Container Orchestration?
Container orchestration is the **automated management of containerized applications** across multiple hosts. It handles deployment, scaling, networking, and availability of containers.

---

### Why Do We Need It?
Running a few containers manually is easy. But in production:
- You may have **hundreds or thousands** of containers
- Containers can **crash** and need to be restarted automatically
- Load needs to be **distributed** across multiple servers
- Rolling updates need to happen **without downtime**
- You need **service discovery** so containers find each other

---

### Benefits of Container Orchestration

| Benefit              | Description                                           |
|----------------------|-------------------------------------------------------|
| Auto-scaling         | Scale up/down based on traffic or resource usage      |
| Self-healing         | Restart failed containers automatically               |
| Load balancing       | Distribute traffic across healthy container instances |
| Rolling updates      | Deploy new versions with zero downtime                |
| Service discovery    | Containers automatically find and talk to each other  |
| Resource management  | Optimally schedule containers across cluster nodes    |

---

### What is Kubernetes?
**Kubernetes (K8s)** is the most popular container orchestration platform, originally developed by Google and now maintained by the CNCF.

Key Kubernetes concepts:

| Concept       | Description                                              |
|---------------|----------------------------------------------------------|
| **Pod**       | Smallest unit — one or more containers running together  |
| **Node**      | A physical or virtual machine in the cluster             |
| **Cluster**   | A set of nodes managed by Kubernetes                     |
| **Deployment**| Manages replica sets and rolling updates                 |
| **Service**   | Exposes pods to the network (internal or external)       |
| **Namespace** | Logical isolation within a cluster                       |

```bash
# Basic kubectl commands
kubectl get pods
kubectl get nodes
kubectl apply -f deployment.yaml
kubectl scale deployment my-app --replicas=5
```

---

### Container Orchestration vs Docker

| Aspect          | Docker (Standalone)        | Orchestration (e.g., Kubernetes)     |
|-----------------|----------------------------|--------------------------------------|
| Scale           | Single host                | Multi-host cluster                   |
| Auto-healing    | No                         | Yes — restarts failed containers     |
| Load balancing  | Manual                     | Built-in                             |
| Rolling updates | Manual                     | Automated                            |
| Use case        | Dev / small deployments    | Production / large-scale systems     |

> 💡 **Docker Swarm** is Docker's own built-in orchestration tool — simpler than Kubernetes but less feature-rich. For production at scale, **Kubernetes** is the industry standard.

---

*End of Docker Basics Reference Guide*
