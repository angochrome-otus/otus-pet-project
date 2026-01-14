# GitHub Actions Workflows

This directory contains CI/CD workflows for the Steel Designer Engineer project.

## ?? Workflows

### 1. CI - Build and Test (`ci-build-test.yml`)
**Purpose:** Continuous Integration - Build, test, and validate code quality

**Triggers:**
- Push to `developer_Nikita`, `main`, `develop` branches
- Pull requests to these branches
- Manual workflow dispatch

**Jobs:**
- **build-and-test**: Builds all .NET projects, runs tests, creates artifacts
- **code-quality**: Performs code analysis and security scans

**Artifacts:**
- `webapi-build` - Compiled application (7 days retention)
- `test-results` - Unit test results
- `security-scan` - Vulnerability scan report

---

### 2. CD - Docker Build and Push (`cd-docker.yml`)
**Purpose:** Build and publish Docker images to container registries

**Triggers:**
- Push to `main`, `develop` branches
- New version tags (e.g., `v1.0.0`)
- Manual workflow dispatch

**Jobs:**
- **docker-build-push**: Multi-architecture build and push
- **docker-compose-test**: Integration testing with Docker Compose

**Features:**
- Multi-platform support (linux/amd64, linux/arm64)
- Layer caching for faster builds
- Security scanning with Trivy
- Pushes to GitHub Container Registry and Docker Hub

**Image Tags:**
- `latest` - Latest from main branch
- `develop` - Latest from develop branch
- `v1.0.0` - Semantic version tags
- `main-abc123` - Branch + commit SHA

---

### 3. CD - Deploy (`cd-deploy.yml`)
**Purpose:** Deploy application to staging/production environments

**Triggers:**
- Manual workflow dispatch (with environment selection)
- Push to version tags

**Jobs:**
- **deploy-staging**: Deploy to staging environment
- **deploy-production**: Deploy to production with rollback support

**Features:**
- Zero-downtime rolling updates
- Automatic backup before production deployment
- Health checks and smoke tests
- Automatic rollback on failure
- GitHub release creation for version tags

**Environments:**
- `staging` - https://staging.steel-designer-engineer.com
- `production` - https://steel-designer-engineer.com

---

## ?? Required Secrets

Configure these in **Settings ? Secrets and variables ? Actions**

### Docker Registry
```
DOCKERHUB_USERNAME
DOCKERHUB_TOKEN
```

### Staging Environment
```
STAGING_HOST
STAGING_USER
STAGING_SSH_KEY
STAGING_PORT (optional, default: 22)
```

### Production Environment
```
PROD_HOST
PROD_USER
PROD_SSH_KEY
PROD_PORT (optional, default: 22)
```

### Application Configuration
```
MONGODB_CONNECTION_STRING
MONGODB_DATABASE
REDIS_CONNECTION_STRING
RABBITMQ_HOST
RABBITMQ_USERNAME
RABBITMQ_PASSWORD
GOOGLE_CLIENT_ID
GOOGLE_CLIENT_SECRET
GITHUB_CLIENT_ID
GITHUB_CLIENT_SECRET
```

---

## ?? Usage Examples

### Trigger CI Build
```bash
# Automatically triggered on push
git push origin developer_Nikita

# Or manually in GitHub UI:
Actions ? CI - Build and Test ? Run workflow
```

### Build Docker Image
```bash
# Push to main branch
git checkout main
git push origin main

# Or create version tag
git tag -a v1.0.0 -m "Release 1.0.0"
git push origin v1.0.0
```

### Deploy to Staging
```bash
Actions ? Deploy to Production ? Run workflow
Environment: staging
Version: latest
```

### Deploy to Production
```bash
# Option 1: Create version tag (recommended)
git tag -a v1.0.0 -m "Production release 1.0.0"
git push origin v1.0.0

# Option 2: Manual trigger
Actions ? Deploy to Production ? Run workflow
Environment: production
Version: v1.0.0
```

---

## ?? Status Badges

Add these to your README.md:

```markdown
![CI Build](https://github.com/angochrome-otus/otus-pet-project/workflows/CI%20-%20Build%20and%20Test/badge.svg)
![Docker Build](https://github.com/angochrome-otus/otus-pet-project/workflows/CD%20-%20Docker%20Build%20and%20Push/badge.svg)
```

---

## ?? Customization

### Modify .NET Version
Edit the `DOTNET_VERSION` in workflow files:
```yaml
env:
  DOTNET_VERSION: '10.0.x'
```

### Change Docker Registry
Update the `REGISTRY` and image names:
```yaml
env:
  REGISTRY: ghcr.io
  DOCKER_IMAGE_NAME: steel-designer-engineer
```

### Add More Tests
Add test projects and they'll be automatically discovered:
```bash
dotnet new xunit -n SteelDesignerEngineer.Tests
```

### Customize Deployment
Modify the SSH commands in `cd-deploy.yml`:
```yaml
script: |
  cd /opt/steel-designer-engineer
  docker-compose pull
  docker-compose up -d
```

---

## ?? Troubleshooting

### Workflow Fails
1. Check the **Actions** tab for detailed logs
2. Click on the failed job to see error messages
3. Fix the issue and push again

### Docker Build Fails
- Verify Dockerfile syntax
- Check all project files are included
- Test locally: `docker build -t test .`

### Deployment Fails
- Verify SSH connection: `ssh user@server`
- Check server disk space: `df -h`
- Review server logs: `docker logs steel-designer-engineer-api`

### Secrets Not Working
- Ensure secret names match exactly (case-sensitive)
- Verify secrets are set in repository settings
- Check secret values don't have extra spaces

---

## ?? Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Docker Build Push Action](https://github.com/docker/build-push-action)
- [SSH Action](https://github.com/appleboy/ssh-action)
- [CICD_SETUP_GUIDE.md](../CICD_SETUP_GUIDE.md) - Complete setup guide

---

## ?? Workflow Updates

To update workflows:
1. Edit workflow files in `.github/workflows/`
2. Commit and push changes
3. Workflows will use the new version on next run

**Note:** Workflow changes only affect new runs, not in-progress runs.

---

**Last Updated:** 2024
**Maintainer:** DevOps Team
