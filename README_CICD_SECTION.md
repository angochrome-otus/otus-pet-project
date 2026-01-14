# README Update - Add CI/CD Section

## ?? Instructions
Add this section to your main `README.md` file.

---

## ?? CI/CD Pipeline

This project includes a complete CI/CD pipeline for automated building, testing, and deployment.

### Status Badges
![CI Build](https://github.com/angochrome-otus/otus-pet-project/workflows/CI%20-%20Build%20and%20Test/badge.svg)
![Docker Build](https://github.com/angochrome-otus/otus-pet-project/workflows/CD%20-%20Docker%20Build%20and%20Push/badge.svg)
![License](https://img.shields.io/badge/license-MIT-blue.svg)

### Features
- ? **Continuous Integration** - Automatic build and test on every push
- ? **Docker Automation** - Multi-architecture container builds
- ? **Deployment Pipeline** - Automated staging and production deployments
- ? **Zero Downtime** - Rolling updates with automatic rollback
- ? **Security Scanning** - Vulnerability detection in code and dependencies
- ? **Complete Documentation** - 400+ pages of guides and references

### Quick Start

#### 1. Setup (5 minutes)
```bash
# Read the quick start guide
cat QUICK_START_CICD.md

# Configure GitHub Secrets (see documentation)
# Push your code
git push origin developer_Nikita
```

#### 2. Build Locally
```bash
# Build with Docker
docker build -t steel-designer-engineer:local .

# Run locally
docker-compose up -d

# Check health
curl http://localhost:5000/swagger/index.html
```

#### 3. Deploy
```bash
# Automatic deployment via GitHub Actions
# Or manually:
./scripts/deploy.sh staging latest
```

### Documentation

| Document | Purpose | Time |
|----------|---------|------|
| [CICD_INDEX.md](./CICD_INDEX.md) | Documentation hub | 5 min |
| [QUICK_START_CICD.md](./QUICK_START_CICD.md) | Get started | 5 min |
| [CICD_SETUP_GUIDE.md](./CICD_SETUP_GUIDE.md) | Complete guide | 1 hour |
| [CICD_COMMANDS_CHEATSHEET.md](./CICD_COMMANDS_CHEATSHEET.md) | Command reference | As needed |

### Architecture

```
???????????????     ???????????????     ???????????????
?   GitHub    ???????   Actions   ???????   Docker    ?
?   (Source)  ?     ?   (CI/CD)   ?     ?  (Registry) ?
???????????????     ???????????????     ???????????????
                           ?
                           ?
                    ???????????????
                    ?  Production ?
                    ?   Server    ?
                    ???????????????
```

### Workflows

#### CI - Build & Test
- Triggers: Push, Pull Request
- Duration: ~5-8 minutes
- Outputs: Build artifacts, test results, security scan

#### CD - Docker Build
- Triggers: Push to main/develop, version tags
- Duration: ~3-5 minutes
- Outputs: Docker images in GHCR

#### CD - Deploy
- Triggers: Manual, version tags
- Duration: ~2-4 minutes
- Outputs: Deployed application with health checks

### Technology Stack

**CI/CD:**
- GitHub Actions
- Docker & Docker Compose
- Nginx (Reverse Proxy)
- Bash/PowerShell Scripts

**Application:**
- .NET 10.0 (WebAPI)
- MongoDB 7.0
- Redis 7.2
- RabbitMQ 3.12

### Getting Help

- ?? **Documentation:** See [CICD_INDEX.md](./CICD_INDEX.md)
- ?? **Issues:** [GitHub Issues](https://github.com/angochrome-otus/otus-pet-project/issues)
- ?? **Discussions:** [GitHub Discussions](https://github.com/angochrome-otus/otus-pet-project/discussions)

---

## ??? Project Structure

*(Add this to your existing project structure section)*

```
.
??? .github/
?   ??? workflows/          # CI/CD pipelines
??? scripts/                # Deployment scripts
??? nginx/                  # Nginx configuration
??? SteelDesignerEngineer/  # Main WebAPI project
??? docker-compose.yml      # Development compose
??? docker-compose.prod.yml # Production compose
??? CICD_*.md              # CI/CD documentation
```

---

## ?? Deployment

*(Add this new section to your README)*

### Local Development
```bash
docker-compose up -d
```

### Staging Deployment
```bash
./scripts/deploy.sh staging latest
```

### Production Deployment
```bash
# Via GitHub Actions (recommended)
# Create version tag
git tag -a v1.0.0 -m "Release 1.0.0"
git push origin v1.0.0

# Or manually
./scripts/deploy.sh production v1.0.0
```

### Health Monitoring
```bash
# Linux/Mac
./scripts/health-check.sh localhost 5000

# Windows
.\scripts\health-check.ps1 localhost 5000
```

---

## ?? Monitoring & Operations

### View Logs
```bash
docker logs steel-designer-engineer-api -f
```

### Backup
```bash
./scripts/backup.sh
```

### Rollback
```bash
./scripts/rollback.sh
```

---

## ?? Contributing

*(Update your contributing section)*

### Development Workflow

1. **Create Feature Branch**
   ```bash
   git checkout -b feature/your-feature
   ```

2. **Develop & Test**
   ```bash
   dotnet build
   dotnet test
   ```

3. **Create Pull Request**
   - CI will automatically run
   - Ensure all checks pass
   - Request review

4. **Merge to Main**
   - Automatic Docker build
   - Ready for deployment

### CI/CD Guidelines

- ? All PRs must pass CI checks
- ? Use semantic versioning for releases
- ? Test locally before pushing
- ? Follow conventional commits
- ? Update documentation

---

## ?? License

*(Keep your existing license)*

---

## ?? Authors

*(Keep your existing authors)*

---

## ?? Acknowledgments

*(Add this if you want)*

- GitHub Actions for CI/CD automation
- Docker for containerization
- The .NET community

---

**Built with ?? using .NET 10.0 and modern DevOps practices**
