#!/bin/bash

# Rollback Script for Steel Designer Engineer
# Usage: ./rollback.sh [backup-file]
# Example: ./rollback.sh /opt/backups/steel-designer-20240115-143022.tar.gz

set -e

BACKUP_FILE=$1
BACKUP_ROOT="/opt/backups"
DEPLOY_DIR="/opt/steel-designer-engineer"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

echo -e "${YELLOW}??  Starting rollback...${NC}"
echo ""

# If no backup file specified, use the latest
if [ -z "$BACKUP_FILE" ]; then
    echo -e "${BLUE}?? Finding latest backup...${NC}"
    BACKUP_FILE=$(ls -t "$BACKUP_ROOT"/steel-designer-*.tar.gz 2>/dev/null | head -1)
    
    if [ -z "$BACKUP_FILE" ]; then
        echo -e "${RED}? No backup files found in ${BACKUP_ROOT}${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}? Found: ${BACKUP_FILE}${NC}"
fi

# Validate backup file
if [ ! -f "$BACKUP_FILE" ]; then
    echo -e "${RED}? Backup file not found: ${BACKUP_FILE}${NC}"
    exit 1
fi

echo -e "${BLUE}?? Backup file: ${BACKUP_FILE}${NC}"
echo ""

# Confirm rollback
echo -e "${YELLOW}??  This will restore the system to a previous state.${NC}"
echo -e "${YELLOW}   Current data will be replaced!${NC}"
read -p "Are you sure you want to continue? (yes/no): " -r
echo ""

if [[ ! $REPLY =~ ^[Yy][Ee][Ss]$ ]]; then
    echo -e "${RED}? Rollback cancelled${NC}"
    exit 1
fi

# Create temporary directory
TEMP_DIR=$(mktemp -d)
echo -e "${BLUE}?? Extracting backup to ${TEMP_DIR}...${NC}"
tar -xzf "$BACKUP_FILE" -C "$TEMP_DIR"

BACKUP_NAME=$(basename "$BACKUP_FILE" .tar.gz)
BACKUP_EXTRACT_DIR="${TEMP_DIR}/${BACKUP_NAME}"

if [ ! -d "$BACKUP_EXTRACT_DIR" ]; then
    echo -e "${RED}? Invalid backup structure${NC}"
    rm -rf "$TEMP_DIR"
    exit 1
fi

# Stop current containers
echo -e "${YELLOW}?? Stopping current containers...${NC}"
cd "$DEPLOY_DIR"
docker-compose down 2>/dev/null || true

# Restore Docker image
echo -e "${BLUE}?? Restoring Docker image...${NC}"
if [ -f "${BACKUP_EXTRACT_DIR}/webapi-image.tar" ]; then
    docker load -i "${BACKUP_EXTRACT_DIR}/webapi-image.tar"
    echo -e "${GREEN}? Docker image restored${NC}"
else
    echo -e "${RED}??  No Docker image found in backup${NC}"
fi

# Restore configuration files
echo -e "${BLUE}??  Restoring configuration...${NC}"
if [ -f "${BACKUP_EXTRACT_DIR}/.env" ]; then
    cp "${BACKUP_EXTRACT_DIR}/.env" "${DEPLOY_DIR}/.env"
    echo -e "${GREEN}? Environment file restored${NC}"
fi

if [ -f "${BACKUP_EXTRACT_DIR}/docker-compose.yml" ]; then
    cp "${BACKUP_EXTRACT_DIR}/docker-compose.yml" "${DEPLOY_DIR}/docker-compose.yml"
    echo -e "${GREEN}? Docker Compose file restored${NC}"
fi

# Start containers (without data restore first)
echo -e "${BLUE}?? Starting containers...${NC}"
docker-compose up -d

# Wait for containers to be ready
echo -e "${BLUE}? Waiting for containers to start...${NC}"
sleep 20

# Restore MongoDB
echo -e "${BLUE}?? Restoring MongoDB...${NC}"
if [ -d "${BACKUP_EXTRACT_DIR}/mongodb" ]; then
    docker cp "${BACKUP_EXTRACT_DIR}/mongodb" steel-designer-mongodb:/tmp/mongodb-restore
    docker exec steel-designer-mongodb mongorestore /tmp/mongodb-restore --drop --quiet
    docker exec steel-designer-mongodb rm -rf /tmp/mongodb-restore
    echo -e "${GREEN}? MongoDB restored${NC}"
else
    echo -e "${YELLOW}??  No MongoDB backup found${NC}"
fi

# Restore Redis
echo -e "${BLUE}?? Restoring Redis...${NC}"
if [ -f "${BACKUP_EXTRACT_DIR}/redis-dump.rdb" ]; then
    docker cp "${BACKUP_EXTRACT_DIR}/redis-dump.rdb" steel-designer-redis:/data/dump.rdb
    docker restart steel-designer-redis
    sleep 5
    echo -e "${GREEN}? Redis restored${NC}"
else
    echo -e "${YELLOW}??  No Redis backup found${NC}"
fi

# Restart all containers to ensure data is loaded
echo -e "${BLUE}?? Restarting all containers...${NC}"
docker-compose restart

# Wait for health check
echo -e "${BLUE}? Waiting for services to be healthy...${NC}"
sleep 15

# Verify rollback
echo -e "${BLUE}?? Verifying rollback...${NC}"
if docker ps | grep -q "steel-designer-engineer-api"; then
    HEALTH_STATUS=$(docker inspect steel-designer-engineer-api --format='{{.State.Health.Status}}' 2>/dev/null || echo "unknown")
    
    if [ "$HEALTH_STATUS" = "healthy" ]; then
        echo -e "${GREEN}? Service is healthy${NC}"
    else
        echo -e "${YELLOW}??  Service health status: ${HEALTH_STATUS}${NC}"
    fi
else
    echo -e "${RED}? Container is not running${NC}"
    rm -rf "$TEMP_DIR"
    exit 1
fi

# Cleanup
rm -rf "$TEMP_DIR"

echo ""
echo -e "${GREEN}? Rollback completed successfully!${NC}"
echo -e "${BLUE}?? Restored from: ${BACKUP_FILE}${NC}"
echo ""
echo -e "${BLUE}?? Container status:${NC}"
docker ps --filter "name=steel-designer"
echo ""
echo -e "${BLUE}?? View logs:${NC}"
echo "  docker logs steel-designer-engineer-api -f"
echo ""
echo -e "${BLUE}?? Health check:${NC}"
echo "  curl http://localhost:5000/swagger/index.html"
