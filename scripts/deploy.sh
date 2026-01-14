#!/bin/bash

# Deployment Script for Steel Designer Engineer
# Usage: ./deploy.sh [environment] [version]
# Example: ./deploy.sh staging latest
#          ./deploy.sh production v1.0.0

set -e

# Configuration
ENVIRONMENT=${1:-staging}
VERSION=${2:-latest}
IMAGE_NAME="ghcr.io/angochrome-otus/steel-designer-engineer:${VERSION}"
DEPLOY_DIR="/opt/steel-designer-engineer"
BACKUP_DIR="/opt/backups"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}?? Starting deployment...${NC}"
echo "Environment: ${ENVIRONMENT}"
echo "Version: ${VERSION}"
echo "Image: ${IMAGE_NAME}"
echo ""

# Validate environment
if [ "$ENVIRONMENT" != "staging" ] && [ "$ENVIRONMENT" != "production" ]; then
    echo -e "${RED}? Invalid environment. Use 'staging' or 'production'${NC}"
    exit 1
fi

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}? Docker is not running${NC}"
    exit 1
fi

# Navigate to deployment directory
if [ ! -d "$DEPLOY_DIR" ]; then
    echo -e "${YELLOW}??  Deploy directory doesn't exist. Creating...${NC}"
    mkdir -p "$DEPLOY_DIR"
fi

cd "$DEPLOY_DIR"

# Create backup for production
if [ "$ENVIRONMENT" = "production" ]; then
    echo -e "${YELLOW}?? Creating backup...${NC}"
    TIMESTAMP=$(date +%Y%m%d-%H%M%S)
    BACKUP_PATH="${BACKUP_DIR}/steel-designer-${TIMESTAMP}"
    mkdir -p "$BACKUP_PATH"
    
    # Backup current container image
    CURRENT_IMAGE=$(docker inspect steel-designer-engineer-api --format='{{.Config.Image}}' 2>/dev/null || echo "")
    if [ -n "$CURRENT_IMAGE" ]; then
        docker save "$CURRENT_IMAGE" -o "${BACKUP_PATH}/image.tar"
        echo -e "${GREEN}? Backup created at ${BACKUP_PATH}${NC}"
    fi
fi

# Pull latest image
echo -e "${BLUE}?? Pulling Docker image...${NC}"
docker pull "$IMAGE_NAME"

# Stop and remove old container
echo -e "${YELLOW}?? Stopping old container...${NC}"
docker-compose down 2>/dev/null || true

# Start new container
echo -e "${GREEN}?? Starting new container...${NC}"
docker-compose up -d

# Wait for health check
echo -e "${BLUE}? Waiting for service to be healthy...${NC}"
sleep 10

MAX_RETRIES=30
RETRY_COUNT=0

while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
    HEALTH_STATUS=$(docker inspect steel-designer-engineer-api --format='{{.State.Health.Status}}' 2>/dev/null || echo "unknown")
    
    if [ "$HEALTH_STATUS" = "healthy" ]; then
        echo -e "${GREEN}? Service is healthy!${NC}"
        break
    fi
    
    echo "Health status: $HEALTH_STATUS (attempt $((RETRY_COUNT + 1))/$MAX_RETRIES)"
    sleep 5
    ((RETRY_COUNT++))
done

if [ $RETRY_COUNT -eq $MAX_RETRIES ]; then
    echo -e "${RED}? Service failed to become healthy${NC}"
    echo -e "${YELLOW}?? Container logs:${NC}"
    docker logs steel-designer-engineer-api --tail 50
    
    if [ "$ENVIRONMENT" = "production" ] && [ -n "$BACKUP_PATH" ]; then
        echo -e "${YELLOW}??  Rolling back to backup...${NC}"
        docker load -i "${BACKUP_PATH}/image.tar"
        docker-compose up -d
        echo -e "${GREEN}? Rollback completed${NC}"
    fi
    
    exit 1
fi

# Verify deployment
echo -e "${BLUE}?? Verifying deployment...${NC}"
if docker ps | grep -q "steel-designer-engineer-api"; then
    echo -e "${GREEN}? Container is running${NC}"
else
    echo -e "${RED}? Container is not running${NC}"
    exit 1
fi

# Clean up old images
echo -e "${YELLOW}?? Cleaning up old images...${NC}"
docker image prune -af > /dev/null 2>&1

echo ""
echo -e "${GREEN}? Deployment completed successfully!${NC}"
echo -e "${BLUE}?? Container status:${NC}"
docker ps --filter "name=steel-designer"

echo ""
echo -e "${BLUE}?? View logs:${NC}"
echo "  docker logs steel-designer-engineer-api -f"
echo ""
echo -e "${BLUE}?? Health check:${NC}"
echo "  curl http://localhost:5000/swagger/index.html"
