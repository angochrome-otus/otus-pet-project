# ? Pre-Flight Checklist - Before First CI/CD Run

## ?? Purpose
Use this checklist before triggering your first CI/CD pipeline run to ensure everything is properly configured.

---

## ?? Pre-Commit Checklist

### 1. Files Verification
- [ ] All 23+ CI/CD files are present
- [ ] Scripts have proper line endings (LF for Linux/Mac)
- [ ] No syntax errors in YAML files
- [ ] All documentation files are readable

### 2. Scripts Preparation (Linux/Mac)
```bash
# Make scripts executable
chmod +x scripts/*.sh

# Verify
ls -la scripts/*.sh
```

### 3. Environment Configuration
- [ ] `.env.template` is present
- [ ] `.env` is in `.gitignore`
- [ ] No secrets committed to repository
- [ ] `.dockerignore` properly configured

### 4. Git Configuration
- [ ] All CI/CD files staged
- [ ] Commit message follows convention
- [ ] Remote repository URL is correct
- [ ] Branch name matches workflows

---

## ?? GitHub Repository Setup

### 1. Repository Settings
- [ ] GitHub Actions is enabled
  ```
  Settings ? Actions ? General ? Allow all actions
  ```

### 2. Branch Protection (Optional but Recommended)
- [ ] Branch protection rules configured
  ```
  Settings ? Branches ? Add rule
  - Require pull request reviews
  - Require status checks to pass
  ```

### 3. Required Secrets (Minimum)
Navigate to: `Settings ? Secrets and variables ? Actions`

#### For Docker Hub (Optional)
- [ ] `DOCKERHUB_USERNAME` - Your Docker Hub username
- [ ] `DOCKERHUB_TOKEN` - Your Docker Hub access token

**How to create:**
```
1. Go to https://hub.docker.com/settings/security
2. Click "New Access Token"
3. Copy token and add to GitHub Secrets
```

#### For Deployment (Optional - if deploying)
- [ ] `STAGING_HOST` - Staging server IP/domain
- [ ] `STAGING_USER` - SSH username
- [ ] `STAGING_SSH_KEY` - Private SSH key content
- [ ] `STAGING_PORT` - SSH port (default: 22)

- [ ] `PROD_HOST` - Production server IP/domain
- [ ] `PROD_USER` - SSH username
- [ ] `PROD_SSH_KEY` - Private SSH key content
- [ ] `PROD_PORT` - SSH port (default: 22)

---

## ?? Docker Verification

### 1. Local Docker Test
```bash
# Build image
docker build -t steel-designer-engineer:test .

# Verify build succeeded
docker images | grep steel-designer-engineer

# Run container
docker run -d -p 5000:80 steel-designer-engineer:test

# Check health
curl http://localhost:5000/swagger/index.html

# Cleanup
docker stop $(docker ps -q --filter ancestor=steel-designer-engineer:test)
docker rmi steel-designer-engineer:test
```

### 2. Docker Compose Test
```bash
# Start services
docker-compose up -d

# Check status
docker-compose ps

# Verify all containers healthy
docker ps

# Check logs
docker-compose logs

# Test endpoint
curl http://localhost:5000/swagger/index.html

# Cleanup
docker-compose down
```

---

## ?? Workflow Configuration Verification

### 1. CI Workflow (`ci-build-test.yml`)
- [ ] .NET version matches project (10.0.x)
- [ ] Solution file path is correct
- [ ] Project paths are correct
- [ ] All referenced projects exist

### 2. Docker Workflow (`cd-docker.yml`)
- [ ] Registry URLs are correct
- [ ] Image names are correct
- [ ] Dockerfile path is correct
- [ ] Platforms are appropriate

### 3. Deploy Workflow (`cd-deploy.yml`)
- [ ] Environment names match your needs
- [ ] Deployment paths are correct
- [ ] SSH commands are appropriate
- [ ] Health check URLs are correct

---

## ?? File System Check

### 1. Required Files Exist
```bash
# Check all required files
cat <<EOF | while read file; do
.github/workflows/ci-build-test.yml
.github/workflows/cd-docker.yml
.github/workflows/cd-deploy.yml
.github/workflows/README.md
scripts/deploy.sh
scripts/backup.sh
scripts/rollback.sh
scripts/health-check.sh
scripts/health-check.ps1
scripts/mongo-init.js
scripts/make-executable.sh
scripts/README.md
nginx/nginx.conf
docker-compose.prod.yml
.env.template
CICD_INDEX.md
QUICK_START_CICD.md
CICD_SETUP_GUIDE.md
CICD_CHECKLIST.md
CICD_README.md
CICD_COMMANDS_CHEATSHEET.md
CICD_FILES.md
CICD_ARCHITECTURE_DIAGRAMS.md
CICD_IMPLEMENTATION_COMPLETE.md
CICD_FINAL_SUMMARY.md
EOF
  [ -f "$file" ] && echo "? $file" || echo "? $file MISSING"
done
```

### 2. File Permissions (Linux/Mac)
```bash
# Scripts should be executable
ls -l scripts/*.sh

# Should show: -rwxr-xr-x
```

---

## ?? Code Quality Check

### 1. Build Verification
```bash
# Restore packages
dotnet restore SteelDesignerEngineer/SteelDesignerEngineer.csproj

# Build project
dotnet build SteelDesignerEngineer/SteelDesignerEngineer.csproj

# Run tests (if exist)
dotnet test

# Check for warnings
echo "Check build output for warnings!"
```

### 2. Syntax Validation
```bash
# Validate YAML files
# Install yamllint if needed: pip install yamllint
yamllint .github/workflows/*.yml

# Or use online validator: https://www.yamllint.com/
```

