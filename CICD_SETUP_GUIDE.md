# CI/CD Configuration Guide for Steel Designer Engineer

## ?? Overview

This project includes a complete CI/CD pipeline using GitHub Actions with the following workflows:

1. **CI - Build and Test** (`ci-build-test.yml`) - Continuous Integration
2. **CD - Docker Build and Push** (`cd-docker.yml`) - Container Image Management
3. **CD - Deploy** (`cd-deploy.yml`) - Production Deployment

---

## ?? Quick Start

### Prerequisites

1. GitHub repository with Actions enabled
2. Docker Hub or GitHub Container Registry account
3. Server(s) for deployment (staging/production)

### Required GitHub Secrets

Configure the following secrets in your GitHub repository:
**Settings ? Secrets and variables ? Actions ? New repository secret**

#### Docker Hub (Optional)
```
DOCKERHUB_USERNAME=<your-dockerhub-username>
DOCKERHUB_TOKEN=<your-dockerhub-token>
```

#### Staging Environment
```
STAGING_HOST=<staging-server-ip-or-domain>
STAGING_USER=<ssh-username>
STAGING_SSH_KEY=<private-ssh-key>
STAGING_PORT=22
```

#### Production Environment
```
PROD_HOST=<production-server-ip-or-domain>
PROD_USER=<ssh-username>
PROD_SSH_KEY=<private-ssh-key>
PROD_PORT=22
```

#### Application Secrets
```
MONGODB_CONNECTION_STRING=mongodb://username:password@host:27017
MONGODB_DATABASE=SteelDesignerDb
REDIS_CONNECTION_STRING=redis-host:6379,password=your-password
REDIS_INSTANCE_NAME=SteelDesigner:
RABBITMQ_HOST=rabbitmq-host
RABBITMQ_PORT=5672
RABBITMQ_USERNAME=guest
RABBITMQ_PASSWORD=guest
GOOGLE_CLIENT_ID=<your-google-oauth-client-id>
GOOGLE_CLIENT_SECRET=<your-google-oauth-client-secret>
GITHUB_CLIENT_ID=<your-github-oauth-client-id>
GITHUB_CLIENT_SECRET=<your-github-oauth-client-secret>
```

---

## ?? Workflow Details

### 1. CI - Build and Test

**Triggers:**
- Push to branches: `developer_Nikita`, `main`, `develop`
- Pull requests to these branches
- Manual trigger via GitHub UI

**What it does:**
- ? Restores NuGet dependencies
- ? Builds all .NET projects
- ? Runs unit tests (if available)
- ? Performs code quality analysis
- ? Scans for vulnerable NuGet packages
- ? Uploads build artifacts

**Artifacts:**
- `webapi-build` - Compiled WebAPI application
- `test-results` - Unit test results (if tests exist)
- `security-scan` - Vulnerability scan report

---

### 2. CD - Docker Build and Push

**Triggers:**
- Push to `main` or `develop` branches
- New version tags (e.g., `v1.0.0`)
- Manual trigger via GitHub UI

**What it does:**
- ?? Builds multi-architecture Docker images (amd64, arm64)
- ?? Pushes to GitHub Container Registry
- ?? Optionally pushes to Docker Hub
- ?? Scans images for vulnerabilities (Trivy)
- ? Tests image with Docker Compose

**Image Tags:**
- `latest` - Latest build from main branch
- `develop` - Latest build from develop branch
- `v1.0.0` - Semantic version tags
- `main-abc123` - Branch name + commit SHA

**Example image:**
```
ghcr.io/angochrome-otus/steel-designer-engineer:latest
```

---

### 3. CD - Deploy

**Triggers:**
- Manual trigger with environment selection
- Push to version tags (e.g., `v1.0.0`)

**Deployment Environments:**

#### Staging
- Used for testing before production
- Automatically deployed from `develop` branch
- URL: `https://staging.steel-designer-engineer.com`

#### Production
- Requires staging deployment success
- Creates backup before deployment
- Automatic rollback on failure
- URL: `https://steel-designer-engineer.com`

**Deployment Steps:**
1. ?? Pull latest Docker image
2. ?? Create backup (production only)
3. ?? Deploy with zero-downtime rolling update
4. ? Health check verification
5. ?? Run smoke tests
6. ?? Automatic rollback on failure

---

## ??? Server Setup

### 1. Install Docker on Your Server

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Add user to docker group
sudo usermod -aG docker $USER
```

### 2. Prepare Deployment Directory

```bash
# Create application directory
sudo mkdir -p /opt/steel-designer-engineer
sudo chown $USER:$USER /opt/steel-designer-engineer
cd /opt/steel-designer-engineer

# Create backup directory
sudo mkdir -p /opt/backups
sudo chown $USER:$USER /opt/backups

