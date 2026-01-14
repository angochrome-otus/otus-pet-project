#!/bin/bash

# Make all scripts executable
# Run this script after cloning the repository

echo "?? Making deployment scripts executable..."

chmod +x scripts/deploy.sh
chmod +x scripts/backup.sh
chmod +x scripts/rollback.sh
chmod +x scripts/health-check.sh

echo "? Done! All scripts are now executable."
echo ""
echo "Available scripts:"
echo "  ./scripts/deploy.sh [staging|production] [version]"
echo "  ./scripts/backup.sh"
echo "  ./scripts/rollback.sh [backup-file]"
echo "  ./scripts/health-check.sh [host] [port]"