---

## ?? Network & Connectivity

### 1. GitHub Connectivity
```bash
# Test GitHub connection
ssh -T git@github.com

# Should see: "Hi username! You've successfully authenticated"
```

### 2. Docker Registry Access
```bash
# Test GitHub Container Registry
echo $GITHUB_TOKEN | docker login ghcr.io -u $GITHUB_USERNAME --password-stdin

# Test Docker Hub (if using)
echo $DOCKERHUB_TOKEN | docker login -u $DOCKERHUB_USERNAME --password-stdin
```

### 3. Server Connectivity (if deploying)
```bash
# Test SSH to staging
ssh user@staging-server "echo 'Connection successful'"

# Test SSH to production
ssh user@prod-server "echo 'Connection successful'"
```

---

## ?? Documentation Review

### 1. Read Essential Docs
- [ ] Read `CICD_INDEX.md` (5 min)
- [ ] Read `QUICK_START_CICD.md` (5 min)
- [ ] Skim `CICD_SETUP_GUIDE.md` (15 min)
- [ ] Keep `CICD_COMMANDS_CHEATSHEET.md` handy

### 2. Team Communication
- [ ] Notify team about new CI/CD
- [ ] Share documentation links
- [ ] Schedule training session (optional)
- [ ] Update project wiki/docs

---

## ?? First Run Preparation

### 1. Branch Preparation
```bash
# Ensure you're on the right branch
git branch

# Should show: * developer_Nikita (or your branch)
```

### 2. Clean Working Directory
```bash
# Check for uncommitted changes
git status

# Should be clean or only show CI/CD files
```

### 3. Commit Message
```bash
# Use conventional commit format
git commit -m "feat: add complete CI/CD pipeline with documentation

- Add GitHub Actions workflows (CI, Docker, Deploy)
- Add deployment scripts (deploy, backup, rollback, health-check)
- Add Docker production configuration
- Add Nginx reverse proxy configuration
- Add comprehensive documentation (400+ pages)
- Add MongoDB initialization script

Closes #<issue-number>
"
```

---

## ?? Launch Checklist

### Before Push
- [ ] All checkboxes above are complete
- [ ] Files are committed
- [ ] Commit message is descriptive
- [ ] No secrets in repository
- [ ] Team is notified

### After Push
- [ ] Watch GitHub Actions tab
- [ ] Monitor build progress
- [ ] Check for errors
- [ ] Review workflow logs
- [ ] Verify artifacts creation

### First Build Expectations
```
Duration: 8-12 minutes (first build)
Subsequent builds: 5-8 minutes (with caching)

Stages:
1. CI Build (5-8 min)
2. Code Quality (1-2 min)
3. Artifacts Upload (1 min)
```

---

## ?? Troubleshooting Quick Reference

### Build Fails
1. Check GitHub Actions logs
2. Look for red ? indicators
3. Read error messages carefully
4. Check `CICD_SETUP_GUIDE.md` - Troubleshooting
5. Fix locally first: `dotnet build`

### Workflow Not Triggering
1. Check repository settings
2. Verify GitHub Actions is enabled
3. Check branch name in workflow file
4. Verify workflow file syntax

### Docker Build Fails
1. Test locally: `docker build -t test .`
2. Check Dockerfile syntax
3. Verify all files are in build context
4. Check `.dockerignore`

---

## ?? Pre-Flight Command Summary

```bash
# Complete pre-flight check script
echo "?? Starting CI/CD Pre-Flight Check..."

# 1. Make scripts executable
chmod +x scripts/*.sh
echo "? Scripts are executable"

# 2. Verify Docker
docker --version && docker-compose --version
echo "? Docker is available"

# 3. Test build
docker build -t test . && docker rmi test
echo "? Docker build successful"

# 4. Verify Git
git status
echo "? Git status checked"

# 5. List CI/CD files
echo "CI/CD Files:"
ls -1 .github/workflows/*.yml scripts/* CICD*.md

echo "?? Pre-flight check complete!"
echo "?? Next: Review QUICK_START_CICD.md"
echo "?? Then: git push origin developer_Nikita"
```

---

## ? Final Verification

### All Systems Go When:
- ? All checkboxes are checked
- ? Documentation is read
- ? Team is informed
- ? Secrets are configured (if needed)
- ? Local tests pass
- ? No uncommitted secrets
- ? Commit message is ready
- ? Backup of current state exists

---

## ?? Ready to Launch!

If all items above are checked, you're ready to:

```bash
git push origin developer_Nikita
```

Then:
1. Go to GitHub Actions tab
2. Watch your first CI/CD run
3. Celebrate! ??

---

## ?? If Something Goes Wrong

1. **Don't Panic!** ??
2. Check GitHub Actions logs
3. Review error messages
4. Consult `CICD_SETUP_GUIDE.md` - Troubleshooting
5. Ask for help in team chat
6. Create GitHub issue if needed

---

## ?? Tips for First Run

1. **Watch the build** - Learn what happens at each stage
2. **Take notes** - Document any issues you encounter
3. **Be patient** - First build takes longer (no cache yet)
4. **Learn from errors** - Each error teaches something
5. **Celebrate success** - CI/CD setup is a major achievement!

---

## ?? Learning Opportunity

Your first CI/CD run is a great learning experience:
- Observe the workflow stages
- Understand the build process
- See Docker image creation
- Learn about artifacts
- Experience automation in action

---

**Good luck with your first CI/CD run! ??**

**Remember:** You have 400+ pages of documentation to help you!

---

**Last Updated:** 2024  
**Checklist Version:** 1.0  
**Status:** Ready for Use ?
