#!/bin/bash

# Health Check Script for Steel Designer Engineer
# Usage: ./health-check.sh [host] [port]

HOST=${1:-localhost}
PORT=${2:-5000}
BASE_URL="http://${HOST}:${PORT}"

echo "?? Starting health check for Steel Designer Engineer..."
echo "?? Target: ${BASE_URL}"
echo ""

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to check endpoint
check_endpoint() {
    local endpoint=$1
    local description=$2
    
    echo -n "Checking ${description}... "
    
    response=$(curl -s -o /dev/null -w "%{http_code}" "${BASE_URL}${endpoint}" --max-time 10)
    
    if [ "$response" = "200" ] || [ "$response" = "302" ]; then
        echo -e "${GREEN}? OK${NC} (HTTP ${response})"
        return 0
    else
        echo -e "${RED}? FAILED${NC} (HTTP ${response})"
        return 1
    fi
}

# Function to check Docker container
check_docker() {
    local container=$1
    local description=$2
    
    echo -n "Checking ${description}... "
    
    if docker ps --filter "name=${container}" --format "{{.Status}}" | grep -q "Up"; then
        echo -e "${GREEN}? Running${NC}"
        return 0
    else
        echo -e "${RED}? Not running${NC}"
        return 1
    fi
}

# Check if running in Docker environment
if command -v docker &> /dev/null; then
    echo "?? Docker Environment Detected"
    echo "================================"
    
    check_docker "steel-designer-engineer-api" "WebAPI Container"
    check_docker "steel-designer-mongodb" "MongoDB Container"
    check_docker "steel-designer-redis" "Redis Container"
    check_docker "steel-designer-rabbitmq" "RabbitMQ Container"
    
    echo ""
fi

# Check WebAPI endpoints
echo "?? Checking WebAPI Endpoints"
echo "================================"

failed=0

check_endpoint "/swagger/index.html" "Swagger UI" || ((failed++))
check_endpoint "/index.html" "Main Page" || ((failed++))
check_endpoint "/login.html" "Login Page" || ((failed++))

echo ""

# Summary
echo "?? Health Check Summary"
echo "================================"

if [ $failed -eq 0 ]; then
    echo -e "${GREEN}? All checks passed!${NC}"
    exit 0
else
    echo -e "${RED}? ${failed} check(s) failed${NC}"
    exit 1
fi
