#!/bin/bash

# Backup Script for Steel Designer Engineer
# Usage: ./backup.sh

set -e

# Configuration
BACKUP_ROOT="/opt/backups"
TIMESTAMP=$(date +%Y%m%d-%H%M%S)
BACKUP_DIR="${BACKUP_ROOT}/steel-designer-${TIMESTAMP}"
DOCKER_VOLUME_PATH="/var/lib/docker/volumes"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${BLUE}?? Starting backup...${NC}"
echo "Backup directory: ${BACKUP_DIR}"
echo ""

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Backup Docker image
echo -e "${YELLOW}?? Backing up Docker image...${NC}"
CURRENT_IMAGE=$(docker inspect steel-designer-engineer-api --format='{{.Config.Image}}' 2>/dev/null || echo "")
if [ -n "$CURRENT_IMAGE" ]; then
    docker save "$CURRENT_IMAGE" -o "${BACKUP_DIR}/webapi-image.tar"
    echo -e "${GREEN}? Docker image backed up${NC}"
else
    echo -e "${RED}??  No running container found${NC}"
fi

# Backup MongoDB data
echo -e "${YELLOW}?? Backing up MongoDB...${NC}"
if docker ps --filter "name=steel-designer-mongodb" --format "{{.Names}}" | grep -q "steel-designer-mongodb"; then
    docker exec steel-designer-mongodb mongodump --out=/tmp/mongodb-backup --quiet
    docker cp steel-designer-mongodb:/tmp/mongodb-backup "${BACKUP_DIR}/mongodb"
    docker exec steel-designer-mongodb rm -rf /tmp/mongodb-backup
    echo -e "${GREEN}? MongoDB backed up${NC}"
else
    echo -e "${YELLOW}??  MongoDB container not running${NC}"
fi

# Backup Redis data
echo -e "${YELLOW}?? Backing up Redis...${NC}"
if docker ps --filter "name=steel-designer-redis" --format "{{.Names}}" | grep -q "steel-designer-redis"; then
    docker exec steel-designer-redis redis-cli --no-auth-warning BGSAVE
    sleep 2
    docker cp steel-designer-redis:/data/dump.rdb "${BACKUP_DIR}/redis-dump.rdb"
    echo -e "${GREEN}? Redis backed up${NC}"
else
    echo -e "${YELLOW}??  Redis container not running${NC}"
fi

# Backup RabbitMQ data
echo -e "${YELLOW}?? Backing up RabbitMQ...${NC}"
if docker ps --filter "name=steel-designer-rabbitmq" --format "{{.Names}}" | grep -q "steel-designer-rabbitmq"; then
    docker cp steel-designer-rabbitmq:/var/lib/rabbitmq "${BACKUP_DIR}/rabbitmq"
    echo -e "${GREEN}? RabbitMQ backed up${NC}"
else
    echo -e "${YELLOW}??  RabbitMQ container not running${NC}"
fi

# Backup configuration files
echo -e "${YELLOW}??  Backing up configuration files...${NC}"
if [ -f "/opt/steel-designer-engineer/.env" ]; then
    cp /opt/steel-designer-engineer/.env "${BACKUP_DIR}/.env"
    echo -e "${GREEN}? Configuration files backed up${NC}"
fi

if [ -f "/opt/steel-designer-engineer/docker-compose.yml" ]; then
    cp /opt/steel-designer-engineer/docker-compose.yml "${BACKUP_DIR}/docker-compose.yml"
fi

# Create backup info file
cat > "${BACKUP_DIR}/backup-info.txt" <<EOF
Backup Information
==================
Date: $(date)
Hostname: $(hostname)
Docker Image: ${CURRENT_IMAGE}
Containers: $(docker ps --format "{{.Names}}" | grep steel-designer | tr '\n' ', ')

Backup Contents:
- Docker image: webapi-image.tar
- MongoDB data: mongodb/
- Redis data: redis-dump.rdb
- RabbitMQ data: rabbitmq/
- Configuration: .env, docker-compose.yml
EOF

# Compress backup
echo -e "${YELLOW}?? Compressing backup...${NC}"
cd "$BACKUP_ROOT"
tar -czf "steel-designer-${TIMESTAMP}.tar.gz" "steel-designer-${TIMESTAMP}"
rm -rf "steel-designer-${TIMESTAMP}"

BACKUP_SIZE=$(du -h "steel-designer-${TIMESTAMP}.tar.gz" | cut -f1)
echo -e "${GREEN}? Backup compressed (${BACKUP_SIZE})${NC}"

# Clean old backups (keep last 7 days)
echo -e "${YELLOW}?? Cleaning old backups...${NC}"
find "$BACKUP_ROOT" -name "steel-designer-*.tar.gz" -type f -mtime +7 -delete
BACKUP_COUNT=$(find "$BACKUP_ROOT" -name "steel-designer-*.tar.gz" -type f | wc -l)
echo -e "${GREEN}? Kept ${BACKUP_COUNT} most recent backups${NC}"

echo ""
echo -e "${GREEN}? Backup completed successfully!${NC}"
echo -e "${BLUE}Backup location: ${BACKUP_ROOT}/steel-designer-${TIMESTAMP}.tar.gz${NC}"
echo -e "${BLUE}Backup size: ${BACKUP_SIZE}${NC}"

# List all backups
echo ""
echo -e "${BLUE}?? Available backups:${NC}"
ls -lh "$BACKUP_ROOT"/steel-designer-*.tar.gz 2>/dev/null || echo "No backups found"
