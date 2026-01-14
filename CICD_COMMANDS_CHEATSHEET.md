# ?? CI/CD Quick Commands Cheat Sheet

## ?? Git & Deployment

### Trigger CI Build
```bash
git add .
git commit -m "feat: your changes"
git push origin developer_Nikita
```

### Create Release Tag
```bash
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

### Check Build Status
```bash
# Go to GitHub
https://github.com/angochrome-otus/otus-pet-project/actions
```

---

## ?? Docker Commands

### Build Image Locally
```bash
docker build -t steel-designer-engineer:local .
```

### Run Container
```bash
docker run -p 5000:80 steel-designer-engineer:local
```

### Check Running Containers
```bash
docker ps
```

### View Logs
```bash
docker logs steel-designer-engineer-api -f
```

### Stop Container
```bash
docker stop steel-designer-engineer-api
```

### Remove Container
```bash
docker rm steel-designer-engineer-api
```

### Clean Up
```bash
docker system prune -a
```

---

## ?? Docker Compose

### Start All Services
```bash
docker-compose up -d
```

### Stop All Services
```bash
docker-compose down
```

### Restart Services
```bash
docker-compose restart
```

### View Logs
```bash
docker-compose logs -f
docker-compose logs -f webapi  # specific service
```

### Pull Latest Images
```bash
docker-compose pull
```

### Rebuild and Start
```bash
docker-compose up -d --build
```

---

## ?? Health Checks

### Linux/Mac
```bash
./scripts/health-check.sh
./scripts/health-check.sh localhost 5000
```

### Windows PowerShell
```powershell
.\scripts\health-check.ps1
.\scripts\health-check.ps1 localhost 5000
```

### Manual Check
```bash
curl http://localhost:5000/swagger/index.html
curl http://localhost:5000/
```

---

## ?? Deployment Scripts

### Deploy to Server
```bash
# Make scripts executable first (Linux/Mac)
chmod +x scripts/*.sh

# Deploy
./scripts/deploy.sh staging latest
./scripts/deploy.sh production v1.0.0
```

### Create Backup
```bash
./scripts/backup.sh
```

### Rollback
```bash
./scripts/rollback.sh
./scripts/rollback.sh /opt/backups/steel-designer-20240115-143022.tar.gz
```

---

## ?? Monitoring & Debugging

### Check Container Status
```bash
docker ps -a
docker inspect steel-designer-engineer-api
```

### Check Container Health
```bash
docker inspect --format='{{.State.Health.Status}}' steel-designer-engineer-api
```

### View Container Resources
```bash
docker stats
docker stats steel-designer-engineer-api
```

### Execute Command in Container
```bash
docker exec -it steel-designer-engineer-api bash
docker exec -it steel-designer-engineer-api sh
```

### View All Images
```bash
docker images
```

### View Volumes
```bash
docker volume ls
docker volume inspect steel-designer-engineer_mongodb-data
```

---

## ??? Database Commands

### MongoDB
```bash
# Connect to MongoDB
docker exec -it steel-designer-mongodb mongosh

# Backup MongoDB
docker exec steel-designer-mongodb mongodump --out=/tmp/backup

# Restore MongoDB
docker exec steel-designer-mongodb mongorestore /tmp/backup
```

### Redis
```bash
# Connect to Redis
docker exec -it steel-designer-redis redis-cli

# Check Redis data
docker exec steel-designer-redis redis-cli KEYS "*"

# Flush Redis cache
docker exec steel-designer-redis redis-cli FLUSHALL
```

### RabbitMQ
```bash
# Access Management UI
http://localhost:15672
# Default credentials: guest/guest

# Check queues
docker exec steel-designer-rabbitmq rabbitmqctl list_queues
```

---

## ??? .NET Commands

### Restore Packages
```bash
dotnet restore SteelDesignerEngineer/SteelDesignerEngineer.csproj
```

### Build Project
```bash
dotnet build SteelDesignerEngineer/SteelDesignerEngineer.csproj
```

### Run Tests
```bash
dotnet test
```

### Run Application
```bash
dotnet run --project SteelDesignerEngineer/SteelDesignerEngineer.csproj
```

### Publish Application
```bash
dotnet publish SteelDesignerEngineer/SteelDesignerEngineer.csproj -c Release -o ./publish
```

### Check for Vulnerable Packages
```bash
dotnet list SteelDesignerEngineer/SteelDesignerEngineer.csproj package --vulnerable
```

### Update Package
```bash
dotnet add SteelDesignerEngineer/SteelDesignerEngineer.csproj package PackageName
```

---

## ?? GitHub Secrets

### View Secrets (via GitHub UI)
```
Settings ? Secrets and variables ? Actions
```

### Required Secrets
```bash
# Docker Hub
DOCKERHUB_USERNAME=your-username
DOCKERHUB_TOKEN=your-token

# Staging
STAGING_HOST=staging-server-ip
STAGING_USER=deploy
STAGING_SSH_KEY=<private-key>

# Production
PROD_HOST=prod-server-ip
PROD_USER=deploy
PROD_SSH_KEY=<private-key>
```

---

## ?? Server Commands

### SSH to Server
```bash
ssh user@server-ip
```

### Check Server Disk Space
```bash
df -h
```

### Check Server Memory
```bash
free -h
```

### Check Running Services
```bash
systemctl status docker
docker ps
```

### View Server Logs
```bash
tail -f /var/log/syslog
journalctl -u docker.service -f
```

### Cleanup Docker on Server
```bash
docker system prune -a
docker volume prune
```

---

## ?? Nginx Commands

### Test Nginx Config
```bash
docker exec steel-designer-nginx nginx -t
```

### Reload Nginx
```bash
docker exec steel-designer-nginx nginx -s reload
```

### View Nginx Logs
```bash
docker logs steel-designer-nginx -f
```

---

## ?? Testing Commands

### Test Endpoint
```bash
curl http://localhost:5000/swagger/index.html
curl -I http://localhost:5000/
```

### Load Test (with Apache Bench)
```bash
ab -n 1000 -c 10 http://localhost:5000/
```

### Check SSL Certificate
```bash
openssl s_client -connect yourdomain.com:443
```

---

## ?? GitHub Actions

### Manual Workflow Trigger
```
GitHub ? Actions ? Select Workflow ? Run workflow
```

### View Workflow Runs
```
GitHub ? Actions ? All workflows
```

### Download Artifacts
```
GitHub ? Actions ? Select Run ? Artifacts section
```

### Cancel Running Workflow
```
GitHub ? Actions ? Select Run ? Cancel workflow
```

---

## ?? Environment Variables

### Create .env from Template
```bash
cp .env.template .env
nano .env  # or vim, code, etc.
```

### Load Environment Variables
```bash
# Linux/Mac
export $(cat .env | xargs)

# Windows PowerShell
Get-Content .env | ForEach-Object {
    $name, $value = $_.split('=')
    Set-Content env:\$name $value
}
```

---

## ?? Emergency Commands

### Quick Rollback
```bash
./scripts/rollback.sh
```

### Force Stop Everything
```bash
docker-compose down -v
docker stop $(docker ps -aq)
docker rm $(docker ps -aq)
```

### Check What Went Wrong
```bash
docker logs steel-designer-engineer-api --tail 100
docker-compose logs --tail 100
```

### Restart Everything Fresh
```bash
docker-compose down -v
docker-compose up -d
```

---

## ?? Pro Tips

### Watch Logs in Real-Time
```bash
docker-compose logs -f | grep ERROR
```

### Find Container IP
```bash
docker inspect -f '{{range.NetworkSettings.Networks}}{{.IPAddress}}{{end}}' steel-designer-engineer-api
```

### Copy Files from Container
```bash
docker cp steel-designer-engineer-api:/app/logs/ ./local-logs/
```

### Copy Files to Container
```bash
docker cp ./local-file.txt steel-designer-engineer-api:/app/
```

### Check Port Usage
```bash
# Linux
netstat -tulpn | grep 5000

# Mac
lsof -i :5000

# Windows
netstat -ano | findstr :5000
```

---

## ?? Useful URLs

### Local Development
```
http://localhost:5000              # WebAPI
http://localhost:5000/swagger      # Swagger UI
http://localhost:15672             # RabbitMQ Management
```

### GitHub
```
https://github.com/angochrome-otus/otus-pet-project
https://github.com/angochrome-otus/otus-pet-project/actions
https://github.com/angochrome-otus/otus-pet-project/settings/secrets/actions
```

### Container Registry
```
https://ghcr.io/angochrome-otus/steel-designer-engineer
```

---

**?? Save this file for quick reference!**

**Last Updated:** 2024
