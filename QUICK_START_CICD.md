# ?? Quick Start Guide - CI/CD Setup

## ? 5-Minute Setup

### Step 1: Configure GitHub Secrets (2 minutes)

1. Go to your GitHub repository
2. Click **Settings** ? **Secrets and variables** ? **Actions**
3. Click **New repository secret** and add:

**Essential Secrets (Minimum):**
```
DOCKERHUB_USERNAME=your-username
DOCKERHUB_TOKEN=your-token
```

**For Deployment (Optional):**
```
STAGING_HOST=your-staging-server-ip
STAGING_USER=deploy
STAGING_SSH_KEY=<paste-your-private-key>
```

### Step 2: Push to Repository (1 minute)

```bash
git add .
git commit -m "Add CI/CD configuration"
git push origin developer_Nikita
```

### Step 3: Watch the Magic! (2 minutes)

1. Go to **Actions** tab in GitHub
2. See your workflow running automatically
3. Wait for ? green checkmark

---

## ?? What Happens Next?

### On Every Push
- ? Code is built
- ? Tests run (if available)
- ? Security scan performed
- ? Artifacts uploaded

### On Push to Main/Develop
- ? Docker image is built
- ? Image is pushed to registry
- ? Image is scanned for vulnerabilities

### On Manual Trigger
- ? Deploy to staging or production
- ? Automatic rollback on failure

---

## ?? Common Commands

### Build and Test Locally
```bash
dotnet restore
dotnet build
dotnet test
```

### Build Docker Image Locally
```bash
docker build -t steel-designer-engineer:local .
docker run -p 5000:80 steel-designer-engineer:local
```

### Test with Docker Compose Locally
```bash
docker-compose up -d
docker ps
curl http://localhost:5000/swagger/index.html
```

### Check Logs
```bash
docker logs steel-designer-engineer-api -f
```

### Stop Everything
```bash
docker-compose down
```

---

## ?? Troubleshooting

### Build Fails in GitHub Actions
1. Check the **Actions** tab for error details
2. Fix the error locally first:
   ```bash
   dotnet build
   ```
3. Push the fix

### Docker Build Fails
1. Test Docker build locally:
   ```bash
   docker build -t test .
   ```
2. Check Dockerfile syntax
3. Ensure all project files are included

### Can't Push to Registry
1. Verify GitHub token has `packages:write` permission
2. Check DOCKERHUB_USERNAME and DOCKERHUB_TOKEN are correct
3. Try logging in manually:
   ```bash
   docker login ghcr.io -u USERNAME -p TOKEN
   ```

---

## ?? Next Steps

1. **Add Tests** - Create unit tests for better CI
2. **Setup Staging Server** - Deploy to test environment
3. **Configure Monitoring** - Add health checks and alerts
4. **Setup Automatic Deployment** - Auto-deploy on tag creation

---

## ?? Full Documentation

For detailed information, see:
- [CICD_SETUP_GUIDE.md](./CICD_SETUP_GUIDE.md) - Complete setup guide
- [scripts/](./scripts/) - Deployment scripts

---

## ?? Pro Tips

1. **Use Branch Protection**
   - Require CI to pass before merging
   - Require pull request reviews

2. **Use Version Tags**
   ```bash
   git tag -a v1.0.0 -m "Release 1.0.0"
   git push origin v1.0.0
   ```

3. **Monitor Your Builds**
   - Add status badges to README
   - Setup notifications for failures

4. **Keep Secrets Safe**
   - Never commit `.env` files
   - Use GitHub Secrets for sensitive data
   - Rotate credentials regularly

---

## ? Checklist

- [ ] GitHub Secrets configured
- [ ] First build successful
- [ ] Docker image pushed to registry
- [ ] Environment files created (`.env.template` ? `.env`)
- [ ] Server prepared (if deploying)
- [ ] SSH keys configured (if deploying)
- [ ] Documentation read

---

**Need Help?** Check the full [CICD_SETUP_GUIDE.md](./CICD_SETUP_GUIDE.md) or open an issue!