# Create environment file
cat > .env <<EOF
GITHUB_REPOSITORY_OWNER=angochrome-otus
MONGODB_CONNECTION_STRING=mongodb://mongodb:27017
MONGODB_DATABASE=SteelDesignerDb
REDIS_CONNECTION_STRING=redis:6379
REDIS_INSTANCE_NAME=SteelDesigner:
RABBITMQ_HOST=rabbitmq
RABBITMQ_PORT=5672
RABBITMQ_USERNAME=guest
RABBITMQ_PASSWORD=guest
GOOGLE_CLIENT_ID=your-client-id
GOOGLE_CLIENT_SECRET=your-client-secret
GITHUB_CLIENT_ID=your-client-id
GITHUB_CLIENT_SECRET=your-client-secret
EOF

# Download docker-compose file
wget https://raw.githubusercontent.com/angochrome-otus/otus-pet-project/main/docker-compose.prod.yml -O docker-compose.yml
```

### 3. Setup SSH Access

```bash
# Generate SSH key on your local machine (if not exists)
ssh-keygen -t ed25519 -C "github-actions"

# Copy public key to server
ssh-copy-id -i ~/.ssh/id_ed25519.pub user@your-server

# Test connection
ssh user@your-server

# Copy PRIVATE key content for GitHub Secret
cat ~/.ssh/id_ed25519
# Copy entire output including -----BEGIN and -----END lines
```

---

## ?? Usage Examples

### Manual Build and Test

```bash
# Trigger manually in GitHub UI
Actions ? CI - Build and Test ? Run workflow
```

### Build and Push Docker Image

```bash
# Automatically triggered on push to main/develop
git push origin main

# Or manually trigger
Actions ? CD - Docker Build and Push ? Run workflow
```

### Deploy to Staging

```bash
# Manually trigger deployment
Actions ? Deploy to Production ? Run workflow
# Select: Environment = staging, Version = latest
```

### Deploy to Production

```bash
# Option 1: Create and push version tag
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0

# Option 2: Manual trigger
Actions ? Deploy to Production ? Run workflow
# Select: Environment = production, Version = v1.0.0
```

---

## ?? Monitoring and Logs

### View Build Status

Check build status badges in README (add these):

```markdown
![CI Build](https://github.com/angochrome-otus/otus-pet-project/workflows/CI%20-%20Build%20and%20Test/badge.svg)
![Docker Build](https://github.com/angochrome-otus/otus-pet-project/workflows/CD%20-%20Docker%20Build%20and%20Push/badge.svg)
```

### Check Server Logs

```bash
# SSH to server
ssh user@your-server

# View Docker logs
docker logs steel-designer-engineer-api -f

# View all containers
docker ps -a

# Check health status
docker inspect steel-designer-engineer-api | grep -A 10 Health
```

---

## ?? Security Best Practices

1. **Never commit secrets** to the repository
2. **Use GitHub Secrets** for all sensitive data
3. **Rotate SSH keys** regularly
4. **Enable branch protection** rules for main/develop
5. **Require pull request reviews** before merging
6. **Run security scans** regularly (Trivy, Dependabot)
7. **Use HTTPS** with valid SSL certificates
8. **Enable rate limiting** in Nginx

---

## ?? Troubleshooting

### Build Fails

```bash
# Check logs in GitHub Actions UI
# Common issues:
- Missing dependencies ? Check .csproj files
- Version conflicts ? Update NuGet packages
- Test failures ? Fix failing tests
```

### Docker Push Fails

```bash
# Verify credentials
- Check GITHUB_TOKEN has package:write permission
- Verify DOCKERHUB_USERNAME and DOCKERHUB_TOKEN
```

### Deployment Fails

```bash
# SSH connection issues
- Test SSH manually: ssh user@server
- Check SSH key format in GitHub Secret
- Verify firewall allows SSH (port 22)

# Docker pull fails
- Login to server and test: docker pull image-name
- Check if GITHUB_TOKEN is valid
- Verify image exists in registry

# Service unhealthy
- Check logs: docker logs steel-designer-engineer-api
- Verify environment variables in .env file
- Check dependencies (MongoDB, Redis, RabbitMQ)
```

---

## ?? Next Steps

1. **Add Tests** - Create unit and integration tests
2. **Set up Monitoring** - Add Prometheus + Grafana
3. **Configure Alerts** - Setup notifications for failures
4. **Add Database Migrations** - Automate schema changes
5. **Implement Blue-Green Deployment** - For zero-downtime
6. **Add Performance Tests** - Load testing with k6
7. **Setup Kubernetes** - For advanced orchestration

---

## ?? Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Documentation](https://docs.docker.com/)
- [.NET CI/CD Best Practices](https://learn.microsoft.com/en-us/dotnet/devops/)
- [Nginx Configuration Guide](https://nginx.org/en/docs/)

---

## ?? Contributing

When contributing, ensure:
- All tests pass
- Code follows project standards
- Docker build succeeds
- Documentation is updated

---

## ?? License

This CI/CD configuration is part of the Steel Designer Engineer project.

---

**Last Updated:** 2024
**Maintained By:** DevOps Team
